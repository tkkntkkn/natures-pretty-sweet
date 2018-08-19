using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

// NOTE: The job that puts pawns in springs when they're too hot is in harmonypatches/jobgiverspringspatch.cs

namespace TKKN_NPS
{
	public class JobGiver_RelaxInSpring : ThinkNode_JobGiver
	{
		private float radius = 30f;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_RelaxInSpring jobGiver_RelaxInSpring = (JobGiver_RelaxInSpring)base.DeepCopy(resolve);
			jobGiver_RelaxInSpring.radius = this.radius;
			return jobGiver_RelaxInSpring;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{

			Predicate<Thing> validator = delegate (Thing t)
			{
				if (t.def.defName == "TKKN_HotSpring" && t.AmbientTemperature < 26 && t.AmbientTemperature > 15)
				{
					return true;

				}
				if (t.def.defName == "TKKN_ColdSpring" && t.AmbientTemperature > 24)
				{
					return true;

				}
				return false;
			};
			Thing spring = GenClosest.ClosestThingReachable(pawn.GetLord().CurLordToil.FlagLoc, pawn.Map, ThingRequest.ForDef(TKKN_NPS.ThingDefOf.TKKN_ColdSpring), PathEndMode.Touch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), -1f, validator, null, 0, -1, false, RegionType.Set_Passable, false);
			if (spring != null)
			{
				return new Job(RimWorld.JobDefOf.GotoSafeTemperature, this.getSpringCell(spring));
			}
			return null;
		}

		private IntVec3 getSpringCell(Thing spring)
		{
			Predicate<IntVec3> validator = delegate (IntVec3 pos)
			{
				if (spring.def.defName == "TKKN_HotSpring")
				{
					return pos.GetTerrain(spring.Map).defName == "TKKN_HotSpringsWater";
				}
				if (spring.def.defName == "TKKN_ColdSpring")
				{
					return pos.GetTerrain(spring.Map).defName == "TKKN_ColdSpringsWater";
				}
				return false;
			};
			IntVec3 c = new IntVec3();
			CellFinder.TryFindRandomCellNear(spring.Position, spring.Map, 6, validator, out c);
			return c;
		}
	}
}
