using System;
using RimWorld;
using Verse;

namespace TKKN_NPS
{

	public class IncidentWorker_WildFlowerBloom : IncidentWorker
	{
		public ThingDef thingDef;

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			IntVec3 intVec = CellFinder.RandomNotEdgeCell(15, map);

			if (!Settings.allowPlantEffects)
			{
				return false;
			}

			string label = "TKKN_NPS_WildFlowerBloom".Translate();
			string text = "TKKN_NPS_WildFlowerBloomTxt".Translate();

			Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NeutralEvent);


			return true;
		}
	}
}

namespace TKKN_NPS
{

	public class IncidentWorker_Superbloom : IncidentWorker
	{
		public ThingDef thingDef;

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			IntVec3 intVec = CellFinder.RandomNotEdgeCell(15, map);

			if (!Settings.allowPlantEffects)
			{
				return false;
			}

			string label = "TKKN_NPS_Superbloom".Translate();
			string text = "TKKN_NPS_SuperbloomTxt".Translate();

			Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NeutralEvent);


			return true;
		}
	}
}
