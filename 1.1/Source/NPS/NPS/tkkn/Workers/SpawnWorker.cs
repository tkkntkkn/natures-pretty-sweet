using System.Linq;
using Verse;
using TKKN_NPS.SaveData;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Noise;
using System.Linq;
using TKKN_NPS.Workers;


namespace TKKN_NPS.Workers
{
	class SpawnWorker : Worker
	{
		static private int maxSprings = 3;
		static private float springSpawnChance = .8f;


		static public void SpawnOasis(Map map)
		{
			if (map.Biome.defName == "TKKN_Oasis")
			{
				//spawn a big ol cold spring
				IntVec3 springSpot = CellFinderLoose.TryFindCentralCell(map, 10, 15, (IntVec3 x) => !x.Roofed(map));
				Spring spring = (Spring)ThingMaker.MakeThing(ThingDef.Named("TKKN_OasisSpring"), null);
				GenSpawn.Spawn(spring, springSpot, map);
			}
			if (Rand.Value < .001f)
			{
				//make another one
				SpawnOasis(map);
			}
		}

		static public void PostInitSpawnElements(CellData cell)
		{
			Map map = cell.map;
			IntVec3 c = cell.location;
			TerrainDef terrain = c.GetTerrain(map);

			Watcher watcher = GetWatcher(map);

			if (watcher.biomeSettings != null)
			{
				maxSprings = watcher.biomeSettings.maxSprings;
				springSpawnChance = watcher.biomeSettings.springSpawnChance;
			}

			foreach (ElementSpawnDef element in DefDatabase<ElementSpawnDef>.AllDefs)
			{
				/// TODO: redo spring spawning.
				bool canSpawn = false;
				bool isSpring = element.thingDef.defName.Contains("Spring");

				if (isSpring && maxSprings <= watcher.totalSprings)
				{
					canSpawn = false;
				}


				if (element.forbiddenBiomes.Contains(map.Biome.defName))
				{
					continue;
				}
				if (!element.allowedBiomes.Contains(map.Biome.defName))
				{
					continue;
				}

				if (!element.terrainValidationAllowed.Contains(terrain.defName))
				{
					continue;
				}

				if (!element.terrainValidationDisallowed.Intersect(terrain.tags).Any())
				{
					continue;
				}

				if (isSpring && canSpawn && Rand.Value < springSpawnChance)
				{
					Thing thing = (Thing)ThingMaker.MakeThing(element.thingDef, null);
					GenSpawn.Spawn(thing, c, map);
					watcher.totalSprings++;
				}

				if (!isSpring && canSpawn && Rand.Value < .0001f)
				{
					Thing thing = (Thing)ThingMaker.MakeThing(element.thingDef, null);
					GenSpawn.Spawn(thing, c, map);
				}

			}
		}
	}
}
