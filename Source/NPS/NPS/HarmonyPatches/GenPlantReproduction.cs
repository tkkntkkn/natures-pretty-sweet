using Verse;
using RimWorld;

namespace TKKN_NPS
{

	[HarmonyPatch(typeof(GenPlant))]
	[HarmonyPatch("CanEverPlantAt")]
	class PatchCanEverPlantAt
	{

		[HarmonyPostfix]
		public static void Postfix(Thing __instance, ref bool __result, this ThingDef plantDef, IntVec3 c, Map map)
		{
			if (__result == true)
			{
				BiomeDef biome = map.Biome;
				BiomeSeasonalSettings biomeSettings = biome.GetModExtension<BiomeSeasonalSettings>();
				if (biomeSettings != null)
				{
					if (!biomeSettings.canPutOnTerrain(c, thingDef, map))
					{
						__result = false;
					}
				}
			}
		}
	}
}
