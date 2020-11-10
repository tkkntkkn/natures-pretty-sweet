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
		private float WetCap => (float)(wetFlood * 1.25);

		public string overrideType = "";
		
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
			location = c;
			baseTerrain = terrain;
			originalTerrain = terrain;


			SetWetLevel();
			/*
			Room room = c.
			(map, RegionType.Set_All);
			WeatherBaseWorker.CalculateHumidity(this, room);
			WeatherBaseWorker.CalculateTemperature(this, room);
*/
		}



		public TerrainWeatherReactions Weather
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

		public TerrainWeatherReactions WeatherOrig
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


		public TerrainWeatherReactions WeatherCurr
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
				baseTerrain = CurrentTerrain;
			}
			else if (!baseTerrain.HasModExtension<TerrainWeatherReactions>() && baseTerrain != CurrentTerrain)
			{
				baseTerrain = CurrentTerrain;
			}

			if (Weather == null)
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
				if (Weather.dryTerrain != null)
				{
					SpawnWorker.DoLoot(this, CurrentTerrain, Weather.dryTerrain);
					ChangeTerrain(Weather.dryTerrain);
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
				if (WeatherCurr != null && WeatherCurr.freezeTerrain != null)
				{
					ChangeTerrain(WeatherCurr.freezeTerrain);
				}
			}
		}

		public bool SetFloodedTerrain()
		{
			if (Weather.floodTerrain != null)
			{
				ChangeTerrain(Weather.floodTerrain);
				return true;
			}
			return false;
		}

		public bool SetWetTerrain()
		{
			if (Weather.wetTerrain != null)
			{
				SpawnWorker.DoLoot(this, CurrentTerrain, Weather.wetTerrain);
				ChangeTerrain(Weather.wetTerrain);
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

		public bool IsWet => Weather != null ? HowWet > wetWet : false;

		public bool IsFlooded => HowWet >= wetFlood;


		public bool IsCold => temperature < 0 && Settings.affectsCold;


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
				if (Weather != null && CurrentTerrain == Weather.packTo)
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
			else if (howPacked <= (packAt/2) && Weather != null && CurrentTerrain != Weather.packTo)
			{
				ChangeTerrain(originalTerrain);
			}

		}

		public void DoPack()
		{
			if (!Settings.doDirtPath || Weather == null)
			{
				return;
			}
			//don't pack if there's a growing zone.
			if (map.zoneManager.ZoneAt(location) is Zone_Growing zone)
			{
				return;
			}

			TerrainDef packTo = Weather.packTo;
			if (packTo == null)
			{
				return;
			}

			howPacked++;
			if (howPacked > packAt && packTo != null)
			{
				ChangeTerrain(packTo);
				baseTerrain = packTo;
				howPacked = (int) Math.Round((float)(howPacked - (howPacked / 5))); //so if the path is removed immediately, it isn't also immediately repacked.
			}

			bool isStone = baseTerrain.affordances.Contains(TerrainAffordanceDefOf.SmoothableStone);
			if (isStone)
			{
				howPacked++;
				if (howPacked > packAt * 10)
				{
					string thisName = baseTerrain.defName;
					thisName.Replace("_Rough", "_Smooth");
					thisName.Replace("_SmoothHewn", "_Smooth");
					TerrainDef packed = TerrainDef.Named(thisName);
					ChangeTerrain(packed);
					baseTerrain = packed;
				}
			}


		}

		private void ChangeTerrain(TerrainDef terrain)
		{
			if (terrain != null && terrain != CurrentTerrain)
			{
				map.terrainGrid.SetTerrain(location, terrain);
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
			else if (CurrentTerrain.defName == "TKKN_LavaRock_RoughHewn" && map.Biome.defName == "TKKN_VolcanicFlow")
			{
				defName = "TKKN_SteamVent";
			}

			if (defName != "")
			{
				Thing check = (Thing)(from t in location.GetThingList(map)
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
			
			Scribe_Values.Look<int>(ref tideStep, "tideStep", tideStep, true);
			Scribe_Collections.Look<int>(ref floodLevel, "floodLevel", LookMode.Value);
			Scribe_Values.Look<int>(ref howPacked, "howPacked", howPacked, true);
			Scribe_Values.Look<float>(ref howWet, "howWet", howWet, true);
			Scribe_Values.Look<float>(ref frostLevel, "frostLevel", frostLevel, true);
			Scribe_Values.Look<string>(ref overrideType, "overrideType", overrideType, true);
			Scribe_Values.Look<IntVec3>(ref location, "location", location, true);
			Scribe_Values.Look<float>(ref temperature, "temperature", 0, true);
			Scribe_Defs.Look<TerrainDef>(ref baseTerrain, "baseTerrain");
			Scribe_Defs.Look<TerrainDef>(ref originalTerrain, "originalTerrain");

			Scribe_Values.Look<float>(ref SaveUpdatedTo, "SaveUpdatedTo");

			//convert data from old saves
			if (Scribe.mode == LoadSaveMode.LoadingVars && SaveUpdatedTo != 1.2f)
			{
				Scribe_Values.Look<float>(ref howWet, "howWetPlants", howWet);
				Scribe_Values.Look<int>(ref tideStep, "tideLevel", tideStep);
				
			}
		}

		public void DoFrostOverlay(string action)
		{
			if (!location.InBounds(map))
			{
				return;
			}
			//KEEPING TO REMOVE OLD WAY OF DOING FROST
			Thing overlayIce = (Thing)(from t in location.GetThingList(map)
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

