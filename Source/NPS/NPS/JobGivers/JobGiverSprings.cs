using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

// NOTE: The job that puts pawns in springs when they're too hot is in harmonypatches/jobgiverspringspatch.cs

namespace TKKN_NPS
{
	public class JobDriver_RelaxInSpring : JobDriver_VisitJoyThing
	{
		private Thing SpringThing
		{
			get
			{
				return this.job.GetTarget(TargetIndex.A).Thing;
			}
		}


		public override bool TryMakePreToilReservations()
		{
			return true;
		}

		protected override Action GetWaitTickAction()
		{
			return delegate
			{
				Pawn pawn = this.pawn;
				float extraJoyGainFactor = 1;
				JoyUtility.JoyTickCheckEnd(pawn, JoyTickFullJoyAction.EndJob, extraJoyGainFactor);
			};
		}

		public override string GetReport()
		{
			TerrainDef terrain = this.pawn.Position.GetTerrain(this.Map);
			if (terrain.defName == "TKKN_HotSpringsWater")
			{
				return "TKKN_NPS_RelaxHotSpring".Translate();

			}
			return "TKKN_NPS_RelaxColdSpring".Translate();
		}
	}
}
