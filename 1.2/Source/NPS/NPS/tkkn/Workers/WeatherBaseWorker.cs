using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Noise;
using TKKN_NPS.SaveData;

namespace TKKN_NPS.Workers
{
	class WeatherBaseWorker : Worker
	{

		static public void ResetWeather(CellData cell)
		{
			cell.humidity = 0;
		}

		/*
		static public void SetBiomeHumidity(Map map)
		{
			if (!Settings.doWeather)
			{
				return;
			}

			Watcher watcher = GetWatcher(map);

			float baseHumidity = (map.TileInfo.rainfall + 1) * (map.TileInfo.temperature + 1) * (map.TileInfo.swampiness + 1);
			float currentHumidity = (1 + map.weatherManager.curWeather.rainRate) * (1 + map.mapTemperature.OutdoorTemp);
			watcher.humidity = ((baseHumidity + currentHumidity) / 1000) + 18;
		}
		*/

		static public float CalculateHumidity(CellData cell, Room room)
		{
			if (!Settings.doWeather)
			{
				return 0;
			}

			Map map = cell.map;

			bool useOutdoorTemp = (room == null || (room != null && room.UsesOutdoorTemperature));
			float baseHumidity = (map.TileInfo.rainfall + 1) * (map.TileInfo.temperature + 1) * (map.TileInfo.swampiness + 1);
			float currentHumidity = 0;

			if (cell.IsFlooded)
			{
				currentHumidity += 5;
			}
			if (cell.IsWet)
			{
				currentHumidity += 5;
			}

			if (useOutdoorTemp)
			{
				currentHumidity = (1 + map.weatherManager.curWeather.rainRate) * (1 + map.mapTemperature.OutdoorTemp) * .9f;
			}
			else
			{
				// amplify humidity from interior water sources
				if (!cell.isCold)
				{
					currentHumidity = currentHumidity * 1.5f;
				}

				float temperature = room.Temperature;
				float tempDiff = Math.Abs(map.mapTemperature.OutdoorTemp - temperature);
				if (tempDiff > 5)
				{
					// assuming it's temperature controlled.
					currentHumidity -= tempDiff;
				}
				currentHumidity -= temperature / 10;
			}

			return ((baseHumidity + currentHumidity) / 1000) + 18;

		}


		static public void SetCellHumidity(ref CellData cell, Room room)
		{
			Map map = cell.map;


			cell.humidity = CalculateHumidity(cell, room);

		}

		static public float CalculateTemperature(CellData cell, Room room)
		{
			float temperature = 0;
			if (!Settings.showCold)
			{
				return temperature;
			}
			bool useOutdoorTemp = (room == null) || (room != null && room.UsesOutdoorTemperature);
			temperature = useOutdoorTemp ? cell.map.mapTemperature.OutdoorTemp : room.Temperature;

			return temperature;
		}

		static public void SetCellTemperature(ref CellData cell, Room room)
		{
			cell.temperature = CalculateTemperature(cell, room);
		}

		static public void SetCellRainWetness(ref CellData cell)
		{
			if (!Settings.doWeather)
			{
				return;
			}


			if (Settings.affectsWet && !TerrainWorker.IsWaterTerrain(cell.CurrentTerrain) && cell.tideStep == -1)
			{
				float adjustWetness = AdjustWetBy(cell);
				cell.HowWet += adjustWetness;
				if (Settings.DoDisasterFloods)
				{
					Watcher watcher = GetWatcher(cell.map);
					FloodWorker.UpdateFloodThreat(ref watcher, adjustWetness);
				}
			}
		}
		/*
		static public float AdjustWetBy(CellData cell)
		{
			return AdjustWetBy(cell.location, cell.map, cell.temperature, cell.humidity, cell.IsCold);
		}
		*/

//		static public float AdjustWetBy(IntVec3 c, Map map, float temperature, float humidity, bool isCold)
		static public float AdjustWetBy(CellData cell)
		{
			float adjustWetness = 0f;
			bool roofed = cell.map.roofGrid.Roofed(cell.location);

			//add wetness from snow/rain
			if (!roofed)
			{
				adjustWetness += (cell.map.weatherManager.curWeather.rainRate + cell.map.weatherManager.curWeather.snowRate) * 10;
			}

			//evaporate wetness
			adjustWetness -= 2.5f;
			if (!cell.isCold)
			{
				//if it's not cold/dry, make things dry faster
				adjustWetness -= (int)Math.Floor((cell.temperature / (cell.humidity + 1) ));
			}

			if (cell.IsFlooded)
			{
				//flood cells should wet/dry slower
				adjustWetness = adjustWetness / 1.5f;
			} else if (FloodWorker.WaitingToFlood(cell) && adjustWetness > 0 && !FloodWorker.ShouldFlood(cell)) {
				//if it's waiting to flood, let the lower levels flood first for a better visual effect
				return 0;
			} else if (!cell.IsWet && adjustWetness > 0)
			{
				//dry cells should wet faster (to help the soil affect show up in a reasonable time)
				adjustWetness = adjustWetness * 1.1f;
			}

			return adjustWetness;

		}

		static public void SpawnWetThings(CellData cell)
		{
			if (!Settings.doWeather)
			{
				return;
			}

			if (!Settings.showRain)
			{
				return;
			}
			Watcher watcher = GetWatcher(cell.map);
			IntVec3 c = cell.location;
			Thing puddle = (Thing)(from t in c.GetThingList(cell.map)
								   where t.def.defName == "TKKN_FilthPuddle"
								   select t).FirstOrDefault<Thing>();

			if (cell.IsWet && !cell.IsCold && watcher.MaxPuddles > watcher.totalPuddles && puddle == null)
			{
				FilthMaker.TryMakeFilth(c, cell.map, ThingDef.Named("TKKN_FilthPuddle"), 1);
				watcher.totalPuddles++;
			}
			else if (!cell.IsWet && puddle != null)
			{
				puddle.Destroy();
				watcher.totalPuddles--;
			}

			// TO DO - set this up to be configurable through defs.
			//spawn special things when it rains.
			if (Rand.Value < .0009)
			{
				if (TerrainWorker.IsLava(cell.CurrentTerrain))
				{
					GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.TKKN_LavaRock), cell.location, cell.map);
				}
				else if (TerrainWorker.IsSandTerrain(cell.baseTerrain) && cell.IsWet)
				{
					Pawn pawn = PawnGenerator.GeneratePawn(TKKN_NPS.PawnKindDefOf.TKKN_crab, null);
					GenSpawn.Spawn(pawn, cell.location, cell.map, WipeMode.Vanish);
				}
				else if (cell.IsFlooded)
				{
					MoteMaker.MakeWaterSplash(cell.location.ToVector3(), cell.map, 1, 1);
				}
			}
		}
	}
}
