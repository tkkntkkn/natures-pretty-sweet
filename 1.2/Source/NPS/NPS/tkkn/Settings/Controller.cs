using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TKKN_NPS
{
	public class Controller : Mod
	{
		public static Settings settings;

		public Controller(ModContentPack content)
			: base(content)
		{
			settings = this.GetSettings<Settings>();
		}

		public override void DoSettingsWindowContents(Rect inRect)
		{
			settings.DoWindowContents(inRect);
		}

		public override string SettingsCategory()
		{
			return "Nature's Pretty Sweet";
		}

		// ReSharper disable once MissingXmlDoc
		public override void WriteSettings()
		{
			settings?.Write();

			if (Current.ProgramState != ProgramState.Playing)
			{
				return;
			}
			
		}
	}
}
