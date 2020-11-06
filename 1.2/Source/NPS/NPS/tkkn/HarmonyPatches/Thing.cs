using Verse;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using TKKN_NPS.SaveData;
using TKKN_NPS.Workers;

namespace TKKN_NPS
{

	[HarmonyPatch(typeof(Thing))]
	[HarmonyPatch("AmbientTemperature", MethodType.Getter)]
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

	[HarmonyPatch(typeof(Thing))]
	[HarmonyPatch("SpawnSetup")]
	class PatchSpawnSetupThing
	{
		//if it is in lava, destroy it
		[HarmonyPostfix]
		public static void Postfix(Thing __instance)
		{
			LavaWorker.HurtWithLava(__instance);
		}
	}
}