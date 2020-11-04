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
					SpawnWorker.DoLoot(this, currentTerrain, weather.dryTerrain);
					changeTerrain(weather.dryTerrain);
				}
				else
				{
					if (baseTerrain != currentTerrain)
					{
						SpawnWorker.DoLoot(this, currentTerrain, baseTerrain);
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
					SpawnWorker.DoLoot(this, currentTerrain, weather.wetTerrain);
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
			//don't pack if there's a growing zone.
			Zone_Growing zone = this.map.zoneManager.ZoneAt(this.location) as Zone_Growing;
			if (zone != null)
			{
				return;
			}

			if (weather != null)
			{
				TerrainDef packTo = weather.packTo;
				if (packTo == null)
				{
					return;
				}

				this.howPacked++;
				if (this.howPacked > this.packAt && packTo != null)
				{
					this.changeTerrain(packTo);
					this.baseTerrain = packTo;
				}


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
					this.changeTerrain(packed);
					this.baseTerrain = packed;
				}
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

