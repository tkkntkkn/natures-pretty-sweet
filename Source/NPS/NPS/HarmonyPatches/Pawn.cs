using Verse;
using Harmony;
using RimWorld;

namespace TKKN_NPS
{
	
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
			HediffDef hediffDef = HediffDefOf.TKKN_Wetness;
			if (__instance.health.hediffSet.GetFirstHediffOfDef(hediffDef) == null)
			{
				Hediff hediff = HediffMaker.MakeHediff(hediffDef, __instance, null);
				hediff.Severity = 0;
				__instance.health.AddHediff(hediff, null, null);
			}
		}
	}
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

			TerrainDef terrain = __instance.Position.GetTerrain(__instance.Map);
			if ((terrain.defName == "TKKN_SaltField" || terrain.defName == "TKKN_Salted_Earth") && __instance.def.defName == "TKKN_giantsnail")
			{
				PatchTickPawn.BurnSnails(__instance);
				return;
			}

			if (!__instance.Dead)
			{
				PatchTickPawn.MakePaths(__instance);
				PatchTickPawn.MakeBreath(__instance);
			}
		}
		public static void BurnSnails(Pawn pawn)
		{
			BattleLogEntry_DamageTaken battleLogEntry_DamageTaken = new BattleLogEntry_DamageTaken(pawn, RulePackDefOf.DamageEvent_Fire, null);
			Find.BattleLog.Add(battleLogEntry_DamageTaken);
			DamageInfo dinfo = new DamageInfo(DamageDefOf.Flame, 100, -1f, pawn, null, null, DamageInfo.SourceCategory.ThingOrUnknown);
			dinfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
			pawn.TakeDamage(dinfo).InsertIntoLog(battleLogEntry_DamageTaken);
		}
		public static void MakePaths(Pawn pawn)
		{
			Map map = pawn.Map;
			Watcher watcher = map.GetComponent<Watcher>();
			if (watcher == null)
			{
				return;
			}
			#region paths
			if (pawn.Position.InBounds(map))
			{
				//damage plants and remove snow/frost where they are. This will hopefully generate paths as pawns walk :)
				if (watcher.checkIfCold(pawn.Position) && pawn.RaceProps.Humanlike)
				{
					map.GetComponent<FrostGrid>().AddDepth(pawn.Position, (float)-.05);
					map.snowGrid.AddDepth(pawn.Position, (float)-.05);
				}

				if (pawn.RaceProps.Humanlike)
				{
					//pack down the soil.
					cellData cell = watcher.cellWeatherAffects[pawn.Position];
					cell.doPack();
					if (Settings.allowPlantEffects)
					{
						watcher.hurtPlants(pawn.Position, true, true);
					}
				}
			}
			#endregion
		}

		public static void MakeBreath(Pawn pawn)
		{
			if (Find.TickManager.TicksGame % 150 == 0)
			{
				Map map = pawn.Map;
				Watcher watcher = map.GetComponent<Watcher>();

				bool isCold = watcher.checkIfCold(pawn.Position);
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
	[HarmonyPatch(typeof(Pawn))]
	[HarmonyPatch("TickRare")]
	class PatchTickRarePawn
	{

		[HarmonyPostfix]
		public static void Postfix(Pawn __instance)
		{
			if (__instance == null || !__instance.Spawned || __instance.Dead ||(__instance.RaceProps.Humanlike && __instance.needs == null))
			{
				return;
			}
			HediffDef hediffDef = new HediffDef();
			TerrainDef terrain = __instance.Position.GetTerrain(__instance.Map);
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
				__instance.needs.rest.CurLevel++;
				hediffDef = HediffDefOf.TKKN_coldspring_chill_out;
				if (__instance.health.hediffSet.GetFirstHediffOfDef(hediffDef, false) == null)
				{
					Hediff hediff = HediffMaker.MakeHediff(hediffDef, __instance, null);
					__instance.health.AddHediff(hediff, null, null);
				}
			}
		}
	}
}
