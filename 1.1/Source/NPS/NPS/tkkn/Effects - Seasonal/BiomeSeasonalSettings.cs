using Verse;
using RimWorld;
using System;
using System.Collections.Generic;

namespace TKKN_NPS
{
	public class BiomeSeasonalSettings : DefModExtension
	{
		public Season LastChanged;
		public Quadrum LastChangedQ;

		public List<ThingDef> specialPlants;


		//weather settings
		public Dictionary<String, List<WeatherCommonalityRecord>> weatherLookup = new Dictionary<String, List<WeatherCommonalityRecord>>();
		public List<WeatherCommonalityRecord> springWeathers;
		public List<WeatherCommonalityRecord> summerWeathers;
		public List<WeatherCommonalityRecord> fallWeathers;
		public List<WeatherCommonalityRecord> winterWeathers;

		//incident settings
		public List<ThingDef> bloomPlants;
		public List<PawnKindDef> specialHerds;
		public List<TKKN_IncidentCommonalityRecord> springEvents;
		public List<TKKN_IncidentCommonalityRecord> summerEvents;
		public List<TKKN_IncidentCommonalityRecord> fallEvents;
		public List<TKKN_IncidentCommonalityRecord> winterEvents;


		//disease settings
		public List<BiomeDiseaseRecord> springDiseases;
		public List<BiomeDiseaseRecord> summerDiseases;
		public List<BiomeDiseaseRecord> fallDiseases;
		public List<BiomeDiseaseRecord> winterDiseases;

		//spring settings
		public int maxSprings;
		public float springSpawnChance;
		public bool springsSurviveDrought;
		public bool springsSurviveSummer;

		//misc settings
		public int wetPlantStart = 50;

		public bool plantsAdded;
		public bool plantCacheUpdated;
		public bool diseaseCacheUpdated;

		public BiomeSeasonalSettings()
		{
			weatherLookup.Add("spring", springWeathers);
			weatherLookup.Add("summer", summerWeathers);
			weatherLookup.Add("fall", fallWeathers);
			weatherLookup.Add("winter", winterWeathers);
		}

		public bool canPutOnTerrain(IntVec3 c, ThingDef thingDef, Map map)
		{
			TerrainDef terrain = c.GetTerrain(map);

			//make sure plants are spawning on terrain that they're limited to:
			ThingWeatherReaction weatherReaction = thingDef.GetModExtension<ThingWeatherReaction>();
			if (weatherReaction != null && terrain != null && weatherReaction.allowedTerrains != null)
			{
				//if they're only allowed to spawn in certain terrains, stop it from spawning.
				if (!weatherReaction.allowedTerrains.Contains(terrain))
				{
					return false;
				}
			}
			return true;
		}

		public string GetLookupKey(Season season) {
			if (Season.Spring == season)
			{
				return "spring";
			}
			else if (Season.Summer == season)
			{
				return "summer";
			}
			else if (Season.Fall == season)
			{
				return "fall";
			}
			else if (Season.Winter == season)
			{
				return "winter";
			}
			return "spring";
		}

		public string GetLookupKey(Quadrum quadrum)
		{
			if (Quadrum.Aprimay == quadrum)
			{
				return "spring";
			}
			else if (Quadrum.Decembary == quadrum)
			{
				return "summer";
			}
			else if (Quadrum.Jugust == quadrum)
			{
				return "fall";
			}
			else if (Quadrum.Septober == quadrum)
			{
				return "winter";
			}
			return "spring";
		}

		public void SetWeatherBySeason(Map map, Season season, Quadrum quadrum)
		{
			List<WeatherCommonalityRecord> setTo = null;
			setTo = weatherLookup[GetLookupKey(season)];

			if (setTo == null) {
				setTo = weatherLookup[GetLookupKey(quadrum)];
			}
			if (setTo != null) {
				map.Biome.baseWeatherCommonalities = setTo;
			}
			return;
		}

		public void SetDiseaseBySeason(Season season, Quadrum quadrum)
		{
			List<BiomeDiseaseRecord> seasonalDiseases = new List<BiomeDiseaseRecord>();
			if (Season.Spring == season && this.springDiseases != null)
			{
				seasonalDiseases = this.springDiseases;
			}
			else if (Season.Summer == season && this.summerDiseases != null)
			{
				seasonalDiseases = this.summerDiseases;
			}
			else if (Season.Fall == season && this.fallDiseases != null)
			{
				seasonalDiseases = this.fallDiseases;
			}
			else if (Season.Winter == season && this.winterDiseases != null)
			{
				seasonalDiseases = this.winterDiseases;
			}
			else
			{
				if (Quadrum.Aprimay == quadrum && this.springDiseases != null)
				{
					seasonalDiseases = this.springDiseases;
				}
				else if (Quadrum.Decembary == quadrum && this.winterDiseases != null)
				{
					seasonalDiseases = this.winterDiseases;
				}
				else if (Quadrum.Jugust == quadrum && this.summerDiseases != null)
				{
					seasonalDiseases = this.summerDiseases;
				}
				else if (Quadrum.Septober == quadrum && this.fallDiseases != null)
				{
					seasonalDiseases = this.fallDiseases;
				}
			}

			for (int i = 0; i < seasonalDiseases.Count; i++)
			{
				BiomeDiseaseRecord diseaseRec = seasonalDiseases[i];
				IncidentDef disease = diseaseRec.diseaseInc;
				disease.baseChance = diseaseRec.commonality;
			}
			diseaseCacheUpdated = false;

		}

		public void SetIncidentsBySeason(Season season, Quadrum quadrum)
		{
			List<TKKN_IncidentCommonalityRecord> seasonalIncidents = new List<TKKN_IncidentCommonalityRecord>();
			if (Season.Spring == season && this.springEvents != null)
			{
				seasonalIncidents = this.springEvents;
			}
			else if (Season.Summer == season && this.summerEvents != null)
			{
				seasonalIncidents = this.summerEvents;
			}
			else if (Season.Fall == season && this.fallEvents != null)
			{
				seasonalIncidents = this.fallEvents;
			}
			else if (Season.Winter == season && this.winterEvents != null)
			{
				seasonalIncidents = this.winterEvents;
			}
			else
			{
				if (Quadrum.Aprimay == quadrum && this.springEvents != null)
				{
					seasonalIncidents = this.springEvents;
				}
				else if (Quadrum.Decembary == quadrum && this.winterEvents != null)
				{
					seasonalIncidents = this.winterEvents;
				}
				else if (Quadrum.Jugust == quadrum && this.summerEvents != null)
				{
					seasonalIncidents = this.summerEvents;
				}
				else if (Quadrum.Septober == quadrum && this.fallEvents != null)
				{
					seasonalIncidents = this.fallEvents;
				}
			}

			for (int i = 0; i < seasonalIncidents.Count; i++){
				TKKN_IncidentCommonalityRecord incidentRate = seasonalIncidents[i];
				IncidentDef incident = incidentRate.incident;
				incident.baseChance = incidentRate.commonality;
			}

		}
	}
}
