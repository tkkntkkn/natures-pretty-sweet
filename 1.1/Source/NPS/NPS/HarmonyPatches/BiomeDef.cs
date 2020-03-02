using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using HarmonyLib;
using Verse;


namespace TKKN_NPS
{
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
