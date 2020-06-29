using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TKKN_NPS
{
	public class Settings : ModSettings
	{
		public static Dictionary<string, IntVec3> wetCells;

		public static Dictionary<string, int> totalThings = new Dictionary<string, int>() {
			{ "TKKN_Lava_Spring", 0 },
			{ "TKKN_ColdSpring", 0 },
			{ "TKKN_HotSpring", 0 },
		};

		public static Dictionary<string, int> totalSpecialThings = new Dictionary<string, int>() {
			{ "TKKN_Total_Special_Elements", 0 },
			{ "TKKN_Total_Discovered_Elements", 0 },
			{ "TKKN_Total_Removed_Elements", 0 },
		};

		public static readonly Dictionary<string, Texture2D> customWeathers;

		public static bool spawnLavaOnlyInBiome = true;
		public static bool allowLavaEruption = true;
		public static bool allowPlantEffects = true;

		//terrain effects:
		public static bool affectsCold = true;
		public static bool affectsWet = true;
		public static bool affectsHot = true;

		public static bool showCold = true;
		public static bool showHot = true;
		public static bool showRain = true;

		public static bool allowPawnsToGetWet = true;
		public static bool allowPawnsSwim = true;
		public static bool doWeather = true;
		public static bool doDirtPath = true;
		public static bool regenCells = false;
		public static bool doTides = true;
		public static bool doSeasonalFloods = true;
		public static bool DoDisasterFloods = true;

		
		public static bool showDevReadout = false;

		public static bool showUpdateNotes = true;

		public static int cellBatchNumber = 100;
		public static string refCellBatchNumber = "";

		public static bool showTempOverlay {
			get {
				return Settings.showCold && Settings.showHot;
			}
		}
		static Settings()
		{
			
		}
		public void DoWindowContents(Rect inRect)
		{
			Listing_Standard list = new Listing_Standard(GameFont.Small) { ColumnWidth = inRect.width / 2 };
			list.Begin(inRect);

			//Performance Settings
			list.TextFieldNumericLabeled(
				"Cell processing batch size (default 100)",
				ref Settings.cellBatchNumber//,
											//				"Limit how many cells are processed every tick for weather affects."
				, ref Settings.refCellBatchNumber
				);
			list.CheckboxLabeled(
				"TKKN_doWeather_title".Translate(),
				ref Settings.doWeather,
				"TKKN_doWeather_text".Translate());
			list.CheckboxLabeled(
				"TKKN_showHot_title".Translate(),
				ref Settings.showHot,
				"TKKN_showHot_text".Translate());
			list.CheckboxLabeled(
				"TKKN_showCold_title".Translate(),
				ref Settings.showCold,
				"TKKN_showCold_text".Translate());
			list.CheckboxLabeled(
				"TKKN_showRain_title".Translate(),
				ref Settings.showRain,
				"TKKN_showRain_text".Translate());
			list.CheckboxLabeled(
				"TKKN_doTides_title".Translate(),
				ref Settings.doTides,
				"TKKN_doTides_text".Translate());
			list.Gap(12f);
			list.CheckboxLabeled(
				"TKKN_doDirtPath_title".Translate(),
				ref Settings.doDirtPath,
				"TKKN_doDirtPath_text".Translate());
			list.Gap(12f);
			

			//Game Play Settings


			list.CheckboxLabeled(
				"TKKN_allowLavaEruption_title".Translate(),
				ref Settings.allowLavaEruption,
				"TKKN_allowLavaEruption_text".Translate());
			list.CheckboxLabeled(
				"TKKN_spawnLavaOnlyInBiome_title".Translate(),
				ref Settings.spawnLavaOnlyInBiome,
				"TKKN_spawnLavaOnlyInBiome_text".Translate());
			list.CheckboxLabeled(
				"TKKN_allowPlantEffects_title".Translate(),
				ref Settings.allowPlantEffects,
				"TKKN_allowPlantEffects_text".Translate());
			list.CheckboxLabeled(
				"TKKN_allowPawnsToGetWet_title".Translate(),
				ref Settings.allowPawnsToGetWet,
				"TKKN_allowPawnsToGetWet_text".Translate());
			list.CheckboxLabeled(
				"TKKN_allowPawnsSwim_title".Translate(),
				ref Settings.allowPawnsSwim,
				"TKKN_allowPawnsToSwim_text".Translate());


			//Development stuff
			list.Gap(30f);
			
			list.CheckboxLabeled(
				"Show Update Notes?",
				ref Settings.showUpdateNotes,
				"");
			list.Gap(30f);
			list.CheckboxLabeled(
				"TKKN_regen_title".Translate(),
				ref Settings.regenCells,
				"TKKN_regen_text".Translate());

			list.CheckboxLabeled(
				"TKKN_showTempReadout_title".Translate(),
				ref Settings.showDevReadout,
				"TKKN_showTempReadout_text".Translate());

			

			list.End();
			
		}
		public override void ExposeData()
		{
			base.ExposeData();
			
			Scribe_Values.Look(ref Settings.doDirtPath, "doDirtPath", true, true);
			Scribe_Values.Look(ref Settings.showHot, "showHot", true, true);
			Scribe_Values.Look(ref Settings.showCold, "showCold", true, true);
			Scribe_Values.Look(ref Settings.showHot, "allowPlantEffects", true, true);
			Scribe_Values.Look(ref Settings.showRain, "showRain", true, true);
			Scribe_Values.Look(ref Settings.affectsWet, "affectsWet", true, true);
			Scribe_Values.Look(ref Settings.doTides, "doTides", true, true);
			Scribe_Values.Look(ref Settings.allowPawnsToGetWet, "allowPawnsToGetWet", true, true);
			Scribe_Values.Look(ref Settings.allowPawnsSwim, "allowPawnsSwim", true, true);
			Scribe_Values.Look(ref Settings.showDevReadout, "showDevReadout", false, true);
			Scribe_Values.Look(ref Settings.spawnLavaOnlyInBiome, "spawnLavaOnlyInBiome", false, true);
			Scribe_Values.Look(ref Settings.allowLavaEruption, "allowLavaEruption", true, true);
			Scribe_Values.Look(ref Settings.regenCells, "regenCells", true, true);
			

		}
	}
}