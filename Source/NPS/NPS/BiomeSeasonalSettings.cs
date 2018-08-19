using Verse;
using RimWorld;
using System;
using System.Collections.Generic;

namespace TKKN_NPS
{
	class BiomeSeasonalSettings : DefModExtension
	{
		public List<ThingDef> bloomPlants;
		public List<WeatherCommonalityRecord> springWeathers;
		public List<WeatherCommonalityRecord> summerWeathers;
		public List<WeatherCommonalityRecord> fallWeathers;
		public List<WeatherCommonalityRecord> winterWeathers;
		public List<TKKN_IncidentCommonalityRecord> springEvents;
		public List<TKKN_IncidentCommonalityRecord> summerEvents;
		public List<TKKN_IncidentCommonalityRecord> fallEvents;
		public List<TKKN_IncidentCommonalityRecord> winterEvents;
		public List<BiomeDiseaseRecord> springDiseases;
		public List<BiomeDiseaseRecord> summerDiseases;
		public List<BiomeDiseaseRecord> fallDiseases;
		public List<BiomeDiseaseRecord> winterDiseases;

		public void setWeatherBySeason(Map map, Season season)
		{
			if (Season.Spring == season) {
				map.Biome.baseWeatherCommonalities = this.springWeathers;
			} else if (Season.Summer == season) {
				map.Biome.baseWeatherCommonalities = this.summerWeathers;
			} else if (Season.Fall == season) {
				map.Biome.baseWeatherCommonalities = this.fallWeathers;
			} else if (Season.Winter == season){
				map.Biome.baseWeatherCommonalities = this.winterWeathers;
			}
			return;

		}

		public void setDiseaseBySeason(Season season)
		{
			List<BiomeDiseaseRecord> seasonalDiseases = new List<BiomeDiseaseRecord>();
			if (Season.Spring == season)
			{
				seasonalDiseases = this.springDiseases;
			}
			else if (Season.Summer == season)
			{
				seasonalDiseases = this.summerDiseases;
			}
			else if (Season.Fall == season)
			{
				seasonalDiseases = this.fallDiseases;
			}
			else if (Season.Winter == season)
			{
				seasonalDiseases = this.winterDiseases;
			}

			for (int i = 0; i < seasonalDiseases.Count; i++)
			{
				BiomeDiseaseRecord diseaseRec = seasonalDiseases[i];
				IncidentDef disease = diseaseRec.diseaseInc;
				disease.baseChance = diseaseRec.commonality;
			}

		}

		public void setIncidentsBySeason(Season season)
		{
			List<TKKN_IncidentCommonalityRecord> seasonalIncidents = new List<TKKN_IncidentCommonalityRecord>();
			if (Season.Spring == season)
			{
				seasonalIncidents = this.springEvents;
			}
			else if (Season.Summer == season)
			{
				seasonalIncidents = this.summerEvents;
			}
			else if (Season.Fall == season)
			{
				seasonalIncidents = this.fallEvents;
			}
			else if (Season.Winter == season)
			{
				seasonalIncidents = this.winterEvents;
			}

			for (int i = 0; i < seasonalIncidents.Count; i++){
				TKKN_IncidentCommonalityRecord incidentRate = seasonalIncidents[i];
				IncidentDef incident = incidentRate.incident;
				incident.baseChance = incidentRate.commonality;
			}

		}
	}
}
