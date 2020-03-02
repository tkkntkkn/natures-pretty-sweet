using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

namespace TKKN_NPS.HarmonyPatches
{

	[HarmonyPatch(typeof(PlantUtility))]
	[HarmonyPatch("CanEverPlantAt")]
	class Patch_CanEverPlantAt_PlantUtility
	{
		[HarmonyPostfix]
		public static void Postfix(ThingDef plantDef, IntVec3 c, Map map, ref bool __result)
		{

			if (__result == true)
			{
				//verify that the plant can grow on this terrain.
				TerrainDef terrain = c.GetTerrain(map);
				ThingWeatherReaction weatherReaction = plantDef.GetModExtension<ThingWeatherReaction>();
				if (weatherReaction != null && weatherReaction.allowedTerrains != null)
				{
					if (!weatherReaction.allowedTerrains.Contains(terrain))
					{
						__result = false;
					}
					else
					{
//						Log.Warning("planting " + plantDef.defName);
					}
				}
			}

		}
	}
}
