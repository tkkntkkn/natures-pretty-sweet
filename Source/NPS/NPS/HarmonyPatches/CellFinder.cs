using Harmony;
using Verse;
using System;

namespace TKKN_NPS
{
	[HarmonyPatch(typeof(CellFinder))]
	[HarmonyPatch("TryRandomClosewalkCellNear")]
	public static class PatchTryRandomClosewalkCellNear
	{

		[HarmonyPrefix]
		public static bool Prefix(IntVec3 root, Map map, int radius, out IntVec3 result, Predicate<IntVec3> extraValidator, bool __result)
		{
			///Predicate<IntVec3> validator, Map map, float roadChance, out IntVec3 result
			// don't enter on deep water
			__result = CellFinder.TryFindRandomReachableCellNear(root, map, (float)radius, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), (Predicate<IntVec3>)((IntVec3 c) => c.Standable(map) && !c.GetTerrain(map).HasTag("TKKN_Swim")  && (extraValidator == null || extraValidator(c))), (Predicate<Region>)null, out result, 999999);		
			return false;
		}
	}
}
