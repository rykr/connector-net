using System;
using System.Resources;

namespace MySql.Data.Common
{
	internal class Resources
	{
		private static ResourceManager rm = null;

		public static string GetString(string name)
		{
			if (rm == null)
				rm = new ResourceManager("Strings", System.Reflection.Assembly.GetCallingAssembly());
			return rm.GetString (name);
		}
	}
}
