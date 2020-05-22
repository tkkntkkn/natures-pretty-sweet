using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace TKKN_NPS
{
	public class Watcher : MapComponent
	{
		//used to save data about active springs.
		public Dictionary<int, springData> activeSprings = new Dictionary<int, springData>();

		//used by weather
		public bool regenCellLists = true;
		public Dictionary<IntVec3, cellData> cellWeatherAffects = new Dictionary<IntVec3, cellData>();

		//rebuild every save load to keep file size down
		public HashSet<IntVec3> lavaCellsList = new HashSet<IntVec3>();
		public List<IntVec3> swimmingCellsList = new List<IntVec3>();

		public int floodLevel = 0; // 0 - 3
		public int floodThreat = 0;
		public int tideLevel = 0; // 0 - 13
		public bool doCoast = true; //false if no coast

		public int howManyTideSteps = 13;
		public int howManyFloodSteps = 5;
		public bool bugFixFrostIsRemoved = false;

		public int ticks = 0;
		public Thing overlay;
		public int cycleIndex;
		public int MaxPuddles = 50;
		public int totalPuddles = 0;
		public int totalSprings = 0;
		public float humidity = 0f;

		public Dictionary<string, Graphic> graphicHolder = new Dictionary<string, Graphic>();
		public float[] frostGrid;

		public ModuleBase frostNoise;

		public BiomeSeasonalSettings biomeSettings;

		//		public Map mapRef;

		/* STANDARD STUFF */
		public Watcher(Map map) : base(map)
		{

		}


		public override void MapComponentTick()
		{
			this.ticks++;
			base.MapComponentTick();


			//run through saved terrain and check it
			this.checkThingsforLava();

			//environmental changes
			if (Settings.doWeather)
			{
				//update humidity
				float baseHumidity = (map.TileInfo.rainfall + 1) * (map.TileInfo.temperature + 1) * (map.TileInfo.swampiness + 1);
				float currentHumidity = (1 + map.weatherManager.curWeather.rainRate) * (1 + map.mapTemperature.OutdoorTemp);
				this.humidity = ((baseHumidity + currentHumidity) / 1000) + 18;

				//rare tick
				if (this.ticks % 250 == 0)
				{
					this.DoTides();
					this.DoFloods();
				}

				//get random cells that we want to affect from our list.
				foreach (cellData cell in RandomValues(cellWeatherAffects).Take(Settings.cellBatchNumber))
				{
					this.DoCellEnvironment(cell);

				}

				/*
				// this.checkRandomTerrain(); triggering on atmosphere affects
				int num = Mathf.RoundToInt((float)this.map.Area * 0.0006f);
				int area = this.map.Area;
				for (int i = 0; i < num; i++)
				{
					if (this.cycleIndex >= area)
					{
						this.cycleIndex = 0;
					}
					IntVec3 c = this.map.cellsInRandomOrder.Get(this.cycleIndex);
					this.cycleIndex++;
				}
				*/
			}
			this.updateBiomeSettings();



		}

		public IEnumerable<TValue> RandomValues<TKey, TValue>(IDictionary<TKey, TValue> dict)
		{
			System.Random rand = new System.Random();
			List<TValue> values = Enumerable.ToList(dict.Values);
			int size = dict.Count;
			while (true)
			{
				yield return values[rand.Next(size)];
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();

			Scribe_Values.Look<bool>(ref this.regenCellLists, "regenCellLists", true, true);

			Scribe_Collections.Look<int, springData>(ref this.activeSprings, "TKKN_activeSprings", LookMode.Value, LookMode.Deep);
			Scribe_Collections.Look<IntVec3, cellData>(ref this.cellWeatherAffects, "cellWeatherAffects", LookMode.Value, LookMode.Deep);

			Scribe_Collections.Look<IntVec3>(ref this.lavaCellsList, "lavaCellsList", LookMode.Value);

			Scribe_Values.Look<bool>(ref this.doCoast, "doCoast", true, true);
			Scribe_Values.Look<int>(ref this.floodThreat, "floodThreat", 0, true);
			Scribe_Values.Look<int>(ref this.tideLevel, "tideLevel", 0, true);
			Scribe_Values.Look<int>(ref this.ticks, "ticks", 0, true);
			Scribe_Values.Look<int>(ref this.totalPuddles, "totalPuddles", this.totalPuddles, true);
			Scribe_Values.Look<bool>(ref this.bugFixFrostIsRemoved, "bugFixFrostIsRemoved", this.bugFixFrostIsRemoved, true);
		}

		public override void FinalizeInit()
		{
			/*
			List<ThingDef> plants = map.Biome.AllWildPlants;
			foreach(ThingDef plant in plants ){
				Log.Warning("Wild plant: " + plant.defName);
			}
			*/

			base.FinalizeInit();
			this.biomeSettings = map.Biome.GetModExtension<BiomeSeasonalSettings>();
			this.updateBiomeSettings(true);

			this.rebuildCellLists();
			// this.map.GetComponent<FrostGrid>().Regenerate(); 
			if (TKKN_Holder.modsPatched.ToArray().Count() > 0)
			{
				Log.Message("TKKN NPS: Loaded patches for: " + string.Join(", ", TKKN_Holder.modsPatched.ToArray()));
			}
		}


		#region runs on setup
		public void rebuildCellLists()
		{
			//rebuild lookup lists.
			this.lavaCellsList = new HashSet<IntVec3>();
			this.swimmingCellsList = new List<IntVec3>();

			this.regenCellLists = Settings.regenCells;

			/*
			#region devonly
			this.regenCellLists = true;
			Log.Error("TKKN DEV STUFF IS ON");
			this.cellWeatherAffects = new Dictionary<IntVec3, cellData>();
			#endregion
			*/

			if (this.regenCellLists)
			{
				//spawn oasis. Do before cell list building so it's stored correctly.
				this.spawnOasis();
				this.fixLava();

				Rot4 rot = Find.World.CoastDirectionAt(map.Tile);

				IEnumerable<IntVec3> tmpTerrain = map.AllCells.InRandomOrder(); //random so we can spawn plants and stuff in this step.
				this.cellWeatherAffects = new Dictionary<IntVec3, cellData>();
				foreach (IntVec3 c in tmpTerrain)
				{
					if (!c.InBounds(map))
					{
						continue;
					}

					TerrainDef terrain = c.GetTerrain(map);
					AddToCellList(c, terrain);
					if (IsLavaTerrain(terrain))
					{
						//fix for lava pathing. If lava is near lava, switch it to deep lava, making it impassable and less likely for pawns to traverse it.
						bool edgeLava = false;
						int num = GenRadial.NumCellsInRadius(1);
						for (int i = 0; i < num; i++)
						{
							IntVec3 lavaCheck = c + GenRadial.RadialPattern[i];
							if (lavaCheck.InBounds(map))
							{
								TerrainDef lavaCheckTerrain = lavaCheck.GetTerrain(this.map);
								if (!IsLavaTerrain(lavaCheckTerrain))
								{
									edgeLava = true;
								}
							}
						}
						if (!edgeLava)
						{
							this.map.terrainGrid.SetTerrain(c, TerrainDefOf.TKKN_LavaDeep);
						}
					}
					else if (rot.IsValid && IsOceanTerrain(terrain, true))
					{
						//Set up tides - get all the ocean pieces that are touching land and assign them a tide level, so the tide will move in and out
						for (int j = 0; j < this.howManyTideSteps; j++)
						{
							IntVec3 waterCheck = this.adjustForRotation(rot, c, j);
							if (waterCheck.InBounds(map) && IsOceanTerrain(waterCheck.GetTerrain(map), true))
							{
								this.map.terrainGrid.SetTerrain(c, TerrainDefOf.TKKN_SandBeachWetSalt);
								cellWeatherAffects[c].tideStep = j;
								break;
							}
						}
					}
					else if (IsFreshWaterTerrain(terrain))
					{
						for (int j = 0; j < this.howManyFloodSteps; j++)
						{
							int num = GenRadial.NumCellsInRadius(j);
							for (int i = 0; i < num; i++)
							{
								IntVec3 bankCheck = c + GenRadial.RadialPattern[i];
								if (bankCheck.InBounds(map))
								{
									TerrainDef bankCheckTerrain = bankCheck.GetTerrain(this.map);
									if (!IsRiverBank(bankCheckTerrain))
									{
										//see if this cell has already been done, because we can have each cell in multiple flood levels.
										cellData bankCell;
										if (!this.cellWeatherAffects.ContainsKey(bankCheck))
										{
											AddToCellList(bankCheck, bankCheckTerrain);
										}
										bankCell = this.cellWeatherAffects[bankCheck];
										bankCell.floodLevel.Add(j);
									}
								}
							}
						}
					}
					//Spawn special elements:
					this.spawnSpecialElements(c);
					this.spawnSpecialPlants(c);
				}
			}




			FrostGrid frostGrid = map.GetComponent<FrostGrid>();

			foreach (KeyValuePair<IntVec3, cellData> thiscell in cellWeatherAffects)
			{
				cellWeatherAffects[thiscell.Key].map = this.map;
				if (!bugFixFrostIsRemoved)
				{
					thiscell.Value.doFrostOverlay("remove");
				}
				//temp fix until I can figure out why regenerate wasn't working
				//				frostGrid.SetDepth(thiscell.Value.location, 0);
				frostGrid.SetDepth(thiscell.Value.location, thiscell.Value.frostLevel);
			}
			bugFixFrostIsRemoved = true;

			if (this.regenCellLists)
			{
				this.SetUpTidesBanks();
				this.regenCellLists = false;
			}
		}

		public void AddToCellList(IntVec3 c)
		{
			if (!c.InBounds(map))
			{
				return;
			}
			TerrainDef terrain = c.GetTerrain(map);
			AddToCellList(c, terrain);
		}


		public void AddToCellList(IntVec3 c, TerrainDef terrain)
		{
			cellData cell = new cellData(terrain, c);
			this.cellWeatherAffects[c] = cell;
		}

		/// <summary>
		/// Checks if the terrain is lava by matching the terrain defName
		/// </summary>
		public bool IsLavaTerrain(TerrainDef terrain)
		{
			return terrain.defName == "TKKN_Lava" || terrain.defName == "TKKN_LavaDeep";
		}

		/// <summary>
		/// Checks if the terrain is Ocean by matching to TKKN_Ocean. Accepts a bool that returns only shallow Ocean water
		/// </summary>
		public bool IsOceanTerrain(TerrainDef terrain, bool ignoreDeep = false)
		{
			return terrain.HasTag("TKKN_Ocean") && !(ignoreDeep && !terrain.HasTag("ShallowWater"));
		}

		/// <summary>
		/// Checks if the terrain is fresh water
		/// </summary>
		public bool IsFreshWaterTerrain(TerrainDef terrain)
		{
			return terrain.HasTag("TKKN_Wet") && terrain.HasTag("ShallowWater") && !terrain.HasTag("TKKN_Ocean");
		}
		/// <summary>
		/// Checks if the terrain is fresh water
		/// </summary>
		public bool IsRiverBank(TerrainDef terrain)
		{
			//TKKN_SandBeachWetSalt - this is to keep the areas where the river meets the sea neater.
			return !terrain.HasTag("TKKN_Wet") && terrain.defName != "TKKN_SandBeachWetSalt";
		}

		public void spawnSpecialPlants(IntVec3 c)
		{
			List<ThingDef> list = new List<ThingDef>()
			{
				ThingDef.Named("TKKN_SaltCrystal"),
				ThingDef.Named("TKKN_PlantBarnacles"),
			};

			//salt crystals:
			TerrainDef terrain = c.GetTerrain(map);
			if (terrain.defName == "TKKN_SaltField" || terrain.defName == "TKKN_SandBeachWetSalt") {
				if (c.GetEdifice(map) == null && c.GetCover(map) == null && Rand.Value < .003f)
				{
					ThingDef thingDef = ThingDef.Named("TKKN_SaltCrystal");
					Plant plant = (Plant)ThingMaker.MakeThing(thingDef, null);
					plant.Growth = Rand.Range(0.07f, 1f);
					if (plant.def.plant.LimitedLifespan)
					{
						plant.Age = Rand.Range(0, Mathf.Max(plant.def.plant.LifespanTicks - 50, 0));
					}

					GenSpawn.Spawn(plant, c, map);
				}
			}

			//barnacles and other ocean stuff
			if (terrain.defName == "TKKN_SandBeachWetSalt")
			{
				if (c.GetEdifice(map) == null && c.GetCover(map) == null && Rand.Value < .003f)
				{
					Log.Warning("Spawning Barnacle");
					ThingDef thingDef = ThingDef.Named("TKKN_PlantBarnacles");
					Plant plant = (Plant)ThingMaker.MakeThing(thingDef, null);
					plant.Growth = Rand.Range(0.07f, 1f);
					if (plant.def.plant.LimitedLifespan)
					{
						plant.Age = Rand.Range(0, Mathf.Max(plant.def.plant.LifespanTicks - 50, 0));
					}

					GenSpawn.Spawn(plant, c, map);
				}
			}

		}

		public void spawnSpecialElements(IntVec3 c)
		{
			TerrainDef terrain = c.GetTerrain(map);


			//defaults
			int maxSprings = 3;
			float springSpawnChance = .8f;

			if (biomeSettings != null)
			{
				maxSprings = biomeSettings.maxSprings;
				springSpawnChance = biomeSettings.springSpawnChance;
			}

			foreach (ElementSpawnDef element in DefDatabase<ElementSpawnDef>.AllDefs)
			{
				bool canSpawn = true;
				bool isSpring = element.thingDef.defName.Contains("Spring");

				if (isSpring && maxSprings <= totalSprings) {
					canSpawn = false;
				}

				foreach (string biome in element.forbiddenBiomes)
				{
					if (map.Biome.defName == biome)
					{
						canSpawn = false;
						break;
					}
				}


				foreach (string biome in element.allowedBiomes)
				{
					if (map.Biome.defName != biome)
					{
						canSpawn = false;
						break;
					}
				}
				if (!canSpawn)
				{
					continue;
				}


				foreach (string allowed in element.terrainValidationAllowed)
				{
					if (terrain.defName == allowed)
					{
						canSpawn = true;
						break;
					}
					canSpawn = false;
				}
				foreach (string notAllowed in element.terrainValidationDisallowed)
				{
					if (terrain.HasTag(notAllowed))
					{
						canSpawn = false;
						break;
					}
				}

				if (isSpring && canSpawn && Rand.Value < springSpawnChance) {
					Thing thing = (Thing)ThingMaker.MakeThing(element.thingDef, null);
					GenSpawn.Spawn(thing, c, map);
					totalSprings++;
				}

				if (!isSpring && canSpawn && Rand.Value < .0001f)
				{
					Thing thing = (Thing)ThingMaker.MakeThing(element.thingDef, null);
					GenSpawn.Spawn(thing, c, map);
				}

			}
		}

		public void spawnOasis()
		{
			if (this.map.Biome.defName == "TKKN_Oasis")
			{
				//spawn a big ol cold spring
				IntVec3 springSpot = CellFinderLoose.TryFindCentralCell(map, 10, 15, (IntVec3 x) => !x.Roofed(map));
				Spring spring = (Spring)ThingMaker.MakeThing(ThingDef.Named("TKKN_OasisSpring"), null);
				GenSpawn.Spawn(spring, springSpot, map);
			}
			if (Rand.Value < .001f)
			{
				this.spawnOasis();
			}
		}

		public void fixLava()
		{
			//set so the area people land in will most likely not be lava.
			if (this.map.Biome.defName == "TKKN_VolcanicFlow")
			{
				IntVec3 centerSpot = CellFinderLoose.TryFindCentralCell(map, 10, 15, (IntVec3 x) => !x.Roofed(map));
				int num = GenRadial.NumCellsInRadius(23);
				for (int i = 0; i < num; i++)
				{
					this.map.terrainGrid.SetTerrain(centerSpot + GenRadial.RadialPattern[i], TerrainDefOf.TKKN_LavaRock_RoughHewn);
				}

			}
		}

		public IntVec3 adjustForRotation(Rot4 rot, IntVec3 cell, int j)
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

		private void SetUpTidesBanks()
		{
			//set up tides and river banks for the first time:
			if (this.doCoast && Settings.doTides)
			{
				int steps = this.howManyTideSteps;
				int max = GetMaxFlood(GetTideLevel(), steps);
				for (int i = 0; i < steps; i++)
				{
					IEnumerable<cellData> updateList = cellWeatherAffects.Select(key => key.Value).Where(cell => cell.tideStep == i);
					foreach (cellData cell in updateList.ToList())
					{
						cell.baseTerrain = TerrainDefOf.TKKN_SandBeachWetSalt;
						cell.SetWetLevel();
						if (i < max)
						{
							cell.SetWetLevel(20);
						}
						this.DoCellEnvironment(cell);
					}
				}
				this.tideLevel = max;
			}

			if (Settings.doSeasonalFloods)
			{
				int steps = this.howManyFloodSteps;
				int max = GetMaxFlood(GetFloodType(), steps);
				for (int i = 0; i < steps; i++)
				{
					IEnumerable<cellData> makeWater = GetFloodableCells(i);
					foreach (cellData cell in makeWater.ToList())
					{
						//set up riverbanks
						if (!cell.baseTerrain.HasTag("TKKN_Wet"))
						{
							cell.baseTerrain = TerrainDefOf.TKKN_RiverDeposit;
						}

						cell.SetWetLevel();
						if (i < max)
						{
							cell.SetWetLevel(20);
						}
						this.DoCellEnvironment(cell);
					}
				}
			}
		}

		private IEnumerable<cellData> GetFloodableCells(int floodLevel)
		{
			return cellWeatherAffects.Select(key => key.Value).Where(cell => cell.floodLevel.Contains(floodLevel));
		}

		private int GetMaxFlood(string type, int steps){
			int max = 0;
			if (type == "normal")
			{
				max = (int)Math.Floor((steps - 1) / 2M);
			}
			else if (type == "high")
			{
				max = steps - 1;
			}
			return max;
		}

		private void updateBiomeSettings(bool force = false)
		{
			if (this.biomeSettings != null)
			{
				Vector2 location = Find.WorldGrid.LongLatOf(map.Tile);
				Season season = GenDate.Season((long)Find.TickManager.TicksAbs, location);
				Quadrum quadrum = GenDate.Quadrum((long)Find.TickManager.TicksAbs, location.x);

				if (force == true || (biomeSettings.lastChanged != season && biomeSettings.lastChangedQ != quadrum))
				{
//					Log.Warning("Updating seasonal settings");
					biomeSettings.setWeatherBySeason(map, season, quadrum);
					biomeSettings.setDiseaseBySeason(season, quadrum);
					biomeSettings.setIncidentsBySeason(season, quadrum);
					biomeSettings.lastChanged = season;
					biomeSettings.lastChangedQ = quadrum;
				}
			}
		}

		#endregion


		#region effect by terrain

		public cellData GetCell(IntVec3 c)
		{
			if (!this.cellWeatherAffects.ContainsKey(c))
			{
				return null;
			}

			if (!c.InBounds(this.map))
			{
				return null;
			}

			return this.cellWeatherAffects[c];
		}

		public void DoCellEnvironment(IntVec3 c)
		{
			cellData cell = this.GetCell(c);
			if (cell == null)
			{
				return;
			}
			DoCellEnvironment(cell, c);
		}

		public void DoCellEnvironment(cellData cell) {
			DoCellEnvironment(cell, cell.location);
		}

		public void DoCellEnvironment(cellData cell, IntVec3 c){
			TerrainDef currentTerrain = c.GetTerrain(this.map);
			cell.DoCellSteadyEffects(currentTerrain);

			Room room = c.GetRoom(this.map, RegionType.Set_All);

			this.SetCellTemperature(cell, room);
			this.SetCellWetness(cell, c);
			cell.SetTerrain();

			//frost effect
			float frostRate = 0.46f * (-1 * cell.temperature / 10);
			CreepFrostAt(c, frostRate, map);

			//damage plants
			this.HurtPlants(cell, c, false, true);

			//spawn puddles
			Thing puddle = (Thing)(from t in c.GetThingList(this.map)
								   where t.def.defName == "TKKN_FilthPuddle"
								   select t).FirstOrDefault<Thing>();

			if (cell.IsWet() && !cell.IsCold() && this.MaxPuddles > this.totalPuddles && cell.currentTerrain.defName != "TKKN_SandBeachWetSalt")
			{
				if (puddle == null)
				{
					FilthMaker.TryMakeFilth(c, this.map, ThingDef.Named("TKKN_FilthPuddle"), 1);
					this.totalPuddles++;
				}
			}
			else if (cell.howWet <= 0 && puddle != null)
			{
				puddle.Destroy();
				this.totalPuddles--;
			}

			this.cellWeatherAffects[c] = cell;
		}

		public void SetCellTemperature(cellData cell, Room room)
		{
			if (!Settings.showCold)
			{
				cell.temperature = 0;
			}
			bool useOutdoorTemp = (room == null) || (room != null && room.UsesOutdoorTemperature);
			if (useOutdoorTemp)
			{
				cell.temperature = this.map.mapTemperature.OutdoorTemp;
			} else {
				float temperature = room.Temperature;
				cell.temperature = temperature;
			}
		}

		public void SetCellWetness(cellData cell, IntVec3 c)
		{
			bool roofed = this.map.roofGrid.Roofed(c);
			#region Rain
			if (Settings.showRain && !cell.currentTerrain.HasTag("TKKN_Wet"))
			{
				//add wetness from snow/rain
				if (!roofed)
				{
					float adjustWetness = this.map.weatherManager.curWeather.rainRate + this.map.weatherManager.curWeather.snowRate;
					cell.howWet += adjustWetness;
					if (this.floodThreat < 1090000)
					{
						this.floodThreat += 1 + 2 * (int)Math.Round(adjustWetness);
					}
				}

				//evaporate wetness
				cell.howWet -= .1f;
				if (cell.IsCold())
				{
					//if it's not cold, make things dry faster
					cell.howWet -= (cell.temperature / (humidity+1)) / 50;
				}
			}
			#endregion
		}

		public static void CreepFrostAt(IntVec3 c, float baseAmount, Map map)
		{
			if (map.GetComponent<Watcher>(). frostNoise == null)
			{
				map.GetComponent<Watcher>().frostNoise = new Perlin(0.039999999105930328, 2.0, 0.5, 5, Rand.Range(0, 651431), QualityMode.Medium);
			}
			float num = map.GetComponent<Watcher>().frostNoise.GetValue(c);
			num += 1f;
			num *= 0.5f;
			float depthToAdd = baseAmount * num;

			map.GetComponent<FrostGrid>().AddDepth(c, depthToAdd);
		}

		public string GetFloodType()
		{
			string flood = "normal";
			Season season = GenLocalDate.Season(this.map);
			if (this.floodThreat > 1000000 || season.Label() == "spring")
			{
				flood = "high";
			}
			else if (season.Label() == "fall")
			{
				flood = "low";
			}
			GameCondition_Drought isDrought = this.map.gameConditionManager.GetActiveCondition<GameCondition_Drought>();
			if (isDrought != null)
			{
				flood = isDrought.floodOverride;
			}
			return flood;
		}

		public void DoFloods()
		{
			int steps = this.howManyFloodSteps;
			string floodType = this.GetFloodType();
			int max = GetMaxFlood(floodType, steps);

			int adjustWet = 0;
			if (this.floodLevel < max)
			{
				this.floodLevel++;
				adjustWet = 20;
			}
			else
			{
				this.floodLevel--;
			}


			foreach (cellData cell in this.GetFloodableCells(this.floodLevel))
			{
				cell.howWet += adjustWet;
				cell.SetTerrain();
			}
		}

		private string GetTideLevel()
		{
			if (!Settings.doTides)
			{
				return "normal";
			}
			if (this.map.gameConditionManager.ConditionIsActive(GameConditionDefOf.Eclipse))
			{
				return "high";
			}
			else if (GenLocalDate.HourOfDay(this.map) > 4 && GenLocalDate.HourOfDay(this.map) < 8)
			{
				return "low";
			}
			else if (GenLocalDate.HourOfDay(this.map) > 15 && GenLocalDate.HourOfDay(this.map) < 20)
			{
				return "high";
			}
			return "normal";
		}

		private void DoTides()
		{
			//notes to future me: use this.howManyTideSteps - 1 so we always have a little bit of wet sand, or else it looks stupid.
			if (!this.doCoast || !Settings.doTides)
			{
				return;
			}

			string tideType = GetTideLevel();
			int steps = this.howManyTideSteps;
			int max = GetMaxFlood(tideType, steps);
			if (tideLevel == max)
			{
				return;
			}

			IEnumerable<cellData> updateList = cellWeatherAffects.Select(key => key.Value).Where(cell => cell.tideStep == this.tideLevel);
			foreach(cellData cell in updateList)
			{
				if (tideLevel < max)
				{
					cell.SetFlooded();
				}
				else if (tideLevel >= max)
				{
					cell.SetWet();
				}
				cell.SetTerrain();
			}

			if (tideLevel < max)
			{
				this.tideLevel++;
			}
			else if (tideLevel > max)
			{
				this.tideLevel--;
			}
		}

		public void HurtPlants(cellData cell, IntVec3 c, bool onlyLow, bool saveHarvest)
		{
			if (!Settings.allowPlantEffects || this.ticks % 150 != 0)
			{
				return;
			}

			//don't hurt plants in growing zone
			Zone_Growing zone = this.map.zoneManager.ZoneAt(c) as Zone_Growing;
			if (zone != null)
			{
				return;
			}

			List<Thing> things = c.GetThingList(this.map);
			foreach (Thing thing in things.ToList())
			{
				Plant plant = thing as Plant;
				if (plant == null)
				{
					continue;
				}
				//see if the plant can survive here:
				float minWetnessToLive = (plant.def.plant.fertilityMin * 5) + (plant.def.plant.fertilitySensitivity * 2f);
				if (cell.howWet >= minWetnessToLive)
				{
					continue;
				}
				//don't hurt trees
				bool isLow = true;
				if (onlyLow)
				{
					isLow = (thing.def.altitudeLayer == AltitudeLayer.LowPlant);
				}
				//don't hurt harvestables
				bool isHarvestable = true;
				if (saveHarvest)
				{
					isHarvestable = (thing.def.plant.harvestTag != "Standard");
				}

				if (thing.def.category == ThingCategory.Plant && isLow && isHarvestable)
				{
					float damage = -.001f;
					damage = damage * thing.def.plant.fertilityMin;
					thing.TakeDamage(new DamageInfo(DamageDefOf.Rotting, damage, 0, 0, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown));
				}
			}
		}

		#endregion

		#region effects by pawns
		public void checkThingsforLava()
		{
			HashSet<IntVec3> removeFromLava = new HashSet<IntVec3>();

			foreach (IntVec3 c in this.lavaCellsList)
			{
				cellData cell = this.cellWeatherAffects[c];
				
				//check to see if it's still lava. Ignore roughhewn because lava can freeze/rain will cool it.
				if (!c.GetTerrain(map).HasTag("Lava") && c.GetTerrain(map).defName != "TKKN_LavaRock_RoughHewn")
				{
					cell.baseTerrain = c.GetTerrain(map);
					removeFromLava.Add(c);
					continue;
				}
			}

			foreach (IntVec3 c in removeFromLava)
			{
				this.lavaCellsList.Remove(c);
			}
			

			int n = 0;
			foreach (IntVec3 c in this.lavaCellsList.InRandomOrder())
			{
				GenTemperature.PushHeat(c, map, 1);
				if (n > 50)
				{
					break;
				}

				if (this.map.weatherManager.curWeather.rainRate > .0001f)
				{
					if (Rand.Value < .0009f)
					{
						MoteMaker.ThrowHeatGlow(c, this.map, 5f);
						MoteMaker.ThrowSmoke(c.ToVector3(), this.map, 4f);
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
					if (Rand.Value < .0005f)
					{
						MoteMaker.ThrowSmoke(c.ToVector3(), this.map, 4f);

					}
				}
				n++;
			}
		}

		public bool CheckIfCold(IntVec3 c)
		{
			if (!Settings.affectsCold){
				return false;
			}
			cellData cell = GetCell(c);
			if (cell != null)
			{
				return cell.IsCold();
			}
			return false;
		}

		#endregion
	}
}
