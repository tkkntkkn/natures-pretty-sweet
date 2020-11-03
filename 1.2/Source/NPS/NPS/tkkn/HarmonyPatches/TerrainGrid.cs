using System;
using System.Collections.Generic;
using HarmonyLib;
using Verse;
using TKKN_NPS.SaveData;
/*
namespace TKKN_NPS
{
	[HarmonyPatch(typeof(TerrainGrid))]
	[HarmonyPatch("SetTerrain")]
	public static class PatchSetTerrain
	{
		[HarmonyPostfix]
		public static void Postfix(TerrainGrid __instance, IntVec3 c, TerrainDef newTerr)
		{
			//add remove the terrain from my checks
			TerrainWeatherReactions weather = newTerr.GetModExtension<TerrainWeatherReactions>();
			if (weather != null)
			{
				Map map = Traverse.Create(__instance).Field("map").GetValue<Map>();// Traverse.Create<TerrainGrid>().Property("map").GetValue<Map>();
				
				if (map != null)
				{
					Watcher watcher = map.GetComponent<Watcher>();
					CellData cell = watcher.GetCell(c);
					if (cell != null)
					{
						cell.changedTerrain = newTerr;
					}
				}
			}
		}
	}
}
*/