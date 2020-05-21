using Harmony;
using Verse;
using System;
using RimWorld;

namespace TKKN_NPS
{
	[HarmonyPatch(typeof(RCellFinder))]
	[HarmonyPatch("TryFindRandomPawnEntryCell")]
	public static class PatchTryFindRandomPawnEntryCell
	{
		[HarmonyPostfix]
		public static void Postfix(bool __result, out IntVec3 result, Map map, float roadChance, bool allowFogged = false, Predicate<IntVec3> extraValidator = null)
		{
			__result = CellFinder.TryFindRandomEdgeCellWith((Predicate<IntVec3>)((IntVec3 c) => c.Standable(map) && (c.GetTerrain(map) != null && !c.GetTerrain(map).HasTag("TKKN_Swim")) && !map.roofGrid.Roofed(c) && map.reachability.CanReachColony(c) && c.GetRoom(map, RegionType.Set_Passable).TouchesMapEdge && (allowFogged || !c.Fogged(map)) && (extraValidator == null || extraValidator(c))), map, roadChance, out result);
//			return false;
		}
	}

	
}
