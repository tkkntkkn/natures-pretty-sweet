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
		/// <summary>
		/// Makes sure the langing area isn't lava
		/// </summary>
		public static void FixLava(Map map)
		{
			//set so the area pawns land in will most likely not be lava, and so they have somewhere to build.
			IntVec3 centerSpot = CellFinderLoose.TryFindCentralCell(map, 10, 15, (IntVec3 x) => !x.Roofed(map));
			int num = GenRadial.NumCellsInRadius(23);
			Watcher watcher = GetWatcher(map);

			for (int i = 0; i < num; i++)
			{
				IntVec3 spot = centerSpot + GenRadial.RadialPattern[i];
				if (TerrainWorker.IsLava(spot, map))
				{
					map.terrainGrid.SetTerrain(spot, TerrainDefOf.TKKN_LavaRock_RoughHewn);
					watcher.AddToCellList(spot, TerrainDefOf.TKKN_LavaRock_RoughHewn);
				}
			}
		}

		/// <summary>
		/// Force non-edge lava to be deep lava
		/// </summary>
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

		/// <summary>
		/// Do Lava effects (on cell weather effect)
		/// </summary>
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

		/// <summary>
		/// Visual effects for lava
		/// </summary>
		public static void DoLavaEffects(CellData cell)
		{
			Map map = cell.map;
			IntVec3 c = cell.location;

			if (Settings.DoLavaDamagingEffects)
			{
				GenTemperature.PushHeat(c, map, 100);
			}

			if (!Settings.DoLavaVisualEffects)
			{
				return;
			}
			if (Rand.Value < Settings.LavaVisualEffectChance)
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
				 * TO DO add ShouldSpawnMotesAt
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
				if (Rand.Value < (Settings.LavaVisualEffectChance / 10))
				{
					MoteMaker.ThrowSmoke(c.ToVector3(), map, 4f);
				}
			}
		}

		/// <summary>
		/// Lava damages pawns
		/// </summary>
		public static void HurtWithLava(Pawn pawn)
		{
			if (CanHurtThingWithLava(pawn)){
				IntVec3 c = pawn.Position;
				Map map = pawn.MapHeld;
				FireUtility.TryAttachFire(pawn, .5f);
			}
		}

		/// <summary>
		/// Lava damages/destroys items
		/// </summary>
		public static void HurtWithLava(Thing thing)
		{
			IntVec3 c = thing.Position;
			Map map = thing.MapHeld;
			if (CanHurtThingWithLava(thing))
			{
				FireUtility.TryStartFireIn(c, map, 5f);
				DoDestruction(thing);
			}
		}

		/// <summary>
		/// Checks if we should do lava damage.
		/// </summary>
		private static bool CanHurtThingWithLava(Thing thing)
		{
			if (!Settings.DoLavaDamagingEffects)
			{
				return false;
			}
			IntVec3 c = thing.Position;
			Map map = thing.MapHeld;
			if (TerrainWorker.IsLava(c, map))
			{
				return true;
			}
			return false;
		}

	

		/// <summary>
		/// Destroys inflammable items
		/// </summary>
		public static void DoDestruction(Thing thing)
		{
			float statValue = thing.GetStatValue(StatDefOf.Flammability, true);
			bool alt = thing.def.altitudeLayer == AltitudeLayer.Item;
			if (statValue == 0f && alt == true)
			{
				if (!thing.Destroyed && thing.def.destroyable)
				{
					thing.Destroy(DestroyMode.Vanish);
				}
			}
		}
	}
}