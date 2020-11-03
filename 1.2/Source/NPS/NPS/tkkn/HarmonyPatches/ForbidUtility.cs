using System;
using Verse;
using Verse.AI;
using HarmonyLib;
using RimWorld;

namespace TKKN_NPS
{
	/*
	[HarmonyPatch(typeof(ForbidUtility), "IsForbidden", new Type[] { typeof(IntVec3), typeof(Pawn) })]
	class PatchIsForbidden
	{
		[HarmonyPostfix]
		public static void Postfix(IntVec3 c, Pawn pawn, bool __result)
		{
			if (__result == true || pawn.Map == null || !pawn.RaceProps.Animal)
			{
				return;
			}

			TerrainDef terrain = c.GetTerrain(pawn.Map);
			if (terrain != null && (terrain.HasTag("TKKN_Lava") || terrain.HasTag("TKKN_Swim")))
			{
				__result = true;
			}
		}

	}
	*/
}
