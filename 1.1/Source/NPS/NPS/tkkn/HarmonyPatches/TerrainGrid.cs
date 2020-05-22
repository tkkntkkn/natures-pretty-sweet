using System;
using System.Collections.Generic;
using HarmonyLib;
using Verse;

namespace TKKN_NPS
{
	[HarmonyPatch(typeof(TerrainGrid))]
	[HarmonyPatch("SetTerrain")]
	public static class PatchSetTerrain
	{
		[HarmonyPostfix]
		public static void Postfix(IntVec3 c, TerrainDef newTerr)
		{
			//add remove the terrain from my checks
			TerrainWeatherReactions weather = newTerr.GetModExtension<TerrainWeatherReactions>();
			if (weather != null)
			{
				Map map = Traverse.Create<TerrainGrid>().Property("map").GetValue<Map>();
				Watcher watcher = map.GetComponent<Watcher>();
				cellData cell = watcher.GetCell(c);
				if (cell.weather != null)
				{

				}
				else
				{

				}

				

			}
		}
	}
}
