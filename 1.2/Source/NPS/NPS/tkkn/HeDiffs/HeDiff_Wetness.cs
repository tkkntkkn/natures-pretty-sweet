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
				if (this.Severity > .62 && (this.ageTicks % 1000 == 0))
				{
					FilthMaker.TryMakeFilth(this.pawn.Position, this.pawn.MapHeld, ThingDef.Named("TKKN_FilthPuddle"), 1);
					this.Severity -= .3f;
				}

			}
			else
			{
				this.Severity += wetness / 1000;

			}
		}

		public float wetnessRate()
		{
			float rate = 0f;
			//check if the pawn is in water
			TerrainDef terrain = this.pawn.Position.GetTerrain(this.pawn.MapHeld);
			if (terrain != null && terrain.HasTag("TKKN_Wet")){
				//deep water gets them soaked.
				if (terrain.HasTag("TKKN_Swim"))
				{
					if (this.Severity < .65f)
					{
						this.Severity = .65f;
					}
					rate = .3f;
					return rate;
				}
				else
				{
					rate = .05f;
				}

			}


			//check if the pawn is wet from the weather
			bool roofed = this.pawn.MapHeld.roofGrid.Roofed(this.pawn.Position);
			if (!roofed && this.pawn.MapHeld.weatherManager.curWeather.rainRate > .001f)
			{
				rate = this.pawn.MapHeld.weatherManager.curWeather.rainRate / 10;
			}
			if (!roofed && this.pawn.MapHeld.weatherManager.curWeather.snowRate > .001f)
			{
				rate = this.pawn.MapHeld.weatherManager.curWeather.snowRate / 100;
			}

			if (rate == 0f)
			{
				timeDrying++;
			}
			else
			{
				timeDrying = 0;
				return rate;
			}

			//dry the pawn.
			if (pawn.AmbientTemperature > 0)
			{
				rate -= pawn.AmbientTemperature / 200;
			}
			//check if the pawn is near a heat source
			foreach (IntVec3 c in GenAdj.CellsAdjacentCardinal(pawn))
			{
				if (!c.InBounds(pawn.MapHeld) || !c.IsValid)
				{
					continue;
				}
				List<Thing> things = c.GetThingList(pawn.MapHeld);
				for (int i = 0; i < things.Count; i++)
				{
					Thing thing = things[i];
					CompHeatPusher heater = thing.TryGetComp<CompHeatPusher>();
					if (heater != null)
					{

						rate -= heater.Props.heatPerSecond / 5000;
					}
				}
			}

			rate -= timeDrying / 250;
//			Log.Warning(rate.ToString());
			return rate;
		}
	}
}
