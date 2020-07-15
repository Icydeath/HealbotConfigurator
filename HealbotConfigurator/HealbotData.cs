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
		public List<BuffList> BuffLists;

		public HealbotData(string windowerpath)
		{
			if (string.IsNullOrEmpty(windowerpath))
				return;

			WindowerPath = windowerpath;
			
			GetBuffLists();
		}

		private void GetBuffLists()
		{
			BuffLists = new List<BuffList>();

			var path = Path.Combine(WindowerPath, "addons\\Healbot\\data\\buffLists.lua");
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
	}

	public class BuffList
	{
		public string Name = string.Empty;
		public Dictionary<string, List<string>> List = new Dictionary<string, List<string>>();
	}
}
