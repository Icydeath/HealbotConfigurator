using EliteMMO.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealbotConfigurator
{
	public class ValidateMethods
	{
		private static EliteAPI _ELITEAPI;

		public ValidateMethods(EliteAPI hook)
		{
			if (hook != null)
				_ELITEAPI = hook;
		}

		public bool CanUseSpell()
		{
			return false;	
		}

		public bool CanUseAbility()
		{
			return false;
		}
	}
}
