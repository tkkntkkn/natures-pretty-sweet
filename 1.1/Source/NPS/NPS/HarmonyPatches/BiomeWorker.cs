using HarmonyLib;
using RimWorld;
using RimWorld.Planet;


namespace TKKN_NPS
{
	[HarmonyPatch(typeof(BiomeWorker_AridShrubland))]
	[HarmonyPatch("GetScore")]
	public static class PatchGetScoreArid
	{

		[HarmonyPostfix]
		public static void Postfix(Tile tile, float __result)
		{
			if (__result != -100f && __result != 0f) {
				if (tile.rainfall >= 1200f || tile.temperature < 23f)
				{
					__result = 0f;
				}
				else
				{
					__result = (float)(22.5 + (tile.temperature - 20.0) * 2.2000000476837158 + (tile.rainfall - 600.0) / 100.0);
				}
			}
		}
	}
	[HarmonyPatch(typeof(BiomeWorker_TemperateForest))]
	[HarmonyPatch("GetScore")]
	public static class PatchGetScoreTempForest
	{

		[HarmonyPostfix]
		public static void Postfix(Tile tile, float __result)
		{
			if (__result != -100f && __result != 0f)
			{
				if (tile.rainfall < 1200f)
				{
					__result = 0f;
					return;
				}
				__result = (float)(15.0 + (tile.temperature - 7.0) + (tile.rainfall - 600.0) / 180.0);
			}
		}
	}
}