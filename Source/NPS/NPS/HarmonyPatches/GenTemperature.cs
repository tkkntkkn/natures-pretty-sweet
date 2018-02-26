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

		__result = temperature;

		}


	}
}