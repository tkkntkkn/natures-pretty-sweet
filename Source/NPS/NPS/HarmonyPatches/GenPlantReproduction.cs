using Verse;
using RimWorld;
using Harmony;

namespace TKKN_NPS
{

	[HarmonyPatch(typeof(GenPlant))]
	[HarmonyPatch("CanEverPlantAt")]
	class PatchCanEverPlantAt
	{

		[HarmonyPostfix]
		public static void Postfix(ref bool __result, ThingDef plantDef, IntVec3 c, Map map)
		{
			if (__result == true && plantDef != null && c != null && map != null)
			{				
				BiomeDef biome = map.Biome;
				BiomeSeasonalSettings biomeSettings = biome.GetModExtension<BiomeSeasonalSettings>();
				if (biomeSettings != null)
				{
					if (!biomeSettings.canPutOnTerrain(c, plantDef, map))
					{
						__result = false;
					}
				}
			}
		}
	}
}
