using System;
using RimWorld;
using Verse;
using Verse.Sound;

namespace TKKN_NPS
{

	public class PlaceWorker_OnSteamVent : PlaceWorker
	{
		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null)
		{
			Thing thing = map.thingGrid.ThingAt(loc, TKKN_NPS.ThingDefOf.TKKN_SteamVent);
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
            this.steamSprayer = new IntermittentSteamSprayer(this);
            this.steamSprayer.startSprayCallback = this.StartSpray;
            this.steamSprayer.endSprayCallback = this.EndSpray;
        }

        private void StartSpray()
        {
            SnowUtility.AddSnowRadial(this.OccupiedRect().RandomCell, base.Map, 4f, -0.06f);
            this.spraySustainer = SoundDefOf.GeyserSpray.TrySpawnSustainer(new TargetInfo(base.Position, base.Map, false));
            this.spraySustainerStartTick = Find.TickManager.TicksGame;
        }

        private void EndSpray()
        {
            if (this.spraySustainer != null)
            {
                this.spraySustainer.End();
                this.spraySustainer = null;
            }
        }

        public override void Tick()
        {
            age++;
            if (this.harvester == null)
            {
                this.steamSprayer.SteamSprayerTick();
            }
            if (this.spraySustainer != null && Find.TickManager.TicksGame > this.spraySustainerStartTick + 1000)
            {
                Log.Message("Geyser spray sustainer still playing after 1000 ticks. Force-ending.");
                this.spraySustainer.End();
                this.spraySustainer = null;
            }

            
            if (Rand.Value < .000001f){
				this.DeSpawn();
            }

        }
    }
	

}
