using EliteMMO.API;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealbotConfigurator
{
	public class WindowerRes
	{
		private EliteAPI _EliteAPI;
		private readonly string WindowerPath = string.Empty;
		public List<ComboboxItem> Weaponskills { get; set; }
		
		public List<ComboboxItem> Spells { get; set; }
		public List<ComboboxItem> BuffSpells { get; set; }
		public List<ComboboxItem> BuffAbilities { get; set; }

		public Dictionary<byte, string> Jobs { get; set; }

		public WindowerRes(string windowerpath, EliteAPI api)
		{
			if (string.IsNullOrEmpty(windowerpath) || api == null)
				return;

			WindowerPath = windowerpath;
			_EliteAPI = api;

			SetupResources();
		}

		private void SetupResources()
		{
			SetupWeaponskills();
			SetupSpells();
			SetupBuffAbilities();
			SetupJobs();
		}

		private void SetupJobs()
		{
			Jobs = new Dictionary<byte, string>();
			var path = Path.Combine(WindowerPath, "res\\jobs.lua");
			string luaItems = File.ReadAllText(path);
			Script script = new Script();
			DynValue res = script.DoString(luaItems);
			foreach (DynValue dv in res.ToScalar().Table.Values)
			{
				var id = (double)dv.Table["id"];
				var job = (string)dv.Table["ens"];
				Jobs.Add((byte)id, job);
			}
		}

		private void SetupWeaponskills()
		{
			Weaponskills = new List<ComboboxItem>();
			var path = Path.Combine(WindowerPath, "res\\weapon_skills.lua");
			string luaItems = File.ReadAllText(path);
			Script script = new Script();
			DynValue res = script.DoString(luaItems);
			foreach (DynValue dv in res.ToScalar().Table.Values.Where(d => (double)d.Table["id"] < 241 && _EliteAPI.Player.HasWeaponSkill(uint.Parse(d.Table["id"].ToString())))) //.OrderBy(x => x.Table["id"])
			{
				Weaponskills.Add(new ComboboxItem {
					Text = (string)dv.Table["en"],
					Value = (double)dv.Table["id"]
				});
			}
		}

		private void SetupSpells()
		{
			var mainJobId = int.Parse(_EliteAPI.Player.MainJob.ToString());
			var mainJobLevel = int.Parse(_EliteAPI.Player.MainJobLevel.ToString());
			var subJobId = int.Parse(_EliteAPI.Player.SubJob.ToString());
			var subJobLevel = int.Parse(_EliteAPI.Player.SubJobLevel.ToString());

			Spells = new List<ComboboxItem>();
			BuffSpells = new List<ComboboxItem>();
			string luaItems = File.ReadAllText(Path.Combine(WindowerPath, @"res\spells.lua"));
			Script script = new Script();
			DynValue res = script.DoString(luaItems);
			foreach (DynValue dv in res.ToScalar().Table.Values
				.Where(d => d.Table["type"] != null
					&& d.Table["unlearnable"] == null
					&& (string)d.Table["type"] != "SummonerPact"
					&& (string)d.Table["type"] != "Trust"
					&& _EliteAPI.Player.HasSpell(uint.Parse(d.Table["id"].ToString()))
					&& ( ((Table)d.Table["levels"])[mainJobId] != null && (double)((Table)d.Table["levels"])[mainJobId] <= mainJobLevel || 
							 ((Table)d.Table["levels"])[subJobId] != null && (double)((Table)d.Table["levels"])[subJobId] <= subJobLevel )))
			{
				Spells.Add(new ComboboxItem
				{
					Text = (string)dv.Table["en"],
					Value = (double)dv.Table["id"]
				});

				if (dv.Table["status"] != null)
				{
					BuffSpells.Add(new ComboboxItem
					{
						Text = (string)dv.Table["en"],
						Value = (double)dv.Table["id"]
					});
				}
					
			}
		}

		private void SetupBuffAbilities()
		{
			string luaItems = File.ReadAllText(Path.Combine(WindowerPath, @"res\job_abilities.lua"));
			Script script = new Script();
			DynValue res = script.DoString(luaItems);
			foreach (DynValue dv in res.ToScalar().Table.Values
				.Where(d => d.Table["type"] != null 
				&& (string)d.Table["type"] != "BloodPactWard" && (string)d.Table["type"] != "Monster"
				&& d.Table["status"] != null
			))
			{
				BuffSpells.Add(new ComboboxItem
				{
					Text = (string)dv.Table["en"],
					Value = (double)dv.Table["id"]
				});
			}
		}
	}
}
