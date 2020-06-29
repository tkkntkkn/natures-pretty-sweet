using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Noise;
using TKKN_NPS.SaveData;
using TKKN_NPS.Workers;

namespace TKKN_NPS
{
	public class Watcher : MapComponent
	{
		public Dictionary<IntVec3, CellData> cellWeatherAffects = new Dictionary<IntVec3, CellData>();

		//used to save data about active springs.
		public Dictionary<int, SpringData> activeSprings = new Dictionary<int, SpringData>();

		//rebuild every save load to keep file size down
		public HashSet<IntVec3> lavaCellsList = new HashSet<IntVec3>();
		public List<IntVec3> swimmingCellsList = new List<IntVec3>();


		//save data
		public int floodLevel = 0; // 0 - 3
		public int floodThreat = 0;
		public int tideLevel = 0; // 0 - 13
		public bool doCoast = true; //false if no coast
		public bool bugFixFrostIsRemoved = false;

		public int cycleIndex;
		public int MaxPuddles = 50;
		public int totalPuddles = 0;
		public int totalSprings = 0;

		//frost
		public Thing overlay;
		public Dictionary<string, Graphic> graphicHolder = new Dictionary<string, Graphic>();
		public float[] frostGrid;
		public ModuleBase frostNoise;
		public BiomeSeasonalSettings biomeSettings;


		//used to debug / for loading existing saved games.
		public bool regenCellLists = true;

		/* STANDARD STUFF */
		public Watcher(Map map) : base(map)
		{

		}

		public override void MapComponentTick()
		{
			base.MapComponentTick();

			//rare tick
			if (Find.TickManager.TicksGame % 250 == 0)
			{
				TideWorker.DoTides(map);
				FloodWorker.DoFloods(map);
				LavaWorker.DoLava(map);
			}

			int num = Mathf.CeilToInt((float)map.Area * 0.0006f);
			int area = map.Area;
			for (int i = 0; i < num; i++)
			{
				if (cycleIndex >= area)
				{
					cycleIndex = 0;
				}
				IntVec3 c = map.cellsInRandomOrder.Get(cycleIndex);
				DoCellEnvironment(c);
				cycleIndex++;
			}

			UpdateBiomeSettings();

		}

		public void UpdateBiomeSettings(bool force = false)
		{
			if (biomeSettings != null)
			{
				Vector2 location = Find.WorldGrid.LongLatOf(map.Tile);
				Season season = GenDate.Season((long)Find.TickManager.TicksAbs, location);
				Quadrum quadrum = GenDate.Quadrum((long)Find.TickManager.TicksAbs, location.x);

				if (force == true || (biomeSettings.LastChanged != season && biomeSettings.LastChangedQ != quadrum))
				{
					biomeSettings.SetWeatherBySeason(map, season, quadrum);
					biomeSettings.SetDiseaseBySeason(season, quadrum);
					biomeSettings.SetIncidentsBySeason(season, quadrum);
					biomeSettings.LastChanged = season;
					biomeSettings.LastChangedQ = quadrum;
				}
			}
		}

		/*
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
		*/

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<bool>(ref this.regenCellLists, "regenCellLists", true, true);

			Scribe_Collections.Look<int, SpringData>(ref activeSprings, "TKKN_activeSprings", LookMode.Value, LookMode.Deep);
			Scribe_Values.Look<bool>(ref doCoast, "doCoast", true, true);
			Scribe_Values.Look<int>(ref floodThreat, "floodThreat", 0, true);
			Scribe_Values.Look<int>(ref tideLevel, "tideLevel", 0, true);
			Scribe_Values.Look<int>(ref totalPuddles, "totalPuddles", this.totalPuddles, true);
			Scribe_Values.Look<bool>(ref bugFixFrostIsRemoved, "bugFixFrostIsRemoved", this.bugFixFrostIsRemoved, true);
			Scribe_Collections.Look<IntVec3, CellData>(ref cellWeatherAffects, "cellWeatherAffects", LookMode.Value, LookMode.Deep);
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
			biomeSettings = map.Biome.GetModExtension<BiomeSeasonalSettings>();
			UpdateBiomeSettings(true);
			this.RebuildCellLists();

			// this.map.GetComponent<FrostGrid>().Regenerate(); 
			if (TKKN_Holder.modsPatched.ToArray().Count() > 0)
			{
				Log.Message("TKKN NPS: Loaded patches for: " + string.Join(", ", TKKN_Holder.modsPatched.ToArray()));
			}
		}


