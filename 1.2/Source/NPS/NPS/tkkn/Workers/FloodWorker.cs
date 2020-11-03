using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using TKKN_NPS.SaveData;


namespace TKKN_NPS.Workers
{
	class FloodWorker : Worker
	{
		static public readonly int howManyFloodSteps = 5;
		static readonly private int middleStep = 3; //if howManyFloodSteps changes, change this.

		static private IEnumerable<CellData> GetFloodableCells(int floodLevel, Dictionary<IntVec3, CellData> cellWeatherAffects)
		{
			return cellWeatherAffects.Select(key => key.Value).Where(cell => cell.floodLevel.Any());
		}


		static public int GetMaxFlood(string type, int howManySteps)
		{
			int max = 1;
			if (type == "normal")
			{
				max = (int)Math.Floor((howManySteps + 1) / 2M);
			}
			else if (type == "high")
			{
				max = howManySteps;
			}
			return max;
		}


		static public void SetUpFloodBanks(ref CellData cell) //, ref Map map)
		{
			Map map = cell.map;
			Watcher watcher = GetWatcher(map);
			int max = GetMaxFlood(GetFloodType(map, watcher.floodThreat), howManyFloodSteps);

			//checking water instead of land because it's usually less cells.
			if (TerrainWorker.IsFreshWaterTerrain(cell.originalTerrain))
			{
				IntVec3 c = cell.location;

				//Get all land touching it.
				HashSet<CellData> bankLocations = new HashSet<CellData>();

				int ii = GenRadial.NumCellsInRadius(1);
				for (int i = 0; i < ii; i++)
				{
					IntVec3 bankC = c + GenRadial.RadialPattern[i];
					if (bankC.InBounds(map))
					{
						if (!watcher.cellWeatherAffects.TryGetValue(bankC, out CellData bankCell))
						{
							bankCell = watcher.AddToCellList(bankC, bankC.GetTerrain(map));
						}

						TerrainDef bankCheckTerrain = bankCell.originalTerrain;
						if (!TerrainWorker.IsFreshWaterTerrain(bankCheckTerrain))
						{
							bankLocations.Add(bankCell);
						}
					}
				}

				//build flood plain around each one.
				foreach (CellData bankCell in bankLocations)
				{
					//set the bank cell as the "middle" of the plain.
					watcher.cellWeatherAffects[bankCell.location].floodLevel.Add(middleStep);
					IntVec3 bankC = bankCell.location;
					for (int j = 1; j < middleStep; j++)
					{
						// add the surrounding cells to the flood plain. Water should be listed lower, land should be listed higher.
						int check = GenRadial.NumCellsInRadius(j);
						for (int k = 0; k < check; k++)
						{
							IntVec3 floodPlainC = bankC + GenRadial.RadialPattern[k];
							if (floodPlainC.InBounds(map))
							{
								if (!watcher.cellWeatherAffects.TryGetValue(floodPlainC, out CellData floodPlainCell))
								{
									floodPlainCell = watcher.AddToCellList(floodPlainC, floodPlainC.GetTerrain(map));
								}
								TerrainDef floodPlainTerrain = floodPlainCell.originalTerrain;

								if (TerrainWorker.IsLand(floodPlainTerrain))
								{
									floodPlainCell.floodLevel.Add(middleStep + j);
									//only change fertile soils & sand.
									if (floodPlainTerrain.fertility > 0)
									{
										floodPlainCell.baseTerrain = TerrainDefOf.TKKN_RiverDeposit;
										floodPlainCell.SetTerrain();
									}
								}
								else if (TerrainWorker.IsFreshWaterTerrain(floodPlainTerrain))
								{
									floodPlainCell.floodLevel.Add(middleStep - j);
									floodPlainCell.baseTerrain = TerrainDefOf.TKKN_RiverDeposit;
									floodPlainCell.SetTerrain();
								}
							}
						}
					}
				}
			}
		}

		static public void DoFloods(Map map)
		{
			if (!Settings.doSeasonalFloods)
			{
				return;
			}
			Watcher watcher = GetWatcher(map);
			Dictionary<IntVec3, CellData> cellWeatherAffects = watcher.cellWeatherAffects;

			string floodType = GetFloodType(map, watcher.floodThreat);
			int max = GetMaxFlood(floodType, howManyFloodSteps);
			if (watcher.floodLevel < max)
			{
				watcher.floodLevel++;
			}
			else
			{
				watcher.floodLevel--;
			}

			foreach (CellData cell in GetFloodableCells(watcher.floodLevel, cellWeatherAffects))
			{
				if (cell.floodLevel.Where(level => level < max).Any())
				{
					cell.SetFlooded();
				}
				else if(cell.IsFlooded)
				{
					cell.SetWet();
				}
				cell.SetTerrain();
			}
		}

		static public string GetFloodType(Map map, int floodThreat)
		{
			string flood = "normal";
			Season season = GenLocalDate.Season(map);
			if (floodThreat > 1000000 || season.Label() == "spring")
			{
				flood = "high";
			}
			else if (season.Label() == "fall")
			{
				flood = "low";
			}
			GameCondition_Drought isDrought = map.gameConditionManager.GetActiveCondition<GameCondition_Drought>();
			if (isDrought != null)
			{
				flood = isDrought.floodOverride;
			}
			return flood;
		}

		public static void UpdateFloodThreat(ref Watcher watcher, float adjustWetness)
		{
			if (watcher.floodThreat < 1090000)
			{
				watcher.floodThreat += 1 + 2 * (int)Math.Round(adjustWetness);
			}

		}

	}

}