using System;
using System.Collections.Generic;
using System.Xml;
using Verse;

namespace TKKN_NPS
{
	class PatchOperationFindModByName : PatchOperation
	{
		private string mod;

		protected override bool ApplyWorker(XmlDocument xml)
		{
			bool hasMod = false;
			foreach (ModMetaData activeMod in ModLister.AllInstalledMods)
			{
				if (activeMod.Active == true && activeMod.Name == mod)
				{
					Watcher.modsPatched.Add(mod);
					hasMod = true;
					break;
				}
			}
			return hasMod;
		}
	}
}
