using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using MoonSharp.Interpreter;

namespace HealbotConfigurator
{
	public class HealbotData
	{
		public static string WindowerPath;
		private readonly string BuffListsLuaPath = "addons\\HealBot\\data\\buffLists.lua";
		private readonly string CurePotencyLuaPath = "addons\\HealBot\\data\\cure_potency.lua";
		private readonly string PrioritiesLuaPath = "addons\\HealBot\\data\\priorities.lua";

		public List<BuffList> BuffLists;
		public Dictionary<string, Dictionary<string, Dictionary<string, List<int>>>> CurePotencySets;
		public Dictionary<string, Dictionary<string, int>> PrioritySets;

		public Dictionary<string, Dictionary<string, int>> DefaultPrioritySets = new Dictionary<string, Dictionary<string, int>>
		{
			{"buffs", new Dictionary<string, int> {
				{"Afflatus Solace", 0},
				{"Composure", 0},
				{"Light Arts", 0},
				{"Haste", 1},
				{"Refresh", 1},
				{"Reraise", 2}
			}},
			{"debuffs", new Dictionary<string, int> {
				{"addle", 5},
				{"anemohelix", 6},
				{"bind", 1},
				{"bio", 3},
				{"blind", 6},
				{"break", 0},
				{"burn", 7},
				{"choke", 7},
				{"cryohelix", 6},
				{"dia", 3},
				{"distract", 5},
				{"drown", 7},
				{"frazzle", 1},
				{"frost", 7},
				{"geohelix", 6},
				{"gravity", 3},
				{"hydrohelix", 6},
				{"ionohelix", 6},
				{"luminohelix", 6},
				{"noctohelix", 6},
				{"paralyze", 4},
				{"poison", 6},
				{"pyrohelix", 6},
				{"rasp", 7},
				{"repose", 0},
				{"shock", 7},
				{"silence", 2},
				{"sleep", 0},
				{"slow", 4},
				{"stun", -1}
			}},
			{"dispel", new Dictionary<string, int> {
				{"accuracy_boost", 4},
				{"agi_boost", 5},
				{"aquaveil", 5},
				{"attack_boost", 3},
				{"ballad", 5},
				{"berserk", 3},
				{"blaze_spikes", 2},
				{"chr_boost", 5},
				{"damage_spikes", 2},
				{"defender", 4},
				{"defense_boost", 4},
				{"dex_boost", 5},
				{"dread_spikes", 1},
				{"enaero", 4},
				{"enblizzard", 4},
				{"enfire", 4},
				{"enstone", 4},
				{"enthunder", 4},
				{"enwater", 4},
				{"evasion_boost", 3},
				{"haste", 1},
				{"ice_spikes", 2},
				{"int_boost", 5},
				{"madrigal", 4},
				{"magic_atk_boost", 3},
				{"magic_def_boost", 1},
				{"mambo", 3},
				{"march", 1},
				{"minuet", 3},
				{"mnd_boost", 5},
				{"paeon", 4},
				{"phalanx", 4},
				{"protect", 4},
				{"refresh", 5},
				{"regen", 4},
				{"shell", 0},
				{"shock_spikes", 2},
				{"str_boost", 5},
				{"vit_boost", 5}
			}},
			{"jobs", new Dictionary<string, int> {
				{"blm", 2},
				{"blu", 2},
				{"brd", 4},
				{"bst", 3},
				{"cor", 3},
				{"dnc", 3},
				{"drg", 2},
				{"drk", 2},
				{"geo", 3},
				{"mnk", 1},
				{"nin", 1},
				{"pld", 1},
				{"pup", 2},
				{"rdm", 4},
				{"rng", 3},
				{"run", 1},
				{"sam", 2},
				{"sch", 2},
				{"smn", 3},
				{"thf", 2},
				{"war", 2},
				{"whm", 2}
			}},
			{"players", new Dictionary<string, int>() },
			{"status_removal", new Dictionary<string, int> {
				{"addle", 2},
				{"attack_down", 3},
				{"bind", 4},
				{"bio", 3},
				{"blind", 4},
				{"curse", 2},
				{"disease", 4},
				{"doom", 0},
				{"elegy", 1},
				{"paralysis", 1},
				{"petrification", 0},
				{"plague", 4},
				{"poison", 4},
				{"silence", 1},
				{"slow", 1},
				{"weight", 4},
			}},
		};

