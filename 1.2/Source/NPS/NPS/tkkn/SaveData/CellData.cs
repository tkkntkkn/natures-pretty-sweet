using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Noise;
using System.Linq;
using TKKN_NPS.Workers;
using System;

namespace TKKN_NPS.SaveData
{
	public class CellData : IExposable
	{
		private float SaveUpdatedTo = 0f;

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

		public CellData()
		{
		}
		public CellData(Map map)
		{
			this.map = map;
		}
		public CellData(Map map, TerrainDef terrain, IntVec3 c)
		{
			this.map = map;
			this.location = c;
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


		public TerrainWeatherReactions weatherCurr
		{
			get
			{
				if (CurrentTerrain.HasModExtension<TerrainWeatherReactions>())
				{
					return CurrentTerrain.GetModExtension<TerrainWeatherReactions>();
				}
				else
				{
					return null;
				}
			}
		}

		public TerrainDef CurrentTerrain
		{
			get {
				if (map == null)
				{
					Log.Error("Map is Null");
				}
				return location.GetTerrain(map);
			}
		}


		public void SetTerrain() {

			// Make sure it hasn't been made a floor or a floor hasn't been removed.
			if (!CurrentTerrain.HasModExtension<TerrainWeatherReactions>())
			{
				this.baseTerrain = CurrentTerrain;
			}
			else if (!baseTerrain.HasModExtension<TerrainWeatherReactions>() && this.baseTerrain != CurrentTerrain)
			{
				this.baseTerrain = CurrentTerrain;
			}

			if (weather == null)
			{
				return;
			}

			//update wetness levels first
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
			else 
			{
				if (weather.dryTerrain != null)
				{
					SpawnWorker.DoLoot(this, CurrentTerrain, weather.dryTerrain);
					ChangeTerrain(weather.dryTerrain);
				}
				else
				{
					if (baseTerrain != CurrentTerrain)
					{
						SpawnWorker.DoLoot(this, CurrentTerrain, baseTerrain);
						ChangeTerrain(baseTerrain);
					}
				}
			}


			//update temperature levels next
			if (IsCold)
			{
				if (weatherCurr != null && weatherCurr.freezeTerrain != null)
				{
					ChangeTerrain(weatherCurr.freezeTerrain);
				}
			}
		}

		public bool SetFloodedTerrain()
		{
			if (weather.floodTerrain != null)
			{
				ChangeTerrain(weather.floodTerrain);
				return true;
			}
			return false;
		}

		public bool SetWetTerrain()
		{
			if (weather.wetTerrain != null)
			{
				SpawnWorker.DoLoot(this, CurrentTerrain, weather.wetTerrain);
				ChangeTerrain(weather.wetTerrain);
				return true;
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
			if (TerrainWorker.IsWaterTerrain(CurrentTerrain))
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


		public void DoCellSteadyEffects()
		{

			//unpack soil so paths are not permenant
			Unpack();
			
			//check if the terrain has been floored
			DesignationCategoryDef cats = CurrentTerrain.designationCategory;
			if (cats != null && cats.defName == "Floors")
			{
				baseTerrain = CurrentTerrain;
			}
		}

		public void Unpack()
		{
			if (!Settings.doDirtPath)
			{
				if (weather != null && CurrentTerrain == weather.packTo)
				{
					ChangeTerrain(originalTerrain);
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
			else if (howPacked <= (packAt/2) && weather != null && CurrentTerrain != weather.packTo)
			{
				ChangeTerrain(originalTerrain);
			}

		}

		public void DoPack()
		{
			if (!Settings.doDirtPath || weather == null)
			{
				return;
			}
			//don't pack if there's a growing zone.
			Zone_Growing zone = this.map.zoneManager.ZoneAt(this.location) as Zone_Growing;
			if (zone != null)
			{
				return;
			}

			TerrainDef packTo = weather.packTo;
			if (packTo == null)
			{
				return;
			}

			this.howPacked++;
			if (this.howPacked > this.packAt && packTo != null)
			{
				this.ChangeTerrain(packTo);
				this.baseTerrain = packTo;
				this.howPacked = (int) Math.Round((float)(howPacked - (howPacked / 5))); //so if the path is removed immediately, it isn't also immediately repacked.
			}

			bool isStone = baseTerrain.affordances.Contains(TerrainAffordanceDefOf.SmoothableStone);
			if (isStone)
			{
				this.howPacked++;
				if (this.howPacked > this.packAt * 10)
				{
					string thisName = baseTerrain.defName;
					thisName.Replace("_Rough", "_Smooth");
					thisName.Replace("_SmoothHewn", "_Smooth");
					TerrainDef packed = TerrainDef.Named(thisName);
					this.ChangeTerrain(packed);
					this.baseTerrain = packed;
				}
			}


		}

		private void ChangeTerrain(TerrainDef terrain)
		{
			if (terrain != null && terrain != CurrentTerrain)
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
			if (CurrentTerrain.defName == "TKKN_Lava")
			{
				defName = "TKKN_LavaRock";
			}
			else if (CurrentTerrain.defName == "TKKN_LavaRock_RoughHewn" && this.map.Biome.defName == "TKKN_VolcanicFlow")
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

		

		public void ExposeData()
		{
			
			Scribe_Values.Look<int>(ref this.tideStep, "tideStep", this.tideStep, true);
			Scribe_Collections.Look<int>(ref this.floodLevel, "floodLevel", LookMode.Value);
			Scribe_Values.Look<int>(ref this.howPacked, "howPacked", this.howPacked, true);
			Scribe_Values.Look<float>(ref this.howWet, "howWet", this.howWet, true);
			Scribe_Values.Look<float>(ref this.frostLevel, "frostLevel", this.frostLevel, true);
			Scribe_Values.Look<string>(ref this.overrideType, "overrideType", this.overrideType, true);
			Scribe_Values.Look<IntVec3>(ref location, "location", location, true);
			Scribe_Values.Look<float>(ref this.temperature, "temperature", 0, true);
			Scribe_Defs.Look<TerrainDef>(ref this.baseTerrain, "baseTerrain");
			Scribe_Defs.Look<TerrainDef>(ref this.originalTerrain, "originalTerrain");

			Scribe_Values.Look<float>(ref this.SaveUpdatedTo, "SaveUpdatedTo");

			//convert data from old saves
			if (Scribe.mode == LoadSaveMode.LoadingVars && SaveUpdatedTo != 1.2f)
			{
				Scribe_Values.Look<float>(ref this.howWet, "howWetPlants", this.howWet);
				Scribe_Values.Look<int>(ref this.tideStep, "tideLevel", this.tideStep);
				
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

