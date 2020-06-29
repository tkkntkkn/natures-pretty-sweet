using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

namespace TKKN_NPS
{
	//load the extra plant graphics
	[HarmonyPatch(typeof(PlantUtility), "CanEverPlantAt")]
	public static class PostLoadCanEverPlantAt
	{
		[HarmonyPostfix]
		public static void Postfix(ThingDef plantDef, IntVec3 c, Map map, ref bool __result)
		{

			//verify that the plant can grow on this terrain.
			TerrainDef terrain = c.GetTerrain(map);
			ThingWeatherReaction weatherReaction = plantDef.GetModExtension<ThingWeatherReaction>();
			if (plantDef.defName == "TKKN_PlantBarnacles")
			{
				Log.Error(weatherReaction.ToString());
			}
			if (weatherReaction != null && weatherReaction.allowedTerrains != null)
			{
				if (!weatherReaction.allowedTerrains.Contains(terrain))
				{
					__result = false;
				}
				else
				{
					__result = true;
				}
			}

		}
	}
}
