using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Noise;
using System.Linq;
using TKKN_NPS.Workers;

namespace TKKN_NPS.SaveData
{
	public class CellData : IExposable
	{
		public IntVec3 location;
		public Map map;
		public int howPacked = 0;

		private float howWet = 0;

		// 0 = dry, >0-<60 = base, >60 - <90 = wet, 90+ = flood.
		public float HowWet {
			get { return howWet;  }
			set {
				howWet = value;
				if (value < 0) {
					howWet = 0;
				}
				if (value > WetCap)
				{
					howWet = WetCap;
				}
			}
		} 

		public float temperature = 0; //in celsius
		public float frostLevel = 0;

		public float humidity = 0;


		public TerrainDef baseTerrain;
		public TerrainDef originalTerrain;

		// configs:
		private readonly int wetFlood = 90;
		private readonly int wetWet = 60;
		public readonly int packAt = 750;
		private float WetCap
		{
			get { return (float)(wetFlood * 1.25); }
		}

		public string overrideType = "";

		public bool isCold {
			get { return temperature< 0; }
		}

		//wet overrides - these are used for cells near large bodies of water that will flood them periodically.
		//what step of the tide this is
		public int tideStep = -1;
		//how flooded the cell is. Cells can be affected by different floodlevels, so include them all here.
		public HashSet<int> floodLevel = new HashSet<int>();

		public CellData(Map map, TerrainDef terrain, IntVec3 c)
		{
			this.location = c;
			this.map = map;
			this.baseTerrain = terrain;
			this.originalTerrain = terrain;

			SetWetLevel();
			/*
			Room room = c.
			(map, RegionType.Set_All);
			WeatherBaseWorker.CalculateHumidity(this, room);
			WeatherBaseWorker.CalculateTemperature(this, room);
*/
		}

		public TerrainWeatherReactions weather
		{
			get
			{
				if (baseTerrain.HasModExtension<TerrainWeatherReactions>()) {
					return baseTerrain.GetModExtension<TerrainWeatherReactions>();
				} else {
					return null;
				}
			}
		}

		public TerrainWeatherReactions weatherOrig
		{
			get
			{
				if (originalTerrain.HasModExtension<TerrainWeatherReactions>())
				{
					return originalTerrain.GetModExtension<TerrainWeatherReactions>();
				}
				else
				{
					return null;
				}
			}
		}

		public TerrainDef currentTerrain
		{
			get { return this.location.GetTerrain(this.map); }
		}


		public void SetTerrain() {

			// Make sure it hasn't been made a floor or a floor hasn't been removed.
			if (!currentTerrain.HasModExtension<TerrainWeatherReactions>())
			{
				this.baseTerrain = currentTerrain;
			}
			else if (!baseTerrain.HasModExtension<TerrainWeatherReactions>() && this.baseTerrain != currentTerrain)
			{
				this.baseTerrain = currentTerrain;
			}

			if (weather == null)
			{
				return;
			}


			if (IsFlooded)
			{
				if (!SetFloodedTerrain())
				{
					SetWetTerrain();
				}

			}
			else if (IsWet)
			{
				SetWetTerrain();
			}
			else if (IsCold)
			{
				if (weather.freezeTerrain != null)
				{
					changeTerrain(weather.freezeTerrain);
				}
			}
			else 
			{
				if (weather.dryTerrain != null)
				{
					this.DoLoot(currentTerrain, weather.dryTerrain);
					changeTerrain(weather.dryTerrain);
				}
				else
				{
					if (baseTerrain != currentTerrain)
					{
						this.DoLoot(currentTerrain, baseTerrain);
						changeTerrain(baseTerrain);
					}
				}
			}
		}

		public bool SetFloodedTerrain()
		{

			if (weather.floodTerrain != null)
			{
				if (IsCold)
				{
					//set to the frozen version of the flooded terrain.

					changeTerrain(weather.floodTerrain.GetModExtension<TerrainWeatherReactions>().freezeTerrain);
					return true;
				}
				else
				{
					changeTerrain(weather.floodTerrain);
					return true;
				}
			}
			return false;
		}

		public bool SetWetTerrain()
		{
			if (weather.wetTerrain != null)
			{
				if (IsCold)
				{
					//set to the frozen version of the wet terrain
					changeTerrain(weather.wetTerrain.GetModExtension<TerrainWeatherReactions>().freezeTerrain);

					return true;
				}
				else
				{
					DoLoot(currentTerrain, weather.wetTerrain);
					changeTerrain(weather.wetTerrain);
					return true;
				}
			}
			return false;
		}
		public void SetFlooded()
		{
			SetWetLevel(WetCap);
		}