		public HealbotData(string windowerpath)
		{
			if (string.IsNullOrEmpty(windowerpath))
				return;

			WindowerPath = windowerpath;
			
			GetBuffLists();
			GetCurePotencySets();
			GetPrioritySets();
		}

		// BUFF LISTS
		private void GetBuffLists()
		{
			BuffLists = new List<BuffList>();
			var path = Path.Combine(WindowerPath, BuffListsLuaPath);
			string luaItems = File.ReadAllText(path);
			Script script = new Script();
			DynValue res = script.DoString(luaItems);
			var keys = new List<string>();
			foreach (DynValue dv in res.ToScalar().Table.Keys)
			{
				keys.Add(dv.ToString().Replace("\"", ""));
			}

			foreach (var key in keys)
			{
				var bl = new BuffList { Name = key };
				var bl_check = BuffLists.Where(x => x.Name == key).FirstOrDefault();
				if (bl_check != null)
					bl = bl_check;

				var dl = new Dictionary<string, List<string>>();
				foreach (var pair in res.ToScalar().Table.Get(key).Table.Pairs)
				{
					var innerKey = key;
					var tbl = pair.Value.Table;
					if (tbl != null)
					{
						innerKey = pair.Key.ToString().Replace("\"", "");
						var l = new List<string>();
						foreach (var iv in tbl.Values)
						{
							l.Add(iv.ToString().Replace("\"", ""));
						}
						dl.Add(innerKey, l);
					}
					else
					{
						innerKey = "me";
						var val = pair.Value.ToString().Replace("\"", "");
						if (bl.List.ContainsKey(innerKey))
						{
							bl.List[innerKey].Add(val);
						}
						else
						{
							dl.Add(innerKey, new List<string> { val });
						}
					}

					bl.List = dl;
				}

				BuffLists.Add(bl);
			}
		}

		private string GetBuffListsAsLua()
		{
			var newText = "return {" + Environment.NewLine;
			foreach (var blist in BuffLists)
			{
				//Console.WriteLine(blist.Name);
				newText += "    ['" + blist.Name + "'] = {" + Environment.NewLine;
				foreach (KeyValuePair<string, List<string>> ilist in blist.List)
				{
					//Console.WriteLine("  " + ilist.Key);
					if (ilist.Key != "me")
						newText += "        ['" + ilist.Key + "'] = {" + Environment.NewLine;

					foreach (var val in ilist.Value)
					{
						//Console.WriteLine("    " + val);
						if (ilist.Key != "me")
							newText += "            '" + val + "'," + Environment.NewLine;
						else
							newText += "        '" + val + "'," + Environment.NewLine;
					}

					if (ilist.Key != "me")
						newText += "        }," + Environment.NewLine;
				}
				newText += "    }," + Environment.NewLine;
			}
			newText += "}";

			return newText;
		}

		public bool SaveBuffList(BuffList list)
		{
			try
			{
				BuffLists.Add(list);

				SaveBuffLists();
			}
			catch
			{
				return false;
			}

			return true;
		}
				
		public bool RemoveBuffList(string name, string inner)
		{
			try
			{
				if (string.IsNullOrEmpty(inner))
				{
					var blist = BuffLists.Where(x => x.Name == name).FirstOrDefault();
					if (blist != null)
						BuffLists.Remove(blist);
				}
				else
				{
					var blist = BuffLists.Where(x => x.Name == name && x.List.ContainsKey(inner)).FirstOrDefault();
					if (blist != null)
					{
						blist.List.Remove(inner);
					}
				}

				SaveBuffLists();
			}
			catch
			{
				return false;
			}

			return true;
		}

		private void SaveBuffLists()
		{
			//make a backup if one doesn't already exists
			var file = Path.Combine(WindowerPath, BuffListsLuaPath);
			BackupFile(file);
			File.WriteAllText(file, GetBuffListsAsLua());
		}

