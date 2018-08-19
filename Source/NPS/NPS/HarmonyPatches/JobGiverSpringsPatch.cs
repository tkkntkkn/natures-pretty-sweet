using RimWorld;
using Harmony;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace TKKN_NPS
{
	//pawns will go sit in cold springs to cool off if there is no better option
	[HarmonyPatch(typeof(JobGiver_SeekSafeTemperature))]
	[HarmonyPatch("TryGiveJob")]
	class PatchTryGiveJob
	{
		public static void Postfix(ref Job __result, Pawn pawn)
		{
			if (__result == null) {
				if (Find.VisibleMap.GetComponent<Watcher>().activeSprings.Count != 0)
				{
					__result = null;
					return;
				}

				bool isHot = false;
				for (int i = 0; i < pawn.health.hediffSet.hediffs.Count; i++)
				{
					if (pawn.health.hediffSet.hediffs[i].def == RimWorld.HediffDefOf.Heatstroke && pawn.health.hediffSet.hediffs[i].CurStageIndex >= (int)TemperatureInjuryStage.Serious)
					{
						isHot = true;
						break;
					}
				}

				if (!isHot)
				{
					__result = null;
					return;
				}

				TerrainDef terrain = pawn.Position.GetTerrain(Find.VisibleMap);
				if (isHot && terrain.defName == "TKKN_ColdSpringsWater") 
				{
					__result = new Job(JobDefOf.WaitSafeTemperature, 500, true);
					return;
				}

				//send them to closest spring to relax

				Thing thing = GenClosest.ClosestThingReachable(pawn.GetLord().CurLordToil.FlagLoc, pawn.Map, ThingRequest.ForDef(TKKN_NPS.ThingDefOf.TKKN_ColdSpring), PathEndMode.Touch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), -1f, null, null, 0, -1, false, RegionType.Set_Passable, false);
				if (thing != null)
				{
					__result = new Job(JobDefOf.GotoSafeTemperature, thing.Position);
					return;
				}

			}
		}
	}

}