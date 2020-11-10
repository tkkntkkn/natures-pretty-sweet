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
			Scribe_Values.Look<float>(ref wetnessLevel, "wetnessLevel", 0f, false);
			Scribe_Values.Look<int>(ref timeDrying, "timeDrying", 0, false);
		}

		public override void Tick()
		{
			base.Tick();
			
			if (!Settings.allowPawnsToGetWet || !pawn.Position.IsValid || pawn.MapHeld == null || !pawn.Position.InBounds(pawn.MapHeld))
			{
				return; 
			}

			float wetness = wetnessRate();
			if (wetness > 0)
			{
				Severity += wetness / 1000;
				wetnessLevel += wetness;
				if (wetnessLevel < 0)
				{
					wetnessLevel = 0;
				}
				if (Severity > .62 && (ageTicks % 1000 == 0))
				{
					FilthMaker.TryMakeFilth(pawn.Position, pawn.MapHeld, ThingDef.Named("TKKN_FilthPuddle"), 1);
					Severity -= .3f;
				}

			}
			else
			{
				Severity += wetness / 1000;

			}
		}

		public float wetnessRate()
		{
			float rate = 0f;
			//check if the pawn is in water
			TerrainDef terrain = pawn.Position.GetTerrain(pawn.MapHeld);
			if (terrain != null && terrain.HasTag("TKKN_Wet")){
				//deep water gets them soaked.
				if (terrain.HasTag("TKKN_Swim"))
				{
					if (Severity < .65f)
					{
						Severity = .65f;
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
			bool roofed = pawn.MapHeld.roofGrid.Roofed(pawn.Position);
			if (!roofed && pawn.MapHeld.weatherManager.curWeather.rainRate > .001f)
			{
				rate = pawn.MapHeld.weatherManager.curWeather.rainRate / 10;
			}
			if (!roofed && pawn.MapHeld.weatherManager.curWeather.snowRate > .001f)
			{
				rate = pawn.MapHeld.weatherManager.curWeather.snowRate / 100;
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
