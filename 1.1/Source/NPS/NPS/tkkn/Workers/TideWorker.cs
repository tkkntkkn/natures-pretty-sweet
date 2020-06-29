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
	class TideWorker : FloodWorker
	{
		static new public int howManyFloodSteps = 13;

		/// <summary>
		/// Sets the tide step level on every cell.
		/// </summary>
		static public void SetUpTides(Rot4 rot, ref CellData cell)
		{
			Map map = cell.map;
			Watcher watcher = GetWatcher(map);

			int max = GetMaxFlood(GetTideLevel(map));

			IntVec3 c = cell.location;
			if (rot.IsValid && TerrainWorker.IsSandTerrain(cell.currentTerrain))
			{
				//get all the sand pieces that are touching water.
				for (int j = 0; j < howManyFloodSteps; j++)
				{
					IntVec3 waterCheck = adjustForRotation(rot, c, j);
					if (waterCheck.InBounds(map) && TerrainWorker.IsOceanTerrain(waterCheck.GetTerrain(map), true))
					{
						cell.tideStep = j;
					//	cell.originalTerrain = TerrainDefOf.TKKN_SandBeachWetSalt;
						cell.baseTerrain = TerrainDefOf.TKKN_SandBeachWetSalt;
						cell.SetWet();
						cell.SetTerrain();
						break;
					}
				}
			}
			/*
			if (rot.IsValid && IsOceanTerrain(cell.currentTerrain, true))
			{

				IntVec3 c = cell.location;
				Log.Error("found ocean water");
				//Set up tides - get all the ocean pieces that are touching land and assign them a tide level, so the tide will move in and out
				for (int j = 0; j < howManyTideSteps; j++)
				{
					IntVec3 waterCheck = adjustForRotation(rot, c, j);
					if (waterCheck.InBounds(map) && IsOceanTerrain(waterCheck.GetTerrain(map), true))
					{
						map.terrainGrid.SetTerrain(c, TerrainDefOf.TKKN_SandBeachWetSalt);
						cell.baseTerrain = TerrainDefOf.TKKN_SandBeachWetSalt;
						cell.tideStep = j;
						cell.SetWetLevel();
						cell.tideStep = j;
						if (j < max)
						{
							cell.SetFlooded();
						}
						break;
					}
				}
			}
			*/

		}


		/// <summary>
		/// finds correct direction of coast.
		/// </summary>

		static public IntVec3 adjustForRotation(Rot4 rot, IntVec3 cell, int j)
		{
			IntVec3 newDirection = new IntVec3(cell.x, cell.y, cell.z);
			if (rot == Rot4.North)
			{
				newDirection.z += j + 1;
			}
			else if (rot == Rot4.South)
			{
				newDirection.z -= j - 1;
			}
			else if (rot == Rot4.East)
			{
				newDirection.x += j + 1;
			}
			else if (rot == Rot4.West)
			{
				newDirection.x -= j - 1;
			}
			return newDirection;
		}


		/// <summary>
		/// Resets tide cells to wet terrain.
		/// </summary>
		static public void ResetTides(Map map)
		{
			Watcher watcher = GetWatcher(map);
			Dictionary<IntVec3, CellData> cellWeatherAffects = watcher.cellWeatherAffects;

			int neutralTide = (int)Math.Floor((decimal)howManyFloodSteps / 2);
			watcher.tideLevel = neutralTide;
			IEnumerable <CellData> updateList = cellWeatherAffects.Select(key => key.Value).Where(cell => cell.tideStep > neutralTide).Where(cell => cell.IsFlooded == true);
			foreach (CellData cell in updateList)
			{
				cell.SetWet();
				cell.SetTerrain();
			}
		}

		/// <summary>
		/// Are tides on in settings and is there a coast?
		/// </summary>
		static bool CanDoTides(Watcher watcher)
		{
			return watcher.doCoast && Settings.doTides;
		}

		/// <summary>
		/// Sets tides based on the current level on watcher
		/// </summary>
		/// <param name="map"></param>
		static public void DoTides(Map map)
		{
			Watcher watcher = GetWatcher(map);
			Dictionary<IntVec3, CellData> cellWeatherAffects = watcher.cellWeatherAffects;

			//notes to future me: use howManyTideSteps - 1 so we always have a little bit of wet sand, or else it looks stupid.
			if (!CanDoTides(watcher))
			{
				return;
			}

			string tideType = GetTideLevel(map);
			int max = GetMaxFlood(tideType);
			if (watcher.tideLevel == max)
			{
				return;
			}

			IEnumerable<CellData> updateList = cellWeatherAffects.Select(key => key.Value).Where(cell => cell.tideStep == watcher.tideLevel);
			foreach (CellData cell in updateList)
			{
				if (watcher.tideLevel < max)
				{
					cell.SetFlooded();
				}
				else if (watcher.tideLevel >= max)
				{
					cell.SetWet();
				}
				cell.SetTerrain();
			}

			if (watcher.tideLevel < max)
			{
				watcher.tideLevel++;
			}
			else if (watcher.tideLevel > max)
			{
				watcher.tideLevel--;
			}
		}

		/// <summary>
		/// Returns the current tide level as a string
		/// </summary>
		/// <param name="map"></param>
		/// <returns>high, low, normal</returns>
		public static string GetTideLevel(Map map)
		{
			if (map.gameConditionManager.ConditionIsActive(RimWorld.GameConditionDefOf.Eclipse))
			{
				return "high";
			}
			else if (GenLocalDate.HourOfDay(map) > 4 && GenLocalDate.HourOfDay(map) < 8)
			{
				return "low";
			}
			else if (GenLocalDate.HourOfDay(map) > 15 && GenLocalDate.HourOfDay(map) < 20)
			{
				return "high";
			}
			return "normal";
		}

	}
}