using System;
using System.Collections.Generic;
using Verse;
using RimWorld;

namespace TKKN_NPS
{
	public class Hediff_Wetness : HediffWithComps
	{
		public float wetnessLevel;
		private int timeDrying = 0;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<float>(ref this.wetnessLevel, "wetnessLevel", 0f, false);
			Scribe_Values.Look<int>(ref this.timeDrying, "timeDrying", 0, false);
		}

		public override void Tick()
		{
			base.Tick();
			
			if (!Settings.allowPawnsToGetWet || !this.pawn.Position.IsValid || this.pawn.MapHeld == null || !this.pawn.Position.InBounds(this.pawn.MapHeld))
			{
				return;
			}

			float wetness = this.wetnessRate();
			if (wetness > 0)
			{
				this.Severity += wetness / 1000;
				this.wetnessLevel += wetness;
				if (this.wetnessLevel < 0)
				{
					this.wetnessLevel = 0;
				}
				if (this.Severity > .62 && (this.ageTicks % 500 == 0))
				{
					FilthMaker.MakeFilth(this.pawn.Position, this.pawn.MapHeld, ThingDef.Named("TKKN_FilthPuddle"), 1);
					this.Severity -= .3f;
				}

			}

		}

		public float wetnessRate()
		{
			float rate = 0f;
			//check if the pawn is in water
			TerrainDef terrain = this.pawn.Position.GetTerrain(this.pawn.MapHeld);
			if (terrain.HasTag("TKKN_Swim")){
				//deep water gets them soaked.
				rate = .1f;
				return rate;
			}

			//check if the pawn is wet from the weather
			bool roofed = this.pawn.Map.roofGrid.Roofed(this.pawn.Position);
			if (!roofed && this.pawn.Map.weatherManager.curWeather.rainRate > .001f)
			{
				rate = this.pawn.Map.weatherManager.curWeather.rainRate / 10;
			}
			if (!roofed && this.pawn.Map.weatherManager.curWeather.snowRate > .001f)
			{
				rate = this.pawn.Map.weatherManager.curWeather.snowRate / 100;
			}
			if (this.wetnessLevel == 0f)
			{
				timeDrying = 0;
				return rate;
			}

			if (rate == 0f) {
				timeDrying++;
			}

			//dry the pawn.
			if (pawn.AmbientTemperature > 0)
			{
				rate -= pawn.AmbientTemperature / 100;
			}
			//check if the pawn is near a heat source
			foreach (IntVec3 c in GenAdj.CellsAdjacentCardinal(pawn))
			{
				List<Thing> things = c.GetThingList(pawn.Map);
				for (int i = 0; i < things.Count; i++)
				{
					Thing thing = things[i];
					CompHeatPusher heater = thing.TryGetComp<CompHeatPusher>();
					if (heater != null)
					{

						rate -= heater.Props.heatPerSecond / 100;
					}
				}
			}

			rate -= timeDrying / 250;
			return rate;
		}
	}
}