		// CURE POTENCY SETS
		public void GetCurePotencySets()
		{
			CurePotencySets = CurePotencySets = new Dictionary<string, Dictionary<string, Dictionary<string, List<int>>>>();

			var path = Path.Combine(WindowerPath, CurePotencyLuaPath);
			string luaItems = File.ReadAllText(path);
			Script script = new Script();
			DynValue res = script.DoString(luaItems);

			var keys = new List<string>();
			foreach (DynValue dv in res.ToScalar().Table.Keys)
			{
				keys.Add(dv.ToString().Replace("\"", ""));
			}

			foreach (var key in keys)
			{
				CurePotencySets.Add(key, new Dictionary<string, Dictionary<string, List<int>>>());

				foreach (var pair in res.ToScalar().Table.Get(key).Table.Pairs)
				{
					var iKey = pair.Key.ToString().Replace("\"", "");
					if (!iKey.Contains("curaga") && !iKey.Contains("cure") && !iKey.Contains("waltz") && !iKey.Contains("waltzga"))
					{
						CurePotencySets[key].Add(iKey, new Dictionary<string, List<int>>());
						foreach (var inner in pair.Value.Table.Pairs)
						{
							var innerKey = inner.Key.ToString().Replace("\"", "");
							if (!CurePotencySets[key][iKey].ContainsKey(innerKey))
								CurePotencySets[key][iKey].Add(innerKey, new List<int>());

							foreach (var v in pair.Value.Table.Get(innerKey).Table.Values)
							{
								CurePotencySets[key][iKey][innerKey].Add(int.Parse(v.ToString()));
							}
						}
					}
					else
					{
						if (!CurePotencySets[key].ContainsKey(""))
						{
							CurePotencySets[key].Add("", new Dictionary<string, List<int>>());
						}
						CurePotencySets[key][""].Add(iKey, new List<int>());

						foreach (var v in pair.Value.Table.Values)
						{
							CurePotencySets[key][""][iKey].Add(int.Parse(v.ToString()));
						}
					}
				}
			}

			// Make sure it contains the 2 required sets: default & None
			AddCurePotencyDefaultSets();
		}

		private void AddCurePotencyDefaultSets()
		{
			if (!CurePotencySets.ContainsKey("default"))
			{
				CurePotencySets.Add("default", new Dictionary<string, Dictionary<string, List<int>>>());
				CurePotencySets["default"].Add("", new Dictionary<string, List<int>>());
				CurePotencySets["default"][""].Add("curaga", new List<int>
				{
					190,
					353,
					696,
					1225,
					1510
				});
				CurePotencySets["default"][""].Add("cure", new List<int>
				{
					110,
					250,
					550,
					1050,
					1210,
					1520
				});
				CurePotencySets["default"][""].Add("waltz", new List<int>
				{
					157,
					325,
					581,
					887,
					1156,
				});
				CurePotencySets["default"][""].Add("waltzga", new List<int>
				{
					160,
					521,
				});
			}

			if (!CurePotencySets.ContainsKey("None"))
			{
				CurePotencySets.Add("None", new Dictionary<string, Dictionary<string, List<int>>>());

				//WHM
				CurePotencySets["None"].Add("WHM", new Dictionary<string, List<int>>());
				CurePotencySets["None"]["WHM"].Add("curaga", new List<int>
				{
					190,
					353,
					696,
					1225,
					1510
				});
				CurePotencySets["None"]["WHM"].Add("cure", new List<int>
				{
					110,
					250,
					550,
					1050,
					1210,
					1520
				});
				CurePotencySets["None"]["WHM"].Add("waltz", new List<int>
				{
					157,
					325,
					581,
					887,
					1156,
				});
				CurePotencySets["None"]["WHM"].Add("waltzga", new List<int>
				{
					160,
					521,
				});
				
				// RDM
				CurePotencySets["None"].Add("RDM", new Dictionary<string, List<int>>());
				CurePotencySets["None"]["RDM"].Add("curaga", new List<int>
				{
					150,
					313,
					636,
					1125,
					1510
				});
				CurePotencySets["None"]["RDM"].Add("cure", new List<int>
				{
					94,
					207,
					469,
					880,
					1110,
					1395
				});
				CurePotencySets["None"]["RDM"].Add("waltz", new List<int>
				{
					157,
					325,
					581,
					887,
					1156,
				});
				CurePotencySets["None"]["RDM"].Add("waltzga", new List<int>
				{
					160,
					521,
				});
			}
		}

