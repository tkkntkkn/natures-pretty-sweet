using Harmony;
using RimWorld;
using RimWorld.Planet;
using Verse;


namespace TKKN_NPS
{
	[HarmonyPatch(typeof(DropCellFinder))]
	[HarmonyPatch("DropCellFinder")]
	public static class PatchDropCellFinder
	{

		[HarmonyPrefix]
		public static bool Prefix(Map map, IntVec3 __result)
		{
			///Predicate<IntVec3> validator, Map map, float roadChance, out IntVec3 result
			// don't drop on deep water
			__result = CellFinderLoose.RandomCellWith((IntVec3 c) => c.Standable(map) && !c.Roofed(map) && !c.Fogged(map) && !c.GetTerrain(map).HasTag("TKKN_Swim"), map, 1000);
			return false;
		}
	}
}