		public void SetWet()
		{
			SetWetLevel(wetWet+5);
		}


		public void SetWetLevel()
		{
			if (TerrainWorker.IsWaterTerrain(currentTerrain))
			{
				SetFlooded();
				return;
			}
			else if (!IsWet)
			{
				//default right under wet so the few days are easier.
				SetWetLevel(wetWet - 5);
			}
		}

		public void SetWetLevel(float level)
		{

			HowWet = level;
		}

		public bool IsWet {
			get { return weather != null ? HowWet > wetWet : false; }
		}

		public bool IsFlooded
		{
			get { return HowWet >= wetFlood; }
		}


		public bool IsCold
		{
			get { return temperature < 0 && Settings.affectsCold; }
		}


		public void DoCellSteadyEffects(TerrainDef currentTerrain)
		{

			//unpack soil so paths are not permenant
			Unpack();
			
			//check if the terrain has been floored
			DesignationCategoryDef cats = currentTerrain.designationCategory;
			if (cats != null && cats.defName == "Floors")
			{
				baseTerrain = currentTerrain;
			}
		}

		public void Unpack()
		{
			if (!Settings.doDirtPath)
			{
				if (currentTerrain.defName == "TKKN_DirtPath") {
					changeTerrain(RimWorld.TerrainDefOf.Soil);
				}
				if (currentTerrain.defName == "TKKN_SandPath")
				{
					changeTerrain(RimWorld.TerrainDefOf.Sand);
				}
				return;
			}
			if (howPacked > packAt)
			{
				howPacked = packAt;
			}
			if (howPacked > 0)
			{
				howPacked--;
			}
			else if(howPacked <= (packAt) && currentTerrain.defName == "TKKN_DirtPath")
			{
				changeTerrain(RimWorld.TerrainDefOf.Soil);
			}
			else if (howPacked <= (packAt) && currentTerrain.defName == "TKKN_SandPath")
			{
				changeTerrain(RimWorld.TerrainDefOf.Sand);
			}
		}

		public void doPack()
		{
			if (!Settings.doDirtPath)
			{
				return;
			}
			Zone_Growing zone = this.map.zoneManager.ZoneAt(this.location) as Zone_Growing;
			if (zone != null && (currentTerrain.defName != "TKKN_DirtPath" || currentTerrain.defName != "TKKN_SandPath"))
			{
				return;
			}           
			//don't pack if there's a growing zone.
			if (baseTerrain.defName == "Soil" || baseTerrain.defName == "Sand" || baseTerrain.texturePath == "Terrain/Surfaces/RoughStone") {
				this.howPacked++;
			}

			if (this.howPacked > this.packAt)
			{
			//	this.howPacked = this.packAt;
				if (baseTerrain.defName == "Soil")
				{
					TerrainDef packed = TerrainDef.Named("TKKN_DirtPath");
					this.changeTerrain(packed);
					this.baseTerrain = packed;
				}
				if (baseTerrain.defName == "Sand")
				{
					TerrainDef packed = TerrainDef.Named("TKKN_SandPath");
					this.changeTerrain(packed);
					this.baseTerrain = packed;
				}
			}

			if (baseTerrain.texturePath == "Terrain/Surfaces/RoughStone" && this.howPacked > this.packAt * 10)
			{
				string thisName = baseTerrain.defName;
				thisName.Replace("_Rough", "_Smooth");
				thisName.Replace("_SmoothHewn", "_Smooth");
				TerrainDef packed = TerrainDef.Named(thisName);
				this.changeTerrain(packed);
				this.baseTerrain = packed;
			}

		}

		private void changeTerrain(TerrainDef terrain)
		{
			if (terrain != null && terrain != currentTerrain)
			{
				this.map.terrainGrid.SetTerrain(location, terrain);
			}
		}

		private void SpawnElement() {
			if (Rand.Value > .0001f)
			{
				return;
			}
			string defName = "";
			if (currentTerrain.defName == "TKKN_Lava")
			{
				defName = "TKKN_LavaRock";
			}
			else if (currentTerrain.defName == "TKKN_LavaRock_RoughHewn" && this.map.Biome.defName == "TKKN_VolcanicFlow")
			{
				defName = "TKKN_SteamVent";
			}

			if (defName != "")
			{
				Thing check = (Thing)(from t in location.GetThingList(this.map)
									  where t.def.defName == defName
									  select t).FirstOrDefault<Thing>();
				if (check == null)
				{
					Thing thing = (Thing)ThingMaker.MakeThing(ThingDef.Named(defName), null);
					GenSpawn.Spawn(thing, location, map);
				}
			}
		}