		public bool SaveCurePotencySets()
		{
			try
			{
				var newText = "return {" + Environment.NewLine;

				foreach (var kvp in CurePotencySets)
				{
					newText += "    ['" + kvp.Key + "'] = {" + Environment.NewLine;
					foreach (var jobKv in kvp.Value)
					{
						if (!string.IsNullOrEmpty(jobKv.Key))
							newText += "        ['" + jobKv.Key + "'] = {" + Environment.NewLine;

						foreach (var typeKv in jobKv.Value)
						{
							newText += "            ['" + typeKv.Key + "'] = {" + Environment.NewLine;
							foreach (var val in typeKv.Value)
							{
								newText += "                " + val + "," + Environment.NewLine;
							}
							newText += "            }," + Environment.NewLine;
						}

						if (!string.IsNullOrEmpty(jobKv.Key))
							newText += "        }," + Environment.NewLine;
					}
					newText += "    }," + Environment.NewLine;
				}
				newText += "}";

				//make a backup if one doesn't already exists
				var file = Path.Combine(WindowerPath, CurePotencyLuaPath);
				BackupFile(file);
				File.WriteAllText(file, newText);
			}
			catch
			{
				return false;
			}

			return true;
		}

		// PRIORITY SETS
		public void GetPrioritySets()
		{
			PrioritySets = new Dictionary<string, Dictionary<string, int>>();

			var path = Path.Combine(WindowerPath, PrioritiesLuaPath);
			string luaItems = File.ReadAllText(path);
			Script script = new Script();
			DynValue res = script.DoString(luaItems);

			foreach (var pair in res.ToScalar().Table.Pairs.Where(x => !x.Key.ToString().StartsWith("\"_")))
			{
				var keyStr = pair.Key.ToString().Replace("\"", "");
				var valStr = pair.Value.ToString().Replace("\"", "");

				if (!PrioritySets.ContainsKey(keyStr))
					PrioritySets.Add(keyStr, new Dictionary<string, int>());

				if (valStr.Contains("(Table)"))
				{
					foreach (var innerPair in pair.Value.Table.Pairs)
					{
						PrioritySets[keyStr].Add(innerPair.Key.ToString().Replace("\"", ""), int.Parse(innerPair.Value.ToString().Replace("\"", "")));
					}
				}
				else
				{
					PrioritySets[keyStr].Add(keyStr, int.Parse(valStr));
				}
			}

			// Add defaults if they weren't found in the lua.
			foreach (var defset in DefaultPrioritySets)
			{
				if (!PrioritySets.ContainsKey(defset.Key))
				{
					PrioritySets.Add(defset.Key, defset.Value);
				}
			}
		}

		public bool SavePrioritySets()
		{
			try
			{
				var newText = "return {" + Environment.NewLine + "    ['_comment'] = 'Lower number = higher priority'," + Environment.NewLine;
				foreach (var kvp in PrioritySets)
				{
					if (kvp.Key == "default")
					{
						//Console.WriteLine(kvp.Key + " = " + kvp.Value[kvp.Key]);
						newText += "    ['" + kvp.Key + "'] = " + kvp.Value[kvp.Key] + "," + Environment.NewLine;
						continue;
					}

					//Console.WriteLine(kvp.Key);
					newText += "    ['" + kvp.Key + "'] = {" + Environment.NewLine;
					foreach (var inner in kvp.Value)
					{
						//Console.WriteLine("  " + inner.Key + " = " + inner.Value);
						newText += "        ['" + inner.Key + "'] = " + inner.Value + "," + Environment.NewLine;
					}
					newText += "    }," + Environment.NewLine;
				}
				newText += "}";

				var file = Path.Combine(WindowerPath, PrioritiesLuaPath);
				BackupFile(file);
				File.WriteAllText(file, newText);
			}
			catch
			{
				return false;
			}

			return true;
		}

		// OTHER METHODS
		private void BackupFile(string file)
		{
			var backupFile = file + ".backup";
			if (File.Exists(file) && !File.Exists(backupFile))
				File.Copy(file, backupFile);
		}
	}

	public class BuffList
	{
		public string Name = string.Empty;
		public Dictionary<string, List<string>> List = new Dictionary<string, List<string>>();
	}
}
