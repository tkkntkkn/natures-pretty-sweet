using RimWorld;
using Verse;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace TKKN_NPS
{

	public class IncidentWorker_WildFlowerBloom : IncidentWorker_Bloom
	{
		public string label = "TKKN_NPS_WildFlowerBloom".Translate();
		public string text = "TKKN_NPS_WildFlowerBloomTxt".Translate();
		public ThingDef thingDef;
		
	}
}

namespace TKKN_NPS
{

	public class IncidentWorker_Superbloom : IncidentWorker_Bloom
	{

		public string label = "TKKN_NPS_Superbloom".Translate();
		public string text = "TKKN_NPS_SuperbloomTxt".Translate();
		public ThingDef thingDef;

	
	}
}

namespace TKKN_NPS
{

	public class IncidentWorker_Bloom : IncidentWorker
	{
		public string label;
		public string text;
		public ThingDef thingDef;

		protected override bool TryExecuteWorker(IncidentParms parms)
		{

			if (!Settings.allowPlantEffects)
			{
				return false;
			}

			Map map = (Map)parms.target;
			IntVec3 intVec = CellFinder.RandomNotEdgeCell(15, map);

			//can the biome support it?
			bool canBloomHere = true;
			BiomeSeasonalSettings biomeSettings = map.Biome.GetModExtension<BiomeSeasonalSettings>();
			List<ThingDef> bloomPlants = biomeSettings.bloomPlants.ToList<ThingDef>();
			if (bloomPlants.Count == 0)
			{
				return false;
			}
			List<Season> allowedSeasons = biomeSettings.bloomSeasons.ToList<Season>();
			Vector2 location = Find.WorldGrid.LongLatOf(Find.VisibleMap.Tile);
			Season season = GenDate.Season((long)Find.TickManager.TicksAbs, location);

			for (int j = 0; j < allowedSeasons.Count; j++)
			{

				if (season != allowedSeasons[j])
				{
					canBloomHere = false;
					break;
				}
			}

			if (!canBloomHere)
			{
				return false;
			}



			Find.LetterStack.ReceiveLetter(this.label, this.text, LetterDefOf.NeutralEvent);


			return true;
		}
	}
}