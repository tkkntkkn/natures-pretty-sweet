using Verse;
using RimWorld;
using System;
using System.Collections.Generic;

namespace TKKN_NPS
{
	public class BiomeSeasonalSettings : DefModExtension
	{
		public Season lastChanged;
		public Quadrum lastChangedQ;

		public List<BiomePlantRecord> specialPlants;

		//weather settings
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

		public void setWeatherBySeason(Map map, Season season, Quadrum quadrum)
		{
			if (Season.Spring == season)
			{
				map.Biome.baseWeatherCommonalities = this.springWeathers;
			}
			else if (Season.Summer == season)
			{
				map.Biome.baseWeatherCommonalities = this.summerWeathers;
			}
			else if (Season.Fall == season)
			{
				map.Biome.baseWeatherCommonalities = this.fallWeathers;
			}
			else if (Season.Winter == season)
			{
				map.Biome.baseWeatherCommonalities = this.winterWeathers;
			}
			else
			{
				if (Quadrum.Aprimay == quadrum)
				{
					map.Biome.baseWeatherCommonalities = this.springWeathers;
				}
				else if (Quadrum.Decembary == quadrum)
				{
					map.Biome.baseWeatherCommonalities = this.winterWeathers;
				}
				else if (Quadrum.Jugust == quadrum)
				{
					map.Biome.baseWeatherCommonalities = this.fallWeathers;
				}
				else if (Quadrum.Septober == quadrum)
				{
					map.Biome.baseWeatherCommonalities = this.summerWeathers;
				}
			}
			return;
		}

		public void setDiseaseBySeason(Season season, Quadrum quadrum)
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

		public void setIncidentsBySeason(Season season, Quadrum quadrum)
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
