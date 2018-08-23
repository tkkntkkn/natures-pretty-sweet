using System;
using Verse;
using Verse.Noise;
using Harmony;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using RimWorld;

namespace TKKN_NPS
{

	[HarmonyPatch(typeof(Thing))]
	[HarmonyPatch("AmbientTemperature", PropertyMethod.Getter)]
	class PatchAmbientTemperature
	{

		[HarmonyPostfix]
		public static void Postfix(Thing __instance, ref float __result)
		{

			float temperature = __result;

			IntVec3 c = __instance.Position;
			Map map = __instance.Map;


			//check if we should have temperature affected by contact with terrain
			if (map != null && c.InBounds(map))
			{
				TerrainDef terrain = c.GetTerrain(map);
				if (terrain.HasModExtension<TerrainWeatherReactions>() && terrain.GetModExtension<TerrainWeatherReactions>().temperatureAdjust != 0)
				{
					temperature = temperature + terrain.GetModExtension<TerrainWeatherReactions>().temperatureAdjust;
				}
			}
			//adjust temperature
			Pawn pawn = __instance as Pawn;
			float adjustForWet = PatchAmbientTemperature.checkWetness(pawn);
			if (adjustForWet > 0)
			{
				temperature -= adjustForWet;
			}
			__result = temperature;

		}

		public static float checkWetness(Pawn pawn)
		{
			if (pawn == null || !pawn.Spawned || pawn.Dead || (pawn.RaceProps.Humanlike && pawn.needs == null))
			{
				return 0f;
			}

			HediffDef hediffDef = HediffDefOf.TKKN_Wetness;
			Hediff_Wetness wetness = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef) as Hediff_Wetness;
			if (wetness != null)
			{
				return wetness.wetnessLevel;
			}

			return 0f;
		}
	}
	

	[HarmonyPatch(typeof(Thing))]
	[HarmonyPatch("Tick")]
	class PatchTickThing{
		[HarmonyPostfix]
		public static void Postfix(Thing __instance)
		{
			IntVec3 c = __instance.Position;
			Map map = __instance.Map;
			TerrainDef terrain = c.GetTerrain(map);
		if (terrain != null && terrain.HasTag("Lava")){
			FireUtility.TryStartFireIn(c, map, 5f);

			float statValue = __instance.GetStatValue(StatDefOf.Flammability, true);
			bool alt = __instance.def.altitudeLayer == AltitudeLayer.Item;
			if (statValue == 0f && alt == true)
			{
				if (!__instance.Destroyed && __instance.def.destroyable)
				{
					__instance.Destroy(DestroyMode.Vanish);
				}
			}
		}
		}
	}
}