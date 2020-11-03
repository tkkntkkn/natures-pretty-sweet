using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace TKKN_NPS
{
	class Hediff_Thirst : HediffWithComps
	{
		public float thirstLevel;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<float>(ref this.thirstLevel, "sweatLevel", 0f, false);
		}

		public override void Tick()
		{
			base.Tick();

			if (!this.pawn.Position.IsValid || this.pawn.MapHeld == null || !this.pawn.Position.InBounds(this.pawn.MapHeld))
			{
				return;
			}
			if (this.pawn.RaceProps.Humanlike || this.pawn.IsColonist)
			{
				Hediff_Thirst thirstDiff = this.pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.TKKN_Thirst) as Hediff_Thirst;
				this.pawn.health.RemoveHediff(thirstDiff);
				return;
			}
			Watcher watcher = pawn.MapHeld.GetComponent<Watcher>();
			if (!watcher.hasDrinking)
			{
				Hediff_Thirst thirstDiff = this.pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.TKKN_Thirst) as Hediff_Thirst;
				this.pawn.health.RemoveHediff(thirstDiff);
				return;
			}

			float thirst = this.thirstRate();
			this.Severity += thirst / 1000;

		}
		public float thirstRate()
		{
			TerrainDef terrain = pawn.Position.GetTerrain(pawn.MapHeld);
			if (terrain != null && terrain.HasTag("TKKN_Water") && !terrain.HasTag("TKKN_Salt")){
				return -.2f;
			}
			float rate = .01f;
			if (pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.TKKN_Sweat) != null)
			{
				rate += .01f;
			}
			if (pawn.health.hediffSet.GetFirstHediffOfDef(RimWorld.HediffDefOf.Heatstroke) != null)
			{
				rate += .02f;
			}
			if (pawn.pather.MovingNow)
			{
				rate += .03f;
			}

			rate += pawn.AmbientTemperature / 1000;

			return rate;
		}
	}
}