		private void DoLoot(TerrainDef currentTerrain, TerrainDef newTerrain)
		{
			if (currentTerrain.HasTag("Water") && !newTerrain.HasTag("Water"))
			{
				this.leaveLoot();
			}
			else
			{
				this.clearLoot();
			}

		}

		private void leaveLoot()
		{
			float leaveSomething = Rand.Value;
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
				else if (leaveWhat > 0.025f)
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
					if(loot != null){
						GenSpawn.Spawn(loot, location, this.map);
					} else {
					//	Log.Error(allowed[leaveWhat2]);
					}
				}
			} else 

			//grow water and shore plants:
			if (leaveSomething < 0.002f && location.GetPlant(map) == null && location.GetCover(this.map) == null)
			{
				List<ThingDef> plants = this.map.Biome.AllWildPlants;
				for (int i = plants.Count - 1; i >= 0; i--)
				{
					//spawn some water plants:
					ThingDef plantDef = plants[i];
					if (plantDef.HasModExtension<ThingWeatherReaction>())
					{
						TerrainDef terrain = currentTerrain;
						ThingWeatherReaction thingWeather = plantDef.GetModExtension<ThingWeatherReaction>();
						List<TerrainDef> okTerrains = thingWeather.allowedTerrains;
						if (okTerrains != null && okTerrains.Contains<TerrainDef>(currentTerrain))
						{
							Plant plant = (Plant)ThingMaker.MakeThing(plantDef, null);
							plant.Growth = Rand.Range(0.07f, 1f);
							if (plant.def.plant.LimitedLifespan)
							{
								plant.Age = Rand.Range(0, Mathf.Max(plant.def.plant.LifespanTicks - 50, 0));
							}
							GenSpawn.Spawn(plant, location, map);
							break;
						}
					}


				}
			}
		}

		private void clearLoot()
		{
			if (!location.IsValid)
			{
				return;
			}
			List<Thing> things = location.GetThingList(this.map);
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
/*
				//remove any plants that might've grown:
				Plant plant = things[i] as Plant; ;
				if (plant != null) {
					if (plant.def.HasModExtension<ThingWeatherReaction>()) {
						TerrainDef terrain = currentTerrain;
						ThingWeatherReaction thingWeather = plant.def.GetModExtension<ThingWeatherReaction>();
						List<TerrainDef> okTerrains = thingWeather.allowedTerrains;
						if (!okTerrains.Contains<TerrainDef>(currentTerrain))
						{
							Log.Warning("Destroying " + plant.def.defName + " at " + location.ToString() + " on " + currentTerrain.defName);
							plant.Destroy();
						}
					} else {
						plant.Destroy();
					}
				}
				*/
			}
		}


		public void ExposeData()
		{
			
			Scribe_Values.Look<int>(ref this.tideStep, "tideStep", this.tideStep, true);
			Scribe_Collections.Look<int>(ref this.floodLevel, "floodLevel", LookMode.Value);
			Scribe_Values.Look<int>(ref this.howPacked, "howPacked", this.howPacked, true);
			Scribe_Values.Look<float>(ref this.howWet, "howWet", this.howWet, true);
			Scribe_Values.Look<float>(ref this.frostLevel, "frostLevel", this.frostLevel, true);
			Scribe_Values.Look<string>(ref this.overrideType, "overrideType", this.overrideType, true);
			Scribe_Values.Look<IntVec3>(ref this.location, "location", this.location, true);
			Scribe_Values.Look<float>(ref this.temperature, "temperature", 0, true);
			Scribe_Defs.Look<TerrainDef>(ref this.baseTerrain, "baseTerrain");
			Scribe_Defs.Look<TerrainDef>(ref this.originalTerrain, "originalTerrain");
		}

		public void doFrostOverlay(string action)
		{
			if (!location.InBounds(this.map))
			{
				return;
			}
			//KEEPING TO REMOVE OLD WAY OF DOING FROST
			Thing overlayIce = (Thing)(from t in location.GetThingList(this.map)
									   where t.def.defName == "TKKN_IceOverlay"
									   select t).FirstOrDefault<Thing>();
			if (overlayIce != null)
			{
				overlayIce.Destroy();
			}
		}

		static public TerrainWeatherReactions GetWeatherReactions(TerrainDef terrain) {
			if (terrain.HasModExtension<TerrainWeatherReactions>())
			{
				return terrain.GetModExtension<TerrainWeatherReactions>();
			}
			return null;
		}

	}
}

