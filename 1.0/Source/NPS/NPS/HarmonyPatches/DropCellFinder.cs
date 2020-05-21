using Harmony;
using RimWorld;
using RimWorld.Planet;
using Verse;


namespace TKKN_NPS
{
	[HarmonyPatch(typeof(DropCellFinder))]
	[HarmonyPatch("RandomDropSpot")]
	public static class PatchRandomDropSpot
	{

		[HarmonyPrefix]
		public static bool Prefix(Map map, IntVec3 __result)
		{
			///Predicate<IntVec3> validator, Map map, float roadChance, out IntVec3 result
			// don't drop on deep water
			__result = CellFinderLoose.RandomCellWith((IntVec3 c) => c.Standable(map) && !c.GetTerrain(map).HasTag("TKKN_Swim") && !c.Roofed(map) && !c.Fogged(map), map, 1000);
			return true;
		}
	}

	[HarmonyPatch(typeof(DropCellFinder))]
	[HarmonyPatch("CanPhysicallyDropInto")]
	public static class PatchCanPhysicallyDropInto
	{

		[HarmonyPrefix]
		public static bool Prefix(IntVec3 c, Map map, bool canRoofPunch, bool __result)
		{
			///Predicate<IntVec3> validator, Map map, float roadChance, out IntVec3 result
			// don't drop on deep water
			if (c.GetTerrain(map).HasTag("TKKN_Swim"))
			{
				__result = false;
				return false;
			}
			return true;
		}
	}

	
}
