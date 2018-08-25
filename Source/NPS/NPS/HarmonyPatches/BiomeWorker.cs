using Harmony;
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
					__result = 22.5f + (tile.temperature - 20f) * 2.2f + (tile.rainfall - 600f) / 100f;
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
				if (tile.rainfall < 700f)
				{
					__result = 0f;
					return;
				}
				__result = 15f + (tile.temperature - 7f) + (tile.rainfall - 700f) / 180f;
			}
		}
	}
}