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
			if (!Settings.allowPawnsToGetWet)
			{
				return;
			}

			float wetness = this.wetnessRate();
			if (wetness > 0)
			{
				Log.Warning("pawn " + base.pawn.Name.ToString());
				Log.Warning("wetness "+wetness.ToString());
				Log.Warning("Severity "+this.Severity.ToString());
				this.Severity += wetness / 1000;
				this.wetnessLevel += wetness;
				if (this.wetnessLevel < 0)
				{
					this.wetnessLevel = 0;
				}
				if (this.Severity > .62 && (this.ageTicks % 250 == 0))
				{
					FilthMaker.MakeFilth(this.pawn.Position, this.pawn.Map, ThingDef.Named("TKKN_FilthPuddle"), 1);
					this.Severity -= .3f;
				}

			}

			if (this.Severity > 0)
			{
				FloatRange floatRange = pawn.ComfortableTemperatureRange();
				FloatRange floatRange2 = pawn.SafeTemperatureRange();
				Log.Warning("Getting temps " + floatRange.ToString() + " " + floatRange2.ToString());

			}

		}

		public float wetnessRate()
		{
			float rate = 0f;
			//check if the pawn is in water
			TerrainDef terrain = this.pawn.Position.GetTerrain(this.pawn.Map);
			if (terrain.HasTag("Water")){
				//deep water gets them soaked.
				if (terrain.defName.ToLower().Contains("deep")){
					this.Severity = 1f;
					rate = 0f;
					return rate;
				} else {
			//		rate = .1f;
				}
			}

			//check if the pawn is wet from the weather
			bool roofed = this.pawn.Map.roofGrid.Roofed(this.pawn.Position);
			if (!roofed && this.pawn.Map.weatherManager.curWeather.rainRate > .001f)
			{
				rate = this.pawn.Map.weatherManager.curWeather.rainRate;
			}
			if (!roofed && this.pawn.Map.weatherManager.curWeather.snowRate > .001f)
			{
				rate = this.pawn.Map.weatherManager.curWeather.snowRate / 10;
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
