using System;
using RimWorld;
using Verse;
using Verse.Sound;

namespace TKKN_NPS
{

	public class PlaceWorker_OnSteamVent : PlaceWorker
	{
		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
		{
//			Thing thing = map.thingGrid.ThingAt(loc, TKKN_NPS.ThingDefOf.TKKN_SteamVent);
			if (thing == null || thing.Position != loc)
			{
				return "TKKN_NPS_MustPlaceOnSteamVent".Translate();
			}
			return true;
		}

		public override bool ForceAllowPlaceOver(BuildableDef otherDef)
		{
			return otherDef == TKKN_NPS.ThingDefOf.TKKN_SteamVent;
		}
	}


	class Building_SteamVent : Building
	{
        private IntermittentSteamSprayer steamSprayer;

        public Building harvester = null;

        private Sustainer spraySustainer = null;

        private int spraySustainerStartTick = -999;
        private int age = 0;

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
            steamSprayer = new IntermittentSteamSprayer(this);
            steamSprayer.startSprayCallback = StartSpray;
            steamSprayer.endSprayCallback = EndSpray;
        }

        private void StartSpray()
        {
            SnowUtility.AddSnowRadial(this.OccupiedRect().RandomCell, base.Map, 4f, -0.06f);
            spraySustainer = SoundDefOf.GeyserSpray.TrySpawnSustainer(new TargetInfo(base.Position, base.Map, false));
            spraySustainerStartTick = Find.TickManager.TicksGame;
        }

        private void EndSpray()
        {
            if (spraySustainer != null)
            {
                spraySustainer.End();
                spraySustainer = null;
            }
        }

        public override void Tick()
        {
            age++;
            if (harvester == null)
            {
                steamSprayer.SteamSprayerTick();
            }
            if (spraySustainer != null && Find.TickManager.TicksGame > spraySustainerStartTick + 1000)
            {
                Log.Message("Geyser spray sustainer still playing after 1000 ticks. Force-ending.");
                spraySustainer.End();
                spraySustainer = null;
            }

            
            if (Rand.Value < .000001f){
				DeSpawn();
            }

        }
    }
	

}
