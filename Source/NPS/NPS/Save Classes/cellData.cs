using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Noise;
using System.Linq;


namespace TKKN_NPS
{
	public class cellData : IExposable
	{
		public IntVec3 location;
		public Map map;
		public int howPacked = 0;
		public int howWet = 0;
		public float howWetPlants = 30;
		public float temperature = -9999;
		public float frostLevel = 0;
		public TerrainDef baseTerrain;
		public TerrainDef originalTerrain;

		public string overrideType = "";

		public bool gettingWet = false;
		public bool isWet = false;
		public bool isMelt = false;
		public bool isFlooded = false;
		public bool isFrozen = false;
		public bool isThawed = true;

		public int tideLevel = -1;
		public HashSet<int> floodLevel = new HashSet<int>();



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

		public TerrainDef currentTerrain
		{
			get { return this.location.GetTerrain(this.map); }
		}

		public void setTerrain(string type) {
			//Make sure it hasn't been made a floor or a floor hasn't been removed.
			if (weather == null)
			{
				return;
			}
			if (!currentTerrain.HasModExtension<TerrainWeatherReactions>())
			{
				this.baseTerrain = currentTerrain;
				return;
			}
			else if (!baseTerrain.HasModExtension<TerrainWeatherReactions>())
			{
				this.baseTerrain = currentTerrain;
			}

			//change the terrain
			if (type == "frozen") {
				this.setFrozenTerrain();
			} else if (type == "dry")
			{
				this.setWetTerrain();
			} else if (type == "wet")
			{
				this.setWetTerrain();
			}
			else if (type == "thaw")
			{
				if (isFrozen == true)
				{
					this.howWet = 1;
					this.setWetTerrain();
					isFrozen = false;
				}
				else
				{
					this.setFrozenTerrain();
				}
			}
			else if (type == "flooded")
			{
				this.setFloodedTerrain();
			}
			else if (type == "tide")
			{
				this.setTidesTerrain();
			}

			this.overrideType = "";
		}

		public void DoCellSteadyEffects()
		{

		}

		public void setWetTerrain()
		{
			if (!Settings.showRain)
			{
				return;
			}

			if (weather.wetTerrain != null && currentTerrain != weather.wetTerrain && howWet > weather.wetAt)
			{
				changeTerrain(weather.wetTerrain);
				if (baseTerrain.defName == "TKKN_Lava")
				{
					this.map.GetComponent<Watcher>().lavaCellsList.Remove(location);
				}
				isWet = true;
				rainSpawns();
			}
			else if (howWet == 0 && currentTerrain != baseTerrain && isWet && !isFlooded){
				changeTerrain(baseTerrain);
				if (baseTerrain.defName == "TKKN_Lava")
				{
					this.map.GetComponent<Watcher>().lavaCellsList.Add(location);
				}
				isWet = false;
				howWet = -1;
			}
			else if (howWet == -1 && weather.dryTerrain != null && !isFlooded)
			{
				if (currentTerrain != weather.dryTerrain || baseTerrain != weather.dryTerrain)
				{
					isWet = false;
					baseTerrain = weather.dryTerrain;
					changeTerrain(weather.dryTerrain);
				}
			}
			//			*/
		}
	
		public void setFrozenTerrain() {
			if (!Settings.showCold) {
				return;
			}


			if (this.temperature < 0  && this.temperature < this.weather.freezeAt && this.weather.freezeTerrain != null)
			{
				if (this.isFlooded && this.weather.freezeTerrain != currentTerrain)
				{
					if (currentTerrain.HasModExtension<TerrainWeatherReactions>())
					{
						TerrainWeatherReactions curWeather = currentTerrain.GetModExtension<TerrainWeatherReactions>();
						this.changeTerrain(curWeather.freezeTerrain);
					}
				}
				else if(!this.isFrozen)
				{
					this.changeTerrain(weather.freezeTerrain);
					if (baseTerrain.defName == "TKKN_Lava")
					{
						this.map.GetComponent<Watcher>().lavaCellsList.Remove(location);
					}

				}
				this.isFrozen = true;
				this.isThawed = false;
			}
			else  if (temperature > 0)
			{
				if (!this.isThawed)
				{
					if (this.baseTerrain.defName == "TKKN_Lava")
					{
						this.map.GetComponent<Watcher>().lavaCellsList.Add(location);
					}
					this.isFrozen = false;
					this.isThawed = true;
					this.changeTerrain(baseTerrain);
				}
			}
		}

