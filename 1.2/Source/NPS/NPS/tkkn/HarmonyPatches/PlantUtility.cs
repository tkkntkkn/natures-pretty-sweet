using Verse;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;

using System.Linq;
using UnityEngine;

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
												Log.Warning("planting " + plantDef.defName);
					}
				}
			}

		}
	}
}

/*
 * Can't figure out how to get this to work, but it doesn't seem to trigger on world gen. Figuring out something else.
namespace TKKN_NPS
{
	[HarmonyPatch(typeof(PlantUtility))]
	[HarmonyPatch("CanEverPlantAt")]
	static class PatchCanEverPlantAt
	{
		[HarmonyPostfix]
		public static void Postfix(this ThingDef plantDef, IntVec3 c, Map map, ref bool __result)
		{
			Log.Error("triggeredpost");
			TerrainDef terrain = c.GetTerrain(map);
			ThingWeatherReaction weatherReaction = plantDef.GetModExtension<ThingWeatherReaction>();
			if (plantDef.defName == "TKKN_PlantBarnacles")
			{
				Log.ErrorOnce("barnacles", 2);
				//				Log.Error(weatherReaction.ToString());
			}
			if (weatherReaction != null && weatherReaction.allowedTerrains != null)
			{
				if (!weatherReaction.allowedTerrains.Contains(terrain))
				{
					__result = false;
				}
			}
		}

		[HarmonyPrefix]
		public static bool Prefix(this ThingDef plantDef, IntVec3 c, Map map)
		{
			Log.Error("triggered");
			return true;
		}


		/*
		public static class PatchCalculatePlantsWhichCanGrowAt
		{
		//	CalculatePlantsWhichCanGrowAt(c, tmpPossiblePlants, cavePlants, plantDensity);

			[HarmonyPostfix]
			public static void Postfix(WildPlantSpawner __instance, IntVec3 c, List<ThingDef> outPlants, bool cavePlants, float plantDensity)
			{
				Log.ErrorOnce("triggered", 1);
				Map map = Traverse.Create(__instance).Field("map").GetValue<Map>();
				foreach (ThingDef plantDef in outPlants)
				{
					TerrainDef terrain = c.GetTerrain(map);
					ThingWeatherReaction weatherReaction = plantDef.GetModExtension<ThingWeatherReaction>();
					if (plantDef.defName == "TKKN_PlantBarnacles")
					{
						Log.ErrorOnce("barnacles", 2);
						//				Log.Error(weatherReaction.ToString());
					}
					if (weatherReaction != null && weatherReaction.allowedTerrains != null)
					{
						if (!weatherReaction.allowedTerrains.Contains(terrain))
						{
							outPlants.Remove(plantDef);
						}
					}
				}

			}
			}

			/*
				[HarmonyPostfix]
					public static void Postfix(ThingDef plantDef, IntVec3 c, Map map, ref bool __result)
					{
						Log.ErrorOnce("triggered", 1);
						//verify that the plant can grow on this terrain.
						TerrainDef terrain = c.GetTerrain(map);
						ThingWeatherReaction weatherReaction = plantDef.GetModExtension<ThingWeatherReaction>();
						if (plantDef.defName == "TKKN_PlantBarnacles")
						{
							Log.ErrorOnce("barnacles", 2);
			//				Log.Error(weatherReaction.ToString());
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
				*
	}
}
*/