		#region runs on setup
		public void RebuildCellLists()
		{
			//rebuild lookup lists.
			
			/*
			#region devonly
			this.regenCellLists = true;
			Log.Error("TKKN DEV STUFF IS ON");
			#endregion
			*/

			if (regenCellLists)
			{
				//Do before initial cell list building so it's stored correctly.
				SpawnWorker.SpawnOasis(map);
				LavaWorker.FixLava(map);

				IEnumerable<IntVec3> tmpTerrain = map.AllCells.InRandomOrder();
				cellWeatherAffects = new Dictionary<IntVec3, CellData>();
				Rot4 rot = Find.World.CoastDirectionAt(map.Tile);
				doCoast = rot.IsValid;
				tideLevel = TideWorker.GetMaxFlood(TideWorker.GetTideLevel(map));

				foreach (IntVec3 c in tmpTerrain)
				{
					if (!c.InBounds(map))
					{
						continue;
					}

					TerrainDef terrain = c.GetTerrain(map);

					
					CellData cell = AddToCellList(c, terrain);

					LavaWorker.SetDeepLava(ref cell);
					TideWorker.SetUpTides(rot, ref cell);
					FloodWorker.SetUpFloodBanks(ref cell); //, ref map);

					//Spawn special elements:
					SpawnWorker.PostInitSpawnElements(cell);

				}

				
				regenCellLists = false;
			}

			FloodWorker.DoFloods(map);
			FrostGrid frostGrid = map.GetComponent<FrostGrid>();
		}

		public CellData AddToCellList(IntVec3 c)
		{
			if (!c.InBounds(map))
			{
				return null;
			}
			TerrainDef terrain = c.GetTerrain(map);
			return AddToCellList(c, terrain);
		}

		public CellData AddToCellList(IntVec3 c, TerrainDef terrain)
		{
			CellData cell = new CellData(map, terrain, c);
			cellWeatherAffects[c] = cell;
			return cell;
		}

		#endregion


		#region effect by terrain

		public CellData GetCell(IntVec3 c)
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
			CellData cell = this.GetCell(c);
			if (cell == null)
			{
				return;
			}
			DoCellEnvironment(ref cell, c);
		}

		public void DoCellEnvironment(ref CellData cell) {
			DoCellEnvironment(ref cell, cell.location);
		}

		public void DoCellEnvironment(ref CellData cell, IntVec3 c){

			TerrainDef currentTerrain = c.GetTerrain(map);
			cell.DoCellSteadyEffects(currentTerrain);

			Room room = c.GetRoom(map, RegionType.Set_All);

			WeatherBaseWorker.SetCellHumidity(ref cell, room);
			WeatherBaseWorker.SetCellTemperature(ref cell, room);
			WeatherBaseWorker.SetCellRainWetness(ref cell);

			cell.SetTerrain();

			FrostWorker.DoFrost(cell);
			//damage plants
			this.HurtPlants(cell, c, false, true);
			WeatherBaseWorker.SpawnWetThings(cell);
		}

		public void HurtPlants(CellData cell, IntVec3 c, bool onlyLow, bool saveHarvest)
		{
			int ticks = Find.TickManager.TicksGame;
				if (!Settings.allowPlantEffects || ticks % 150 != 0)
			{
				return;
			}

			//don't hurt plants in growing zone
			if (this.map.zoneManager.ZoneAt(c) is Zone_Growing zone)
			{
				return;
			}

			List<Thing> things = c.GetThingList(this.map).Where(key => key.def.category.ToString() == "Plant").ToList();
			foreach (Plant plant in things)
			{
				//see if the plant can survive here:
				float minWetnessToLive = (plant.def.plant.fertilityMin * 5) + (plant.def.plant.fertilitySensitivity * 2f);
				if (cell.HowWet >= minWetnessToLive)
				{
					continue;
				}
				//don't hurt trees
				bool isLow = true;
				if (onlyLow)
				{
					isLow = (plant.def.altitudeLayer == AltitudeLayer.LowPlant);
				}
				//don't hurt harvestables
				bool isHarvestable = true;
				if (saveHarvest)
				{
					isHarvestable = (plant.def.plant.harvestTag != "Standard");
				}

				if (plant.def.category == ThingCategory.Plant && isLow && isHarvestable)
				{
					float damage = -.001f;
					damage = damage * plant.def.plant.fertilityMin;
					plant.TakeDamage(new DamageInfo(DamageDefOf.Rotting, damage, 0, 0, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown));
				}
			}
		}

		#endregion
	}
}
