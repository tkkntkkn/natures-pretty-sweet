using Harmony;
using RimWorld;
using Verse;
using UnityEngine;

namespace TKKN_NPS
{


	[HarmonyPatch(typeof(DateNotifier))]
	[HarmonyPatch("DateNotifierTick")]
	class PatchDateNotifierTick
	{
		static void Prefix(DateNotifier __instance)
		{
			Map map = Find.VisibleMap;
			BiomeSeasonalSettings biomeSettings = map.Biome.GetModExtension<BiomeSeasonalSettings>();
			if (biomeSettings != null)
			{
				Vector2 location = Find.WorldGrid.LongLatOf(map.Tile);
				Season season = GenDate.Season((long)Find.TickManager.TicksAbs, location);

				if (biomeSettings.lastChanged != season)
				{
					biomeSettings.setWeatherBySeason(map, season);
					biomeSettings.setDiseaseBySeason(season);
					biomeSettings.setIncidentsBySeason(season);
					biomeSettings.lastChanged = season;
				}
				return;
			}

		}
	}
}
