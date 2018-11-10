using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Harmony;
using Verse;


namespace TKKN_NPS
{
	//swap out plant graphics based on seasonal effects
	/*
	[HarmonyPatch(typeof(BiomeDef))]
	[HarmonyPatch("AllWildPlants", PropertyMethod.Getter)]
	public static class PatchAllWildPlants
	{
		//add special plants to wild plants
		[HarmonyPrefix]
		public static void Prefix(BiomeDef __instance)
		{
			BiomeSeasonalSettings biomeSettings = __instance.GetModExtension<BiomeSeasonalSettings>();
			if (biomeSettings != null && biomeSettings.plantsAdded == false)
			{
				Log.Error("updating wildPlants");
				List<BiomePlantRecord> wildPlants = Traverse.Create(__instance).Field("wildPlants").GetValue<List<BiomePlantRecord>>();
				List<BiomePlantRecord> specialPlants = biomeSettings.specialPlants;
				for (int i = 0; i < specialPlants.Count; i++)
				{
					wildPlants.Add(specialPlants[i]);
				}
				Traverse.Create(__instance).Field("wildPlants").SetValue(wildPlants);
				biomeSettings.plantsAdded = true;
			}
		}

	}

	[HarmonyPatch(typeof(BiomeDef))]
	[HarmonyPatch("CommonalityOfPlant")]
	public static class PatchCommonalityOfPlant
	{
		[HarmonyPrefix]
		public static void Prefix(BiomeDef __instance)
		{
			BiomeSeasonalSettings biomeSettings = __instance.GetModExtension<BiomeSeasonalSettings>();
			if (biomeSettings != null && biomeSettings.plantCacheUpdated == false)
			{
				Traverse.Create(__instance).Field("cachedPlantCommonalities").SetValue(null);
				biomeSettings.plantCacheUpdated = true;
			}
		}
	}
	*/

	[HarmonyPatch(typeof(BiomeDef))]
	[HarmonyPatch("CommonalityOfDisease")]
	public static class PatchCommonalityOfDisease
	{
		[HarmonyPrefix]
		public static void Prefix(BiomeDef __instance)
		{
			BiomeSeasonalSettings biomeSettings = __instance.GetModExtension<BiomeSeasonalSettings>();
			if (biomeSettings != null && biomeSettings.diseaseCacheUpdated == false)
			{
				// Log.Warning("updating cachedDiseaseCommonalities");
				Traverse.Create(__instance).Field("cachedDiseaseCommonalities").SetValue(null);
				biomeSettings.diseaseCacheUpdated = true;
			}
		}
	}	
}
