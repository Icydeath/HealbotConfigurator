using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EliteMMO.API;
using HealbotConfigurator.Properties;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using SharpConfig;

namespace HealbotConfigurator
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : MetroWindow
	{
		public static EliteAPI _ELITEAPI;
		public static string AppPath;
		public static string ConfigPath;
		public MainWindow()
		{
			InitializeComponent();

			AppPath = AppDomain.CurrentDomain.BaseDirectory;
			ConfigPath = System.IO.Path.Combine(AppPath, "Configs");
			if (!Directory.Exists(ConfigPath))
			{
				Directory.CreateDirectory(ConfigPath);
			}

			SetupControls();
		}

		public static bool IsAdministrator()
		{
			return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
		}

		private void SetupControls()
		{
			IEnumerable<Process> pol = Process.GetProcessesByName("pol").Union(Process.GetProcessesByName("xiloader")).Union(Process.GetProcessesByName("edenxi"));
			if (pol.Count() < 1)
			{
				MessageBox.Show("FFXI not found");
				return;
			}

			for (int i = 0; i < pol.Count(); i++)
			{
				var ch = pol.ElementAt(i).MainWindowTitle;
				var cbi = new ComboboxItem()
				{
					Text = ch,
					Value = pol.ElementAt(i).Id
				};
				Select_HealbotPlayer.Items.Add(cbi);
				Select_AssistPlayer.Items.Add(cbi);
				Select_BuffPlayer.Items.Add(cbi);
				Select_FollowPlayer.Items.Add(cbi);
				Select_Weaponskill_Waitfor.Items.Add(cbi);
				Select_IgnorePlayer.Items.Add(cbi);
				Select_MonitorPlayer.Items.Add(cbi);
			}

			Select_Weaponskill_Operator.Items.Add(">");
			Select_Weaponskill_Operator.Items.Add("<");
			Select_Weaponskill_Operator.Items.Add("=");
			Select_Weaponskill_Operator.SelectedIndex = 0;
		}

		private void Select_HealbotPlayer_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (IsAdministrator())
			{
				var cb = (ComboBox)sender;
				var selected = (ComboboxItem)cb.SelectedItem;
				_ELITEAPI = new EliteAPI((int)selected.Value);

				FillLists();
				SetupDefaults();
			}
			else
			{
				var msg = new CustomMessageDialog("ERROR", "You must run this program as ADMINISTRATOR!");
				msg.ShowDialog();
				Select_HealbotPlayer.SelectedIndex = -1;
			}
		}

		private void FillLists(bool clearLists = false)
		{
			// Setup resource comboboxes ws/spells
			if (string.IsNullOrEmpty(Settings.Default.WindowerPath) || _ELITEAPI == null)
				return;

			if (clearLists)
			{
				Select_Weaponskill.Items.Clear();
				Select_SpamSpell.Items.Clear();
				Select_BuffSpell.Items.Clear();
				Select_DebuffSpell.Items.Clear();
			}

			var res = new WindowerRes(Settings.Default.WindowerPath, _ELITEAPI);
			foreach (var ws in res.Weaponskills)
			{
				Select_Weaponskill.Items.Add(ws);
			}
			foreach (var spell in res.Spells)
			{ 
				Select_SpamSpell.Items.Add(spell); 
			}
			foreach (var spell in res.BuffSpells)
			{
				Select_BuffSpell.Items.Add(spell);
				Select_DebuffSpell.Items.Add(spell);
			}
		}

		private void SetupDefaults()
		{
			Toggle_Buffs.IsOn = true;
			Toggle_Curing.IsOn = true;
			Toggle_Curaga.IsOn = true;
			Toggle_Debuffs.IsOn = true;
			Toggle_Erase.IsOn = true;
			Toggle_Na.IsOn = true;

			Toggle_Follow.IsOn = false;
			Toggle_Spam.IsOn = false;
			Toggle_Weaponskill.IsOn = false;
			Toggle_Weaponskill_Waitfor.IsOn = false;
		}

		private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
		{
			var toggleSwitch = sender as ToggleSwitch;
			if (toggleSwitch != null)
			{
				string cmd;
				if (toggleSwitch.IsOn == true)
				{
					// Enable feature
					cmd = GetCommandByControl(toggleSwitch.Name, true);
				}
				else
				{
					// Disable feature
					cmd = GetCommandByControl(toggleSwitch.Name);
				}
				SendCommand(cmd);
			}
		}

		private string GetCommandByControl(string controlname, bool on = false)
		{
			var cmd = "hb ";
			var type = on ? "enable" : "disable";

			var name = controlname.ToLower().Replace("toggle_", "");
			switch (name)
			{
				case "curing":
					cmd += type + " cure";
					break;

				case "ignoretrusts":
					cmd += name + " " + (on ? "on" : "off");
					break;

				case "assist":
					if (on)
					{
						var p = (Select_AssistPlayer.SelectedItem != null ? ((ComboboxItem)Select_AssistPlayer.SelectedItem).Text : Select_AssistPlayer.Text);
						if (!string.IsNullOrEmpty(p) && p.ToLower() != _ELITEAPI.Player.Name.ToLower())
						{
							cmd += "assist " + p + "";
						}
						else
						{
							Toggle_Assist.IsOn = false;
							return "";
						}
					}
					else
					{
						cmd += "assist off";
					}
					break;

				case "attack":
					cmd += "assist attack " + (on ? "on" : "off");
					break;

				case "follow":
					if (on)
					{
						var p = (Select_FollowPlayer.SelectedItem != null ? ((ComboboxItem)Select_FollowPlayer.SelectedItem).Text : Select_FollowPlayer.Text);
						if (!string.IsNullOrEmpty(p) && p.ToLower() != _ELITEAPI.Player.Name.ToLower())
						{
							cmd += "follow " + p + ";hb follow distance " + Num_Follow.Value;
						}
						else
						{
							Toggle_Follow.IsOn = false;
							return "";
						}
					}
					else
						cmd += "follow off";
					break;

				case "weaponskill_waitfor":
					if (on)
					{
						var p = !string.IsNullOrEmpty(Select_Weaponskill_Waitfor.Text) ? Select_Weaponskill_Waitfor.Text : Select_Weaponskill_Waitfor.SelectedItem != null ? ((ComboboxItem)Select_Weaponskill_Waitfor.SelectedItem).Text : "";
						cmd += "ws waitfor " + p + " " + Num_Weaponskill_Waitfor.Value; 
					}
					else
						cmd += "ws nopartner";
					break;
				case "spam":
					if (on)
					{
						var spell = (!string.IsNullOrEmpty(Select_SpamSpell.Text) ? Select_SpamSpell.Text : Select_SpamSpell.SelectedItem != null ? ((ComboboxItem)Select_SpamSpell.SelectedItem).Text : "");
						if (!string.IsNullOrEmpty(spell))
							cmd += name + " " + spell + ";hb " + type + " " + name;
						else
							cmd += type + " " + name;
					}
					else
						cmd += type + " " + name;

					break;

				default:
					cmd += type + " " + name;
					if (name == "weaponskill" && on) // && Toggle_Assist.IsOn
					{
						var ws = (!string.IsNullOrEmpty(Select_Weaponskill.Text) ? Select_Weaponskill.Text : Select_Weaponskill.SelectedItem != null ? ((ComboboxItem)Select_Weaponskill.SelectedItem).Text : "");
						if (!string.IsNullOrEmpty(ws))
							cmd += ";hb ws use " + ws;
						cmd += ";hb ws hp " + ((string)Select_Weaponskill_Operator.SelectedItem) + " " + Num_Weaponskill_Percent.Value;
					}
					break;
			}

			return cmd;
		}

		private void Button_LaunchWindowerPathDialog_Click(object sender, RoutedEventArgs e)
		{
			var inputDialog = new CustomInputDialog("Windower Path:", Settings.Default.WindowerPath);
			if (inputDialog.ShowDialog() == true && !string.IsNullOrEmpty(inputDialog.DialogValue) && Directory.Exists(inputDialog.DialogValue))
			{
				Settings.Default.WindowerPath = inputDialog.DialogValue.Trim();
				Settings.Default.Save();
				FillLists(true);
			}
		}

		private void SaveSettings()
		{
			var inputDialog = new CustomInputDialog("Save as...", "default");
			if (inputDialog.ShowDialog() == true)
			{
				var fileName = inputDialog.DialogValue.Replace(".txt", "") + ".txt";
				if (string.IsNullOrEmpty(fileName))
					fileName = "default.txt";

				var config = new Configuration();
				config["Features"]["Curing"].BoolValue = Toggle_Curing.IsOn;
				config["Features"]["Curaga"].BoolValue = Toggle_Curaga.IsOn;
				config["Features"]["Na"].BoolValue = Toggle_Na.IsOn;
				config["Features"]["Erase"].BoolValue = Toggle_Erase.IsOn;
				config["Features"]["Buffs"].BoolValue = Toggle_Buffs.IsOn;
				config["Features"]["Enfeebling"].BoolValue = Toggle_Debuffs.IsOn;
				config["Features"]["IgnoreTrusts"].BoolValue = Toggle_IgnoreTrusts.IsOn;
				config["Features"]["Mincure"].IntValue = int.Parse(Num_Mincure.Value.ToString());

				config["Follow"]["FollowPlayer"].StringValue = Select_FollowPlayer.SelectedItem != null ? ((ComboboxItem)Select_FollowPlayer.SelectedItem).Text : !string.IsNullOrEmpty(Select_FollowPlayer.Text) ? Select_FollowPlayer.Text : string.Empty;
				config["Follow"]["FollowDistance"].FloatValue = float.Parse(Num_Follow.Value.ToString());

				var list = new List<string>();
				foreach(var b in Lb_Buffs.Items)
				{
					list.Add(b.ToString());
				}
				config["Buffs"]["BuffSpells"].StringValueArray = list.ToArray();

				list = new List<string>();
				foreach (var b in Lb_Debuffs.Items)
				{
					list.Add(b.ToString());
				}
				config["Enfeebling"]["EnfeeblingSpells"].StringValueArray = list.ToArray();

				list = new List<string>();
				foreach (var b in Lb_MonitorPlayer.Items)
				{
					list.Add(b.ToString());
				}
				config["Monitor"]["MonitorPlayers"].StringValueArray = list.ToArray();

				list = new List<string>();
				foreach (var b in Lb_IgnorePlayer.Items)
				{
					list.Add(b.ToString());
				}
				config["Ignore"]["IgnorePlayers"].StringValueArray = list.ToArray();


				config["Assist"]["AssistPlayer"].StringValue = Select_AssistPlayer.SelectedItem != null ? ((ComboboxItem)Select_AssistPlayer.SelectedItem).Text : !string.IsNullOrEmpty(Select_AssistPlayer.Text) ? Select_AssistPlayer.Text : string.Empty;
				config["Assist"]["EngageTarget"].BoolValue = Toggle_Attack.IsOn;

				config["Weaponskill"]["WS"].StringValue = Select_Weaponskill.SelectedItem != null ? ((ComboboxItem)Select_Weaponskill.SelectedItem).Text : !string.IsNullOrEmpty(Select_Weaponskill.Text) ? Select_Weaponskill.Text : string.Empty;
				config["Weaponskill"]["TargetHP"].IntValue = int.Parse(Num_Weaponskill_Percent.Value.ToString());
				config["Weaponskill"]["HPOperator"].StringValue = Select_Weaponskill_Operator.SelectedItem.ToString();
				config["Weaponskill"]["WaitForPlayer"].StringValue = Select_Weaponskill_Waitfor.SelectedItem != null ? ((ComboboxItem)Select_Weaponskill_Waitfor.SelectedItem).Text : !string.IsNullOrEmpty(Select_Weaponskill_Waitfor.Text) ? Select_Weaponskill_Waitfor.Text : string.Empty;
				config["Weaponskill"]["WaitForTP"].IntValue = int.Parse(Num_Weaponskill_Waitfor.Value.ToString());

				config["Spam"]["SpamSpell"].StringValue = Select_SpamSpell.SelectedItem != null ? ((ComboboxItem)Select_SpamSpell.SelectedItem).Text : !string.IsNullOrEmpty(Select_SpamSpell.Text) ? Select_SpamSpell.Text : string.Empty;

				config.SaveToFile(System.IO.Path.Combine(ConfigPath, fileName));
			}

		}

		private void LoadSettings()
		{
			if (_ELITEAPI == null)
			{
				var msg = new CustomMessageDialog("ERROR", "You must select a character before loading configuration settings.");
				msg.ShowDialog();
				return;
			}	

			OpenFileDialog openFileDialog = new OpenFileDialog
			{
				Filter = "Text files (*.txt)|*.txt",
				InitialDirectory = ConfigPath
			};

			if (openFileDialog.ShowDialog() == true)
			{
				SendCommand("hb reload");
				Thread.Sleep(1000);

				var config = Configuration.LoadFromFile(openFileDialog.FileName);

				Toggle_Curing.IsOn = config["Features"]["Curing"].BoolValue;
				Toggle_Curaga.IsOn = config["Features"]["Curaga"].BoolValue;
				Toggle_Na.IsOn = config["Features"]["Na"].BoolValue;
				Toggle_Erase.IsOn = config["Features"]["Erase"].BoolValue;
				Toggle_Buffs.IsOn = config["Features"]["Buffs"].BoolValue;
				Toggle_Debuffs.IsOn = config["Features"]["Enfeebling"].BoolValue;
				Toggle_IgnoreTrusts.IsOn = config["Features"]["IgnoreTrusts"].BoolValue;
				Num_Mincure.Value = config["Features"]["Mincure"].IntValue;

				Select_FollowPlayer.Text = config["Follow"]["FollowPlayer"].StringValue;
				Num_Follow.Value = config["Follow"]["FollowDistance"].FloatValue;

				Lb_Buffs.Items.Clear();
				foreach (var txt in config["Buffs"]["BuffSpells"].StringValueArray)
				{
					Lb_Buffs.Items.Add(txt);
				}

				Lb_Debuffs.Items.Clear();
				foreach (var txt in config["Enfeebling"]["EnfeeblingSpells"].StringValueArray)
				{
					Lb_Debuffs.Items.Add(txt);
				}

				Lb_MonitorPlayer.Items.Clear();
				foreach (var txt in config["Monitor"]["MonitorPlayers"].StringValueArray)
				{
					Lb_MonitorPlayer.Items.Add(txt);
				}

				Lb_IgnorePlayer.Items.Clear();
				foreach (var txt in config["Ignore"]["IgnorePlayers"].StringValueArray)
				{
					Lb_IgnorePlayer.Items.Add(txt);
				}

				Select_AssistPlayer.Text = config["Assist"]["AssistPlayer"].StringValue;
				Toggle_Attack.IsOn = config["Assist"]["EngageTarget"].BoolValue;

				Select_Weaponskill.Text = config["Weaponskill"]["WS"].StringValue;
				Num_Weaponskill_Percent.Value = config["Weaponskill"]["TargetHP"].IntValue;
				Select_Weaponskill_Operator.Text = config["Weaponskill"]["HPOperator"].StringValue;
				Select_Weaponskill_Waitfor.Text = config["Weaponskill"]["WaitForPlayer"].StringValue;
				Num_Weaponskill_Waitfor.Value = config["Weaponskill"]["WaitForTP"].IntValue;

				Select_SpamSpell.Text = config["Spam"]["SpamSpell"].StringValue;
			}
		}

		private void SendCommand(string cmd)
		{
			if (_ELITEAPI != null)
			{
				_ELITEAPI.ThirdParty.SendString(@"//" + cmd);
				//_ELITEAPI.ThirdParty.SendString(@"/echo " + cmd);
			}
			//else
			//{
			//	var msg = new CustomMessageDialog("ERROR", "Player instance not found. Unable to send command:" + Environment.NewLine + cmd);
			//	msg.ShowDialog();
			//}
		}

		private void Button_SaveSettings_Click(object sender, RoutedEventArgs e)
		{
			SaveSettings();
		}

		private void Button_LoadSettings_Click(object sender, RoutedEventArgs e)
		{
			LoadSettings();
		}

		private void Button_HealbotOnReload_Click(object sender, RoutedEventArgs e)
		{
			SendCommand("hb reload");
		}

		private void Button_HealbotRefresh_Click(object sender, RoutedEventArgs e)
		{
			SendCommand("hb refresh");
		}

		private void Button_HealbotOn_Click(object sender, RoutedEventArgs e)
		{
			SendCommand("hb on");
		}

		private void Button_HealbotOff_Click(object sender, RoutedEventArgs e)
		{
			SendCommand("hb off");
		}

		private void Button_HealbotLoad_Click(object sender, RoutedEventArgs e)
		{
			SendCommand("lua load healbot");
		}

		private void Button_HealbotUnload_Click(object sender, RoutedEventArgs e)
		{
			SendCommand("lua unload healbot");
		}

		private void Button_AddBuff_Click(object sender, RoutedEventArgs e)
		{
			var player = Select_BuffPlayer.SelectedItem != null ? ((ComboboxItem)Select_BuffPlayer.SelectedItem).Text : !string.IsNullOrEmpty(Select_BuffPlayer.Text) ? Select_BuffPlayer.Text : string.Empty;
			var buff = Select_BuffSpell.SelectedItem != null ? ((ComboboxItem)Select_BuffSpell.SelectedItem).Text : !string.IsNullOrEmpty(Select_BuffSpell.Text) ? Select_BuffSpell.Text : string.Empty;
			if (!string.IsNullOrEmpty(player) && !string.IsNullOrEmpty(buff))
			{
				var hasItem = false;
				foreach (var item in Lb_Buffs.Items)
				{
					if (item.ToString() == player + " " + buff)
					{
						hasItem = true;
						break;
					}
				}

				if (!hasItem)
				{
					Lb_Buffs.Items.Add(player + " " + buff);
					var cmd = "hb buff " + player + " " + buff;
					SendCommand(cmd);
				}
			}
		}

		private void Button_AddDebuffSpell_Click(object sender, RoutedEventArgs e)
		{
			var debuff = Select_DebuffSpell.SelectedItem != null ? ((ComboboxItem)Select_DebuffSpell.SelectedItem).Text : !string.IsNullOrEmpty(Select_DebuffSpell.Text) ? Select_DebuffSpell.Text : string.Empty;
			if (!string.IsNullOrEmpty(debuff))
			{
				var hasItem = false;
				foreach (var item in Lb_Debuffs.Items)
				{
					if (item.ToString() == debuff)
					{
						hasItem = true;
						break;
					}
				}

				if (!hasItem)
				{
					Lb_Debuffs.Items.Add(debuff);
					var cmd = "hb debuff " + debuff;
					SendCommand(cmd);
				}
			}
		}

		private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			_ELITEAPI = null;
		}

		private void Lb_Buffs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			var playerbuff = Lb_Buffs.SelectedItem != null ? Lb_Buffs.SelectedItem.ToString() : string.Empty;
			if (!string.IsNullOrEmpty(playerbuff))
			{
				Lb_Buffs.Items.Remove(Lb_Buffs.SelectedItem);
				SendCommand("hb cancelbuff " + playerbuff);
			}
		}

		private void Lb_Debuffs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			var debuff = Lb_Debuffs.SelectedItem != null ? Lb_Debuffs.SelectedItem.ToString() : string.Empty;
			if (!string.IsNullOrEmpty(debuff))
			{
				Lb_Debuffs.Items.Remove(Lb_Debuffs.SelectedItem);
				SendCommand("hb debuff remove " + debuff);
			}
		}

		private void Num_Mincure_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
		{
			SendCommand("hb mincure " + Num_Mincure.Value);
		}

		private void Button_IgnorePlayer_Click(object sender, RoutedEventArgs e)
		{
			var sel = Select_IgnorePlayer.SelectedItem != null ? ((ComboboxItem)Select_IgnorePlayer.SelectedItem).Text : !string.IsNullOrEmpty(Select_IgnorePlayer.Text) ? Select_IgnorePlayer.Text : string.Empty;
			if (!string.IsNullOrEmpty(sel))
			{
				var hasItem = false;
				foreach (var item in Lb_IgnorePlayer.Items)
				{
					if (item.ToString() == sel)
					{
						hasItem = true;
						break;
					}
				}

				if (!hasItem)
				{
					Lb_IgnorePlayer.Items.Add(sel);
					var cmd = "hb ignore " + sel;
					SendCommand(cmd);
				}
			}
		}

		private void Lb_IgnorePlayer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			var p = Lb_IgnorePlayer.SelectedItem != null ? Lb_IgnorePlayer.SelectedItem.ToString() : string.Empty;
			if (!string.IsNullOrEmpty(p))
			{
				Lb_IgnorePlayer.Items.Remove(Lb_IgnorePlayer.SelectedItem);
				SendCommand("hb unignore " + p);
			}
		}

		private void Button_MonitorPlayer_Click(object sender, RoutedEventArgs e)
		{
			var sel = Select_MonitorPlayer.SelectedItem != null ? ((ComboboxItem)Select_MonitorPlayer.SelectedItem).Text : !string.IsNullOrEmpty(Select_MonitorPlayer.Text) ? Select_MonitorPlayer.Text : string.Empty;
			if (!string.IsNullOrEmpty(sel))
			{
				var hasItem = false;
				foreach (var item in Lb_MonitorPlayer.Items)
				{
					if (item.ToString() == sel)
					{
						hasItem = true;
						break;
					}
				}

				if (!hasItem)
				{
					Lb_MonitorPlayer.Items.Add(sel);
					var cmd = "hb watch " + sel;
					SendCommand(cmd);
				}
			}
		}

		private void Lb_MonitorPlayer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			var p = Lb_MonitorPlayer.SelectedItem != null ? Lb_MonitorPlayer.SelectedItem.ToString() : string.Empty;
			if (!string.IsNullOrEmpty(p))
			{
				Lb_MonitorPlayer.Items.Remove(Lb_MonitorPlayer.SelectedItem);
				SendCommand("hb unwatch " + p);
			}
		}

		private void Button_ResetBuffs_Click(object sender, RoutedEventArgs e)
		{
			SendCommand("hb reset buffs");
		}

		private void Button_ResetDebuffs_Click(object sender, RoutedEventArgs e)
		{
			SendCommand("hb reset debuffs");
		}
	}
}
