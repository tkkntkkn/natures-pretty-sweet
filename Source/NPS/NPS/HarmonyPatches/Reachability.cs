using System;
using Verse;
using Verse.AI;
using Harmony;

5
namespace TKKN_NPS
{
	[HarmonyPatch(typeof(Reachability), "CanReach", new Type[] { typeof(IntVec3), typeof(LocalTargetInfo), typeof(PathEndMode), typeof(TraverseParms) })]
	class PatchCanReach
	{
		[HarmonyPostfix]
		public static void Postfix(IntVec3 start, LocalTargetInfo dest, PathEndMode peMode, TraverseParms traverseParams, bool __result)
		{
			if (__result == false)
			{
				return;
			}
			if (traverseParams.pawn != null)
			{
				if (traverseParams.pawn.RaceProps.Animal)
				{
					IntVec3 c = dest.Cell;
					if (c.GetTerrain(traverseParams.pawn.Map).HasTag("TKKN_Swim") || c.GetTerrain(traverseParams.pawn.Map).HasTag("TKKN_Lava"))
					{
						__result = false;
						return;
					}
				}
			}
		}
	}
}