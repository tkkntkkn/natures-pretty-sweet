using Verse;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using TKKN_NPS.Workers;
using TKKN_NPS.SaveData;


namespace TKKN_NPS
{
	/*
	[HarmonyPatch(typeof(Pawn))]
	[HarmonyPatch("SpawnSetup")]
	class PatchSpawnSetupPawn
	{
		[HarmonyPostfix]
		public static void Postfix(Pawn __instance)
		{
			if (__instance == null || !__instance.Spawned || !__instance.RaceProps.Humanlike)
			{
				return;
			}
			
		}
	}
	*/

	[HarmonyPatch(typeof(Pawn))]
	[HarmonyPatch("Tick")]
	class PatchTickPawn
	{
		[HarmonyPostfix]
		public static void Postfix(Pawn __instance)
		{
			if (__instance == null || !__instance.Spawned)
			{
				return;
			}
			Map map = __instance.MapHeld;
			TerrainDef terrain = __instance.Position.GetTerrain(map);
			if ((terrain.defName == "TKKN_SaltField" || terrain.defName == "TKKN_Salted_Earth") && __instance.def.defName == "TKKN_giantsnail")
			{
				PatchTickPawn.BurnSnails(__instance);
				return;
			}

			if (!__instance.Dead)
			{
				if (!Find.TickManager.Paused)
				{
					IntVec3 c = __instance.Position;
					Watcher watcher = Worker.GetWatcher(map);
					CellData cell = watcher.GetCell(__instance.Position);

					if (cell == null)
					{
						return;
					}

					PatchTickPawn.MakePaths(__instance, cell);
					PatchTickPawn.MakeBreath(__instance, cell);
					PatchTickPawn.MakeWet(__instance, cell);
					LavaWorker.HurtWithLava(__instance);
					PatchTickPawn.DyingCheck(__instance, terrain);
				}
			}

			if (__instance == null || !__instance.Spawned || __instance.Dead || (__instance.RaceProps.Humanlike && __instance.needs == null))
			{
				return;
			}
			HediffDef hediffDef = new HediffDef();
			if (terrain.defName == "TKKN_HotSpringsWater")
			{
				if (__instance.needs.comfort != null)
				{
					__instance.needs.comfort.lastComfortUseTick--;
				}
				hediffDef = HediffDefOf.TKKN_hotspring_chill_out;
				if (__instance.health.hediffSet.GetFirstHediffOfDef(hediffDef) == null)
				{
					Hediff hediff = HediffMaker.MakeHediff(hediffDef, __instance, null);
					__instance.health.AddHediff(hediff, null, null);
				}
			}
			if (terrain.defName == "TKKN_ColdSpringsWater")
			{
				__instance.needs.rest.TickResting(.05f);
				hediffDef = HediffDefOf.TKKN_coldspring_chill_out;
				if (__instance.health.hediffSet.GetFirstHediffOfDef(hediffDef, false) == null)
				{
					Hediff hediff = HediffMaker.MakeHediff(hediffDef, __instance, null);
					__instance.health.AddHediff(hediff, null, null);
				}
			}

		}
		public static void DyingCheck(Pawn pawn, TerrainDef terrain)
		{
			//drowning == immobile and in water
			if (pawn == null || terrain == null)
			{
				return;
			}
			if (pawn.RaceProps.Humanlike && pawn.health.Downed && terrain.HasTag("TKKN_Wet"))
			{
				float damage = .0005f;
				//if they're awake, take less damage
				if (!pawn.health.capacities.CanBeAwake)
				{
					if (terrain.HasTag("TKKN_Swim")) {
						damage = .0001f;
					}
					else {
						return;
					}
				}

				//heavier clothing hurts them more
				List<Apparel> apparel = pawn.apparel.WornApparel;
				float weight = 0f;
				for (int i = 0; i < apparel.Count; i++)
				{
					weight += apparel[i].HitPoints / 10000;
				}
				damage += weight / 5000;
				HealthUtility.AdjustSeverity(pawn, HediffDef.Named("TKKN_Drowning"), damage);

				HediffDef hediffDef = HediffDefOf.TKKN_Drowning;
				if (pawn.Faction.IsPlayer && pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef) == null && pawn.RaceProps.Humanlike)
				{
					string text = "TKKN_NPS_DrowningText".Translate();
					Messages.Message(text, MessageTypeDefOf.NeutralEvent);

				}

			}
			else if (pawn.RaceProps.Humanlike && pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.TKKN_Drowning) != null) {
				pawn.health.RemoveHediff(pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.TKKN_Drowning));

			}
		}

		public static void MakeWet(Pawn pawn, CellData cell)
		{
			if (!Settings.allowPawnsToGetWet)
			{
				return;
			}
			HediffDef hediffDef = HediffDefOf.TKKN_Wetness;
			if (pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef) == null && pawn.RaceProps.Humanlike)
			{
				Map map = pawn.MapHeld;
				IntVec3 c = pawn.Position;

				bool isWet = false;
		
				if (TerrainWorker.IsLava(cell.currentTerrain))
				{
					//lava should dry them?
					isWet = false;
				}
				else {
					if (TerrainWorker.IsWaterTerrain(cell.currentTerrain))
					{
						isWet = true;
					}
					else
					{
						isWet = WeatherBaseWorker.AdjustWetBy(cell) > 0;
					}
				}

				if (isWet)
				{
					Hediff hediff = HediffMaker.MakeHediff(hediffDef, pawn, null);
					hediff.Severity = 0;
					pawn.health.AddHediff(hediff, null, null);
				}
			}
		}

		public static void BurnSnails(Pawn pawn)
		{
			BattleLogEntry_DamageTaken battleLogEntry_DamageTaken = new BattleLogEntry_DamageTaken(pawn, RulePackDefOf.DamageEvent_Fire, null);
			Find.BattleLog.Add(battleLogEntry_DamageTaken);
			DamageInfo dinfo = new DamageInfo(RimWorld.DamageDefOf.Flame, 100, -1f, 0, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown, pawn);
			dinfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
			pawn.TakeDamage(dinfo).AssociateWithLog(battleLogEntry_DamageTaken);
		}
		#region paths
		public static void MakePaths(Pawn pawn, CellData cell)
		{
			if (!Settings.doDirtPath)
			{
				return;
			}
			Map map = pawn.Map;
			if (pawn.Position.InBounds(map) && pawn.RaceProps.Humanlike)
			{
				//remove snow/frost where they are. This will hopefully generate paths as pawns walk :)
				if (cell.isCold)
				{
					FrostGrid fg = FrostWorker.GetFrostGrid(map);
					fg.AddDepth(pawn.Position, (float)-.05);
					map.snowGrid.AddDepth(pawn.Position, (float)-.05);
				}

				//pack down the soil only if the pawn is moving AND is in our colony
				if (pawn.pather.MovingNow && pawn.IsColonist)
				{
					cell.DoPack();
				}
			}
		}
		#endregion
		public static void MakeBreath(Pawn pawn, CellData cell)
		{
			if (!Settings.showCold)
			{
				return;
			}
			if (Find.TickManager.TicksGame % 150 == 0)
			{
				Map map = pawn.Map;

				bool isCold = cell.IsCold;

				if (isCold)
				{
					IntVec3 head = pawn.Position;
					head.z += 1;
					if (!head.ShouldSpawnMotesAt(map) || map.moteCounter.SaturatedLowPriority)
					{
						return;
					}
					MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDef.Named("TKKN_Mote_ColdBreath"), null);
					moteThrown.airTimeLeft = 99999f;
					moteThrown.Scale = Rand.Range(.5f, 1.5f);
					moteThrown.rotationRate = Rand.Range(-30f, 30f);
					moteThrown.exactPosition = head.ToVector3();
					moteThrown.SetVelocity((float)Rand.Range(20, 30), Rand.Range(0.5f, 0.7f));
					GenSpawn.Spawn(moteThrown, head, map);
				}
			}
		}
	}
	
}