		public void setFloodedTerrain()
		{
			if (!Settings.showRain)
			{
				return;
			}

			TerrainDef floodTerrain = weather.floodTerrain;
			if (isFrozen)
			{
				TerrainWeatherReactions currWeather = currentTerrain.GetModExtension<TerrainWeatherReactions>();
				TerrainDef frozenTerrain = currWeather.freezeTerrain;
				if (frozenTerrain != null)
				{
					changeTerrain(frozenTerrain);
				}
			} else if (overrideType == "dry")
			{
				floodTerrain = baseTerrain;
				changeTerrain(floodTerrain);
			}
			else if (floodTerrain != null && currentTerrain != floodTerrain)
			{
				changeTerrain(floodTerrain);

				this.isFlooded = true;
				if (!floodTerrain.HasTag("Water"))
				{
					this.isFlooded = false;
					this.leaveLoot();
				}
			}
		}

		public void setTidesTerrain()
		{
			if (!Settings.doTides)
			{
				return;
			}
			if (currentTerrain != baseTerrain)
			{
				changeTerrain(baseTerrain);
			}
			else
			{
				changeTerrain(weather.tideTerrain);
			}

			if (weather.tideTerrain != null && weather.tideTerrain.HasTag("TKKN_Wet"))
			{
				this.clearLoot();
			}
			else
			{
				this.leaveLoot();
			}
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
				if (isFrozen)
				{
					isMelt = true;
				}
				overlayIce.Destroy();
			}

		}
		

		public void unpack()
		{
			//only pack dirt right now. TO DO: Add sand.
			//don't pack if there's a growing zone.

			if (this.howPacked > 1000)
			{
				this.howPacked = 1000;
			}
			if (this.howPacked > 0)
			{
				this.howPacked--;
			}
			else if(this.howPacked == 500 && this.currentTerrain.defName == "TKKN_DirtPath")
			{
				this.changeTerrain(RimWorld.TerrainDefOf.Soil);
			}
			else if (this.howPacked == 500 && this.currentTerrain.defName == "TKKN_SandPath")
			{
				this.changeTerrain(RimWorld.TerrainDefOf.Sand);
			}
		}

