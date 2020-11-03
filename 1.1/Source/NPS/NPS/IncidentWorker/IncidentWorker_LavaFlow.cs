using System;
using RimWorld;
using Verse;

namespace TKKN_NPS
{

    public class IncidentWorker_LavaFlow : IncidentWorker
    {
        private const float FogClearRadius = 4.5f;
        public ThingDef thingDef;

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
			Map map = (Map)parms.target;
			IntVec3 intVec = CellFinder.RandomNotEdgeCell(15, map);

			if (!Settings.allowLavaEruption)
			{
				return false;
			}
			else if (Settings.spawnLavaOnlyInBiome && map.Biome.defName != "TKKN_VolcanicFlow")
			{
				return false;
			}
            
            ThingWithComps lava = (ThingWithComps)GenSpawn.Spawn(ThingMaker.MakeThing(TKKN_NPS.ThingDefOf.TKKN_Lava_Spring, null), intVec, map);


            string label = "TKKN_NPS_LavaHasEruptedNearby".Translate();
            string text = "TKKN_NPS_LavaHasEruptedNearbyTxt".Translate();

            Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NeutralEvent, new TargetInfo(intVec, map, false), null);


            return true;
        }      
    }
}
