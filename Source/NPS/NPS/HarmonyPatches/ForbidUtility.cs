using System;
using Verse;
using Harmony;
using RimWorld;

namespace TKKN_NPS
{

	[HarmonyPatch(typeof(ForbidUtility), "IsForbidden", new Type[] { typeof(Thing), typeof(Pawn) })]
	class PatchIsForbidden
	{
		[HarmonyPostfix]
		public static void Postfix(Thing t, Pawn pawn, bool __result)
		{
			if (__result == false || pawn.Map == null)
			{
				return;
			}
			TerrainDef terrain = pawn.Position.GetTerrain(pawn.Map);
			if (terrain != null && (terrain.HasTag("Lava") || terrain.HasTag("TKKN_Swim")) && !pawn.RaceProps.Humanlike) {
				__result = false;

			}
		}

	}

}