		public void doPack()
		{
			//only pack dirt right now. TO DO: Add sand.
			//don't hurt things in growing zone
			Zone_Growing zone = this.map.zoneManager.ZoneAt(this.location) as Zone_Growing;
			if (zone != null && (currentTerrain.defName != "TKKN_DirtPath" || currentTerrain.defName != "TKKN_SandPath"))
			{
				return;
			}           
			//don't pack if there's a growing zone.
			if (baseTerrain.defName == "Soil" || baseTerrain.defName == "Sand") {
				this.howPacked++;
			}
			/*
			if (currentTerrain.defName == "PackedDirt")
			{
				TerrainDef packed1 = TerrainDef.Named("Soil");
				this.changeTerrain(packed1);
				this.baseTerrain = packed1;
			}
			*/

			if (this.howPacked > 1000 )
			{
				this.howPacked = 1000;
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
		}
		/*
		public void doFrostOverlay(string action) {
			if (action == "add")
			{
				if (!Settings.showCold) {
					return;
				}
				Thing overlayIce = (Thing)(from t in location.GetThingList(this.map)
										   where t.def.defName == "TKKN_IceOverlay"
										   select t).FirstOrDefault<Thing>();
				if ((weather.freezeTerrain == null || currentTerrain != weather.freezeTerrain || weather.isSalty) && !currentTerrain.HasTag("Water") && overlayIce == null)
				{
					Thing ice = ThingMaker.MakeThing(ThingDefOf.TKKN_IceOverlay, null);
					GenSpawn.Spawn(ice, location, map);
				}
			}
			else
			{
				Thing overlayIce = (Thing)(from t in location.GetThingList(this.map)
										   where t.def.defName == "TKKN_IceOverlay"
										   select t).FirstOrDefault<Thing>();
				if (overlayIce != null)
				{
					if (isFrozen)
					{
						isMelt = true;
					}
					overlayIce.Destroy();
				}

			}
		}
		*/

		private void changeTerrain(TerrainDef terrain)
		{
			if (terrain != null)
			{
				this.map.terrainGrid.SetTerrain(location, terrain);
			}
		}

		private void rainSpawns()
		{
			//spawn special things when it rains.
			if (Rand.Value < .009){
				if (baseTerrain.defName == "TKKN_Lava"){
					GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.TKKN_LavaRock), location, map);
				} else if (baseTerrain.defName == "TKKN_SandBeachWetSalt") {
					GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.TKKN_crab), location, map);
				} else if (currentTerrain.HasTag("TKKN_Wet"))
				{
					MoteMaker.MakeWaterSplash(location.ToVector3(), this.map, 1, 1);
				}
					
			} else if (Rand.Value < .04 && currentTerrain.HasTag("Lava"))
			{
				MoteMaker.ThrowSmoke(location.ToVector3(), this.map, 5);
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
				else if (leaveWhat > 0.02f)
				{
					//leave ultrarare
					allowed = new List<string>
					{
						"AIPersonaCore",
						"MechSerumHealer",
						"MechSerumNeurotrainer",
						"AdvancedComponent",
						"GlitterworldMedicine",
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
						Log.Error(allowed[leaveWhat2]);
					}
						
				}

			}
		}

		private void clearLoot()
		{

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
				"Medicine",
				"Component",
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
				"AdvancedComponent",
				"GlitterworldMedicine",
				"ThrumboHorn",
			};

			for (int i = things.Count - 1; i >= 0; i--)
			{
				if (remove.Contains(things[i].def.defName))
				{
					things[i].Destroy();
				}
			}
		}


		public void ExposeData()
		{
			
			Scribe_Values.Look<int>(ref this.tideLevel, "tideLevel", this.tideLevel, true);
			Scribe_Collections.Look<int>(ref this.floodLevel, "floodLevel", LookMode.Value);
			Scribe_Values.Look<int>(ref this.howPacked, "howPacked", this.howPacked, true);
			Scribe_Values.Look<int>(ref this.howWet, "howWet", this.howWet, true);
			Scribe_Values.Look<float>(ref this.howWetPlants, "howWetPlants", this.howWetPlants, true);
			Scribe_Values.Look<float>(ref this.frostLevel, "frostLevel", this.frostLevel, true);
			Scribe_Values.Look<bool>(ref this.isWet, "isWet", this.isWet, true);
			Scribe_Values.Look<bool>(ref this.isFlooded, "isFlooded", this.isFlooded, true);
			Scribe_Values.Look<bool>(ref this.isMelt, "isMelt", this.isMelt, true);
			Scribe_Values.Look<string>(ref this.overrideType, "overrideType", this.overrideType, true);

			Scribe_Values.Look<bool>(ref this.isThawed, "isThawed", this.isThawed, true);

			

			Scribe_Values.Look<IntVec3>(ref this.location, "location", this.location, true);
			Scribe_Values.Look<float>(ref this.temperature, "temperature", -999, true);
			Scribe_Defs.Look<TerrainDef>(ref this.baseTerrain, "baseTerrain");

			Scribe_Defs.Look<TerrainDef>(ref this.originalTerrain, "originalTerrain");
			



		}

	}
}

