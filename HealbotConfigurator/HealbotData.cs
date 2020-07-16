using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoonSharp.Interpreter;

namespace HealbotConfigurator
{
	public class HealbotData
	{
		public static string WindowerPath;
		private string BuffListsLuaPath = "addons\\HealBot\\data\\buffLists.lua";
		private string CurePotencyLuaPath = "addons\\HealBot\\data\\cure_potency.lua";
		private string PrioritiesLuaPath = "addons\\HealBot\\data\\priorities.lua";

		public List<BuffList> BuffLists;
		public Dictionary<string, Dictionary<string, Dictionary<string, List<int>>>> CurePotencySets;
		public Dictionary<string, Dictionary<string, int>> PrioritySets;

		public HealbotData(string windowerpath)
		{
			if (string.IsNullOrEmpty(windowerpath))
				return;

			WindowerPath = windowerpath;
			
			GetBuffLists();
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

		public bool SaveBuffList(BuffList list)
		{
			try
			{
				BuffLists.Add(list);

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

				//make a backup if one doesn't already exists
				var file = Path.Combine(WindowerPath, BuffListsLuaPath);
				var backupFile = Path.Combine(WindowerPath, BuffListsLuaPath.Replace("buffLists.lua", "buffLists.lua.backup"));
				if (File.Exists(file) && !File.Exists(backupFile))
					File.Copy(file, backupFile);

				//overwrite data/buffList.lua
				File.WriteAllText(file, newText);
			}
			catch
			{
				return false;
			}

			return true;
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
				// [Main Dictionary]
				CurePotencySets.Add(key, new Dictionary<string, Dictionary<string, List<int>>>());

				//setup the potencies main dict. VALUE [key, innerDict]
				foreach (var pair in res.ToScalar().Table.Get(key).Table.Pairs)
				{
					// [Inner Dictionary]
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
		}

		public bool SaveCurePotencySet()
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
				var backupFile = Path.Combine(WindowerPath, BuffListsLuaPath.Replace("cure_potency.lua", "cure_potency.lua.backup"));
				if (File.Exists(file) && !File.Exists(backupFile))
					File.Copy(file, backupFile);

				//overwrite data/buffList.lua
				File.WriteAllText(file, newText);
			}
			catch
			{
				return false;
			}

			return true;
		}

		// TODO: PRIORITY SETS
		public void GetPrioritySets()
		{
			PrioritySets = new Dictionary<string, Dictionary<string, int>>();

			var path = Path.Combine(WindowerPath, PrioritiesLuaPath);
			string luaItems = File.ReadAllText(path);
			Script script = new Script();
			DynValue res = script.DoString(luaItems);

			var keys = new List<string>();
			foreach (DynValue dv in res.ToScalar().Table.Keys)
			{
				keys.Add(dv.ToString().Replace("\"", ""));
			}

		}

		public void SavePrioritySet()
		{

		}
	}

	public class BuffList
	{
		public string Name = string.Empty;
		public Dictionary<string, List<string>> List = new Dictionary<string, List<string>>();
	}

	public class PrioritySet
	{
		//Category, <ActionName, PriorityLevel>
		public Dictionary<string, Dictionary<string, int>> Priorities = new Dictionary<string, Dictionary<string, int>>();
	}
}
