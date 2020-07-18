using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HealbotConfigurator
{
	/// <summary>
	/// Interaction logic for CurePotencyWindow.xaml
	/// </summary>
	public partial class CurePotencyWindow : Window
	{
		public Dictionary<string, Dictionary<string, Dictionary<string, List<int>>>> ModifiedCurePotencySets { get; }

		private List<string> Jobs = new List<string>() {"WAR","MNK","WHM","BLM","RDM", "THF", "PLD", "DRK", "BST", "BRD", "RNG", "SAM", "NIN", "DRG", "SMN", "BLU", "COR", "PUP", "DNC", "SCH", "GEO", "RUN" };

		public CurePotencyWindow(Dictionary<string, Dictionary<string, Dictionary<string, List<int>>>> cps)
		{
			InitializeComponent();
			if (cps == null)
				ModifiedCurePotencySets = new Dictionary<string, Dictionary<string, Dictionary<string, List<int>>>>();

			ModifiedCurePotencySets = cps;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			Left = Application.Current.MainWindow.Left + 100;
			Top = Application.Current.MainWindow.Top + 100;
		}

		private void Window_ContentRendered(object sender, EventArgs e)
		{
			foreach (var job in Jobs)
				cb_NewSetJob.Items.Add(job);

			SetupSetsCombobox();
		}

		private void SetupSetsCombobox()
		{
			cb_CurePotencySetsJobs.Items.Clear();
			cb_CurePotencySets.Items.Clear();
			foreach (var set in ModifiedCurePotencySets.Keys)
			{
				cb_CurePotencySets.Items.Add(set);
			}
		}

		private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			e.Handled = new Regex("[^0-9]+").IsMatch(e.Text);
		}

		private void cb_CurePotencySets_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ClearPotencyTextBoxes();
			cb_CurePotencySetsJobs.Items.Clear();

			var set = cb_CurePotencySets.SelectedItem != null ? cb_CurePotencySets.SelectedItem.ToString() : string.Empty;
			if (!string.IsNullOrEmpty(set) && set != "default")
			{
				// setup jobs combobox
				cb_CurePotencySetsJobs.Items.Clear();
				foreach (var job in ModifiedCurePotencySets[set].Keys)
				{
					cb_CurePotencySetsJobs.Items.Add(job);
				}
				cb_CurePotencySetsJobs.Visibility = Visibility.Visible;
			}
			else if (!string.IsNullOrEmpty(set))
			{
				cb_CurePotencySetsJobs.Visibility = Visibility.Hidden;

				var job = string.Empty;
				if (ModifiedCurePotencySets[set].ContainsKey(job))
				{
					if (ModifiedCurePotencySets[set][job].ContainsKey("cure"))
					{
						txt_Cure1.Text = ModifiedCurePotencySets[set][job]["cure"][0].ToString();
						txt_Cure2.Text = ModifiedCurePotencySets[set][job]["cure"][1].ToString();
						txt_Cure3.Text = ModifiedCurePotencySets[set][job]["cure"][2].ToString();
						txt_Cure4.Text = ModifiedCurePotencySets[set][job]["cure"][3].ToString();
						txt_Cure5.Text = ModifiedCurePotencySets[set][job]["cure"][4].ToString();
						txt_Cure6.Text = ModifiedCurePotencySets[set][job]["cure"][5].ToString();
					}

					if (ModifiedCurePotencySets[set][job].ContainsKey("curaga"))
					{
						txt_Curaga1.Text = ModifiedCurePotencySets[set][job]["curaga"][0].ToString();
						txt_Curaga2.Text = ModifiedCurePotencySets[set][job]["curaga"][1].ToString();
						txt_Curaga3.Text = ModifiedCurePotencySets[set][job]["curaga"][2].ToString();
						txt_Curaga4.Text = ModifiedCurePotencySets[set][job]["curaga"][3].ToString();
						txt_Curaga5.Text = ModifiedCurePotencySets[set][job]["curaga"][4].ToString();
					}
					if (ModifiedCurePotencySets[set][job].ContainsKey("waltz"))
					{
						txt_Waltz1.Text = ModifiedCurePotencySets[set][job]["waltz"][0].ToString();
						txt_Waltz2.Text = ModifiedCurePotencySets[set][job]["waltz"][1].ToString();
					}
					if (ModifiedCurePotencySets[set][job].ContainsKey("waltzga"))
					{
						txt_Waltzga1.Text = ModifiedCurePotencySets[set][job]["waltzga"][0].ToString();
						txt_Waltzga2.Text = ModifiedCurePotencySets[set][job]["waltzga"][1].ToString();
					}
				}
			}
		}

		private void cb_CurePotencySetsJobs_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ClearPotencyTextBoxes();

			var set = cb_CurePotencySets.SelectedItem != null ? cb_CurePotencySets.SelectedItem.ToString() : string.Empty;
			if (!string.IsNullOrEmpty(set))
			{
				var job = cb_CurePotencySetsJobs.SelectedItem != null ? cb_CurePotencySetsJobs.SelectedItem.ToString() : string.Empty;
				if (ModifiedCurePotencySets[set].ContainsKey(job))
				{
					if (ModifiedCurePotencySets[set][job].ContainsKey("cure"))
					{
						txt_Cure1.Text = ModifiedCurePotencySets[set][job]["cure"][0].ToString();
						txt_Cure2.Text = ModifiedCurePotencySets[set][job]["cure"][1].ToString();
						txt_Cure3.Text = ModifiedCurePotencySets[set][job]["cure"][2].ToString();
						txt_Cure4.Text = ModifiedCurePotencySets[set][job]["cure"][3].ToString();
						txt_Cure5.Text = ModifiedCurePotencySets[set][job]["cure"][4].ToString();
						txt_Cure6.Text = ModifiedCurePotencySets[set][job]["cure"][5].ToString();
					}

					if (ModifiedCurePotencySets[set][job].ContainsKey("curaga"))
					{
						txt_Curaga1.Text = ModifiedCurePotencySets[set][job]["curaga"][0].ToString();
						txt_Curaga2.Text = ModifiedCurePotencySets[set][job]["curaga"][1].ToString();
						txt_Curaga3.Text = ModifiedCurePotencySets[set][job]["curaga"][2].ToString();
						txt_Curaga4.Text = ModifiedCurePotencySets[set][job]["curaga"][3].ToString();
						txt_Curaga5.Text = ModifiedCurePotencySets[set][job]["curaga"][4].ToString();
					}
					if (ModifiedCurePotencySets[set][job].ContainsKey("waltz"))
					{
						txt_Waltz1.Text = ModifiedCurePotencySets[set][job]["waltz"][0].ToString();
						txt_Waltz2.Text = ModifiedCurePotencySets[set][job]["waltz"][1].ToString();
					}
					if (ModifiedCurePotencySets[set][job].ContainsKey("waltzga"))
					{
						txt_Waltzga1.Text = ModifiedCurePotencySets[set][job]["waltzga"][0].ToString();
						txt_Waltzga2.Text = ModifiedCurePotencySets[set][job]["waltzga"][1].ToString();
					}
				}
			}
		}

		private void ClearPotencyTextBoxes()
		{
			txt_Cure1.Clear();
			txt_Cure2.Clear();
			txt_Cure3.Clear();
			txt_Cure4.Clear();
			txt_Cure5.Clear();
			txt_Cure6.Clear();

			txt_Curaga1.Clear();
			txt_Curaga2.Clear();
			txt_Curaga3.Clear();
			txt_Curaga4.Clear();
			txt_Curaga5.Clear();

			txt_Waltz1.Clear();
			txt_Waltz2.Clear();

			txt_Waltzga1.Clear();
			txt_Waltzga2.Clear();
		}

		private void Button_Save_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}

		private void TextBox_LostFocus(object sender, RoutedEventArgs e)
		{
			var tbox = (TextBox)sender;
			var cureType = tbox.Name.ToLower().Replace("txt_", "");
			
			var set = cb_CurePotencySets.SelectedItem != null ? cb_CurePotencySets.SelectedItem.ToString() : string.Empty;
			if (!string.IsNullOrEmpty(set))
			{
				var job = cb_CurePotencySetsJobs.SelectedItem != null ? cb_CurePotencySetsJobs.SelectedItem.ToString() : string.Empty;
				if (set == "default")
					job = "";
				
				if (ModifiedCurePotencySets[set].ContainsKey(job))
				{
					var val = !string.IsNullOrEmpty(tbox.Text) ? int.Parse(tbox.Text) : 0;
					if (cureType.Contains("cure") && ModifiedCurePotencySets[set][job].ContainsKey("cure"))
					{
						if (cureType.Contains("1"))
							ModifiedCurePotencySets[set][job]["cure"][0] = val;
						else if (cureType.Contains("2"))
							ModifiedCurePotencySets[set][job]["cure"][1] = val;
						else if (cureType.Contains("3"))
							ModifiedCurePotencySets[set][job]["cure"][2] = val;
						else if (cureType.Contains("4"))
							ModifiedCurePotencySets[set][job]["cure"][3] = val;
						else if (cureType.Contains("5"))
							ModifiedCurePotencySets[set][job]["cure"][4] = val;
						else if (cureType.Contains("6"))
							ModifiedCurePotencySets[set][job]["cure"][5] = val;
					}

					if (cureType.Contains("curaga") && ModifiedCurePotencySets[set][job].ContainsKey("curaga"))
					{
						if (cureType.Contains("1"))
							ModifiedCurePotencySets[set][job]["curaga"][0] = val;
						else if (cureType.Contains("2"))
							ModifiedCurePotencySets[set][job]["curaga"][1] = val;
						else if (cureType.Contains("3"))
							ModifiedCurePotencySets[set][job]["curaga"][2] = val;
						else if (cureType.Contains("4"))
							ModifiedCurePotencySets[set][job]["curaga"][3] = val;
						else if (cureType.Contains("5"))
							ModifiedCurePotencySets[set][job]["curaga"][4] = val;
					}

					if (cureType.Contains("waltz") && ModifiedCurePotencySets[set][job].ContainsKey("waltz"))
					{
						if (cureType.Contains("1"))
							ModifiedCurePotencySets[set][job]["waltz"][0] = val;
						else if (cureType.Contains("2"))
							ModifiedCurePotencySets[set][job]["waltz"][1] = val;
					}

					if (cureType.Contains("waltzga") && ModifiedCurePotencySets[set][job].ContainsKey("waltzga"))
					{
						if (cureType.Contains("1"))
							ModifiedCurePotencySets[set][job]["waltzga"][0] = val;
						else if (cureType.Contains("2"))
							ModifiedCurePotencySets[set][job]["waltzga"][1] = val;
					}
				}
			}
		}

		private void Button_AddNewSet_Click(object sender, RoutedEventArgs e)
		{
			var set = txt_NewSetName.Text;
			var job = cb_NewSetJob.SelectedItem != null ? cb_NewSetJob.SelectedItem.ToString() : string.Empty;
			if (string.IsNullOrEmpty(set) || string.IsNullOrEmpty(job))
				return;

			// get default to use as a base.
			var baseSettings = ModifiedCurePotencySets["default"][""];

			var checkForJob = string.Empty;

			var checkForSet = ModifiedCurePotencySets.Keys.Where(x => x.ToLower() == set.ToLower()).FirstOrDefault();
			if (checkForSet == null)
			{
				ModifiedCurePotencySets.Add(set, new Dictionary<string, Dictionary<string, List<int>>>());
			}
			else
			{
				set = checkForSet;
			}

			checkForJob = ModifiedCurePotencySets[set].Keys.Where(x => x.ToLower() == job.ToLower()).FirstOrDefault();
			if (checkForJob != null)
			{
				var msg = new CustomMessageDialog("ERROR", "Job for this set already exists. Modify the existing Set > Job.");
				msg.ShowDialog();

				SetComboboxesSelectedItems(set, job);
			}
			else
			{
				ModifiedCurePotencySets[set].Add(job, baseSettings);
				SetupSetsCombobox();
				SetComboboxesSelectedItems(set, job);
			}
			ClearAddFields();
		}

		private void SetComboboxesSelectedItems(string set, string job)
		{
			cb_CurePotencySets.SelectedItem = set;
			cb_CurePotencySetsJobs.SelectedItem = job;
		}

		private void ClearAddFields()
		{
			txt_NewSetName.Clear();
			cb_NewSetJob.Text = string.Empty;
		}

		private void Button_RemoveSet_Click(object sender, RoutedEventArgs e)
		{
			var set = cb_CurePotencySets.SelectedItem != null ? cb_CurePotencySets.SelectedItem.ToString() : string.Empty;
			var job = cb_CurePotencySetsJobs.SelectedItem != null ? cb_CurePotencySetsJobs.SelectedItem.ToString() : string.Empty;

			if (string.IsNullOrEmpty(set) && string.IsNullOrEmpty(job)) 
			{ return; }
			else if (string.IsNullOrEmpty(job) && (set == "default" || set == "None"))
			{
				var msg = new CustomMessageDialog("Denied", "You are not allowed to remove the following required sets: 'default', 'None'");
				msg.ShowDialog();
				return;
			}
			
			if (!string.IsNullOrEmpty(set) && string.IsNullOrEmpty(job)) 
			{
				// remove the whole set because a job wasn't selected.
				ModifiedCurePotencySets.Remove(set);
				SetupSetsCombobox();
			}
			else
			{
				// remove the job from the set.
				ModifiedCurePotencySets[set].Remove(job);
				SetupSetsCombobox();
				cb_CurePotencySets.SelectedItem = set;
			}

			ClearPotencyTextBoxes();
		}
	}
}
