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
	class LavaWorker : Worker
	{
/*
		public static bool spawnLavaOnlyInBiome = true;
		public static bool allowLavaEruption = true;
*/
		public static void FixLava(Map map)
		{
			//set so the area pawns land in will most likely not be lava, and so they have somewhere to build.
			IntVec3 centerSpot = CellFinderLoose.TryFindCentralCell(map, 10, 15, (IntVec3 x) => !x.Roofed(map));
			int num = GenRadial.NumCellsInRadius(23);
			for (int i = 0; i < num; i++)
			{
				IntVec3 spot = centerSpot + GenRadial.RadialPattern[i];
				if (spot.GetTerrain(map).HasTag("TKKN_Lava"))
				{
					map.terrainGrid.SetTerrain(centerSpot + GenRadial.RadialPattern[i], TerrainDefOf.TKKN_LavaRock_RoughHewn);
				}
			}
		}

		public static void SetDeepLava(ref CellData cell)
		{
			if (TerrainWorker.IsLava(cell))
			{
				IntVec3 c = cell.location;
				Map map = cell.map;

				//fix for lava pathing. If lava is near lava, switch it to deep lava, making it impassable and less likely for pawns to traverse it.
				bool edgeLava = false;
				int num = GenRadial.NumCellsInRadius(1);
				for (int i = 0; i < num; i++)
				{
					IntVec3 lavaCheck = c + GenRadial.RadialPattern[i];
					if (lavaCheck.InBounds(map))
					{
						TerrainDef lavaCheckTerrain = lavaCheck.GetTerrain(map);
						if (!TerrainWorker.IsLava(lavaCheckTerrain))
						{
							edgeLava = true;
							break;
						}
					}
				}
				if (!edgeLava)
				{
					map.terrainGrid.SetTerrain(c, TerrainDefOf.TKKN_LavaDeep);
					cell.baseTerrain = TerrainDefOf.TKKN_LavaDeep;
				}
			}
		}

		public static void DoLavaEffects(CellData cell)
		{
			Map map = cell.map;
			IntVec3 c = cell.location;
			GenTemperature.PushHeat(c, map, 1);
			if (Rand.Value < .09f)
			{
				MoteMaker.ThrowHeatGlow(c, map, 5f);
				if (map.weatherManager.curWeather.rainRate > 1)
				{
					MoteMaker.ThrowSmoke(c.ToVector3(), map, 4f);
				}
			}
			else
			{
				/*
				if (Rand.Value < .0001f)
				{

					MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDef.Named("TKKN_HeatWaver"), null);
					moteThrown.Scale = Rand.Range(2f, 4f) * 2;
					moteThrown.exactPosition = c.ToVector3();
					moteThrown.SetVelocity(Rand.Bool ? -90 : 90, 0.12f);
					GenSpawn.Spawn(moteThrown, c, this.map);
				}
				else 
				*/
				if (Rand.Value < .01f)
				{
					MoteMaker.ThrowSmoke(c.ToVector3(), map, 4f);
				}
			}
		}

		public static void DoLava(Map map)
			{
				Watcher watcher = GetWatcher(map);
				Dictionary<IntVec3, CellData> cellWeatherAffects = watcher.cellWeatherAffects;

				IEnumerable<CellData> updateList = cellWeatherAffects.Select(key => key.Value).Where(cell => TerrainWorker.IsLava(cell.currentTerrain) == true);
				foreach (CellData cell in updateList.ToList().InRandomOrder().Take(updateList.Count()/3))
				{
					DoLavaEffects(cell);
				}
			}
		}
}