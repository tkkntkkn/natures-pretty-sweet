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
		static public void SpawnPlants(CellData cell)
		{
			float leaveSomething = LeaveSomething();
			SpawnPlants(leaveSomething, cell);
		}

		static public void SpawnPlants(float leaveSomething, CellData cell)
		{
			//Grow special plants:
			if (leaveSomething < 0.005f && cell.location.GetPlant(cell.map) == null && cell.location.GetCover(cell.map) == null)
			{
				Watcher watcher = Worker.GetWatcher(cell.map);
				List<ThingDef> plants = watcher.biomeSettings.specialPlants;
				// plantDensity
				foreach (ThingDef plantDef in plants)
				{
					if (plantDef.HasModExtension<ThingWeatherReaction>())
					{
						TerrainDef terrain = cell.currentTerrain;
						ThingWeatherReaction thingWeather = plantDef.GetModExtension<ThingWeatherReaction>();
						List<TerrainDef> okTerrains = thingWeather.allowedTerrains;
						if (okTerrains != null && okTerrains.Contains<TerrainDef>(cell.currentTerrain))
						{
							if (cell.map.Biome.plantDensity * Rand.Value > 2)
							{
								Plant plant = (Plant)ThingMaker.MakeThing(plantDef, null);
								plant.Growth = Rand.Range(0.07f, 1f);
								if (plant.def.plant.LimitedLifespan)
								{
									plant.Age = Rand.Range(0, Mathf.Max(plant.def.plant.LifespanTicks - 50, 0));
								}
								GenSpawn.Spawn(plant, cell.location, cell.map);
								break;
							}
						}
					}


				}
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
				bool canSpawn = false;
				bool isSpring = element.thingDef.defName.Contains("Spring");

				// spawn at least one spring in water.



				if (isSpring && maxSprings <= watcher.totalSprings)
				{
					canSpawn = false;
				}

				if (element.thingDef.defName == "TKKN_PlantBarnacles") {
					if (element.forbiddenBiomes.Contains(map.Biome.defName)){
						Log.Error("A");
					}

					if (!element.allowedBiomes.NullOrEmpty() || !element.allowedBiomes.Contains(map.Biome.defName)) {
						Log.Error("B");

					}

					if (!element.terrainValidationAllowed.Contains(terrain.defName)){
						Log.Error("C");

					}

					if (!element.terrainValidationDisallowed.Intersect(terrain.tags).Any())
					{
						Log.Error("D");
					}
				}
				if (element.forbiddenBiomes.Contains(map.Biome.defName) ||
					(!element.allowedBiomes.NullOrEmpty() || !element.allowedBiomes.Contains(map.Biome.defName)) ||
					!element.terrainValidationAllowed.Contains(terrain.defName) ||
					!element.terrainValidationDisallowed.Intersect(terrain.tags).Any()
					)
				{
					continue;
				}

				Log.Error("spawning " + element.thingDef.defName);

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
		public static void spawnSpring(CellData cell, bool makeHot = false)
		{
			Watcher watcher = GetWatcher(cell.map);
			spawnSpring(cell, watcher, makeHot);
		}

		public static void spawnSpring(CellData cell, Watcher watcher, bool makeHot = false)
		{
			string name = makeHot ? "TKKN_HotSpring" : "TKKN_ColdSpring";

			Thing thing = (Thing)ThingMaker.MakeThing(ThingDef.Named(name), null);
			GenSpawn.Spawn(thing, cell.location, cell.map);
			watcher.totalSprings++;

		}

		public static void DoLoot(CellData cell, TerrainDef currentTerrain, TerrainDef newTerrain)
		{
			if (currentTerrain.HasTag("Water") && !newTerrain.HasTag("Water"))
			{
				LeaveLoot(cell);
			}
			else
			{
				ClearLoot(cell);
			}

		}

		public static float LeaveSomething()
		{
			return Rand.Value;
		}

		private static void LeaveLoot(CellData cell)
		{
			float leaveSomething = LeaveSomething();
			if (leaveSomething < 0.001f)
			{
				float leaveWhat = Rand.Value;
				List<string> allowed = new List<string>();
				if (leaveWhat > 0.1f)
				{
					//leave trash;
					allowed = new List<string>
					{
						"Filth_Slime",
						"TKKN_FilthShells",
						"TKKN_FilthPuddle",
						"TKKN_FilthSeaweed",
						"TKKN_FilthDriftwood",
						"TKKN_Sculpture_Shell",
						"Kibble",
						"EggRoeFertilized",
						"EggRoeUnfertilized",
					};
				}
				else if (leaveWhat > 0.05f)
				{
					//leave resource;
					allowed = new List<string>
					{
						"Steel",
						"Cloth",
						"WoodLog",
						"Synthread",
						"Hyperweave",
						"Kibble",
						"SimpleProstheticLeg",
						"MedicineIndustrial",
						"ComponentIndustrial",
						"Neutroamine",
						"Chemfuel",
						"MealSurvivalPack",
						"Pemmican",
					};
				}
				else if (leaveWhat > 0.03f)
				{
					// leave treasure.
					allowed = new List<string>
					{
						"Silver",
						"Plasteel",
						"Gold",
						"Uranium",
						"Jade",
						"Heart",
						"Lung",
						"BionicEye",
						"ScytherBlade",
						"ElephantTusk",
					};

					string text = "TKKN_NPS_TreasureWashedUpText".Translate();
					Messages.Message(text, MessageTypeDefOf.NeutralEvent);
				}
				else if (leaveWhat > 0.125f)
				{
					//leave ultrarare
					allowed = new List<string>
					{
						"AIPersonaCore",
						"MechSerumHealer",
					//	"MechSerumNeurotrainer",
						"ComponentSpacer",
						"MedicineUltratech",
						"ThrumboHorn",
					};
					string text = "TKKN_NPS_UltraRareWashedUpText".Translate();
					Messages.Message(text, MessageTypeDefOf.NeutralEvent);

				}
				if (allowed.Count > 0)
				{
					int leaveWhat2 = Rand.Range(1, allowed.Count) - 1;
					Thing loot = ThingMaker.MakeThing(ThingDef.Named(allowed[leaveWhat2]), null);
					if (loot != null)
					{
						GenSpawn.Spawn(loot, cell.location, cell.map);
					}
					else
					{
						//	Log.Error(allowed[leaveWhat2]);
					}
				}
			}
			else
			{
				SpawnPlants(leaveSomething, cell);
			}

		}

		private static void ClearLoot(CellData cell)
		{
			if (!cell.location.IsValid)
			{
				return;
			}
			List<Thing> things = cell.location.GetThingList(cell.map);
			List<string> remove = new List<string>(){
				"FilthSlime",
				"TKKN_FilthShells",
				"TKKN_FilthPuddle",
				"TKKN_FilthSeaweed",
				"TKKN_FilthDriftwood",
				"TKKN_Sculpture_Shell",
				"Kibble",
				"Steel",
				"Cloth",
				"WoodLog",
				"Synthread",
				"Hyperweave",
				"Kibble",
				"SimpleProstheticLeg",
				"MedicineIndustrial",
				"ComponentIndustrial",
				"Neutroamine",
				"Chemfuel",
				"MealSurvivalPack",
				"Pemmican",
				"Silver",
				"Plasteel",
				"Gold",
				"Uranium",
				"Jade",
				"Heart",
				"Lung",
				"BionicEye",
				"ScytherBlade",
				"ElephantTusk",
				"AIPersonaCore",
				"MechSerumHealer",
				"MechSerumNeurotrainer",
				"ComponentSpacer",
				"MedicineUltratech",
				"ThrumboHorn",
			};

			for (int i = things.Count - 1; i >= 0; i--)
			{
				if (things[i] == null)
				{
					continue;
				}
				if (remove.Contains(things[i].def.defName))
				{
					things[i].Destroy();
					continue;
				}

			}
		}
	}
}
