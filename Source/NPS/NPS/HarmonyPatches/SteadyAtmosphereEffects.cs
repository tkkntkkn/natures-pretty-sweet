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
	[HarmonyPatch(typeof(SteadyAtmosphereEffects))]
	[HarmonyPatch("DoCellSteadyEffects")]
	class PatchDoCellSteadyEffects
	{
		public static ModuleBase frostNoise;

		static void Postfix(SteadyAtmosphereEffects __instance, IntVec3 c)
		{
			
			if (c == null)
			{
				return;
			}
			Map map = HarmonyMain.MapFieldInfo.GetValue(__instance) as Map;
			if (map == null)
			{
				return;
			}

			Room room = c.GetRoom(map, RegionType.Set_All);
			bool flag = map.roofGrid.Roofed(c);
			bool flag2 = room != null && room.UsesOutdoorTemperature;

			if (room == null || flag2)
			{
				if (!flag && map.weatherManager.SnowRate > 0.001f)
				{
					//remove frost if snowing
					map.GetComponent<FrostGrid>().AddDepth(c, map.weatherManager.SnowRate * -.3f);
					//					map.GetComponent<FrostGrid>().AddDepth(c, -1 * 0.046f * map.weatherManager.SnowRate);
				}
				else if (room == null || flag2)
				{
					if (map.mapTemperature.OutdoorTemp < 0)
					{
						//add frost if freezing
						CreepFrostAt(c, 0.46f * .3f, map);
					}
					else
					{
						//remove frost if not
						float frosty = map.mapTemperature.OutdoorTemp * -.03f;
						//					float frosty = map.mapTemperature.OutdoorTemp * -.04f;
						map.GetComponent<FrostGrid>().AddDepth(c, frosty);
					}
				}
			}
		}

		public static void CreepFrostAt(IntVec3 c, float baseAmount, Map map)
		{
			if (frostNoise == null)
			{
				frostNoise = new Perlin(0.039999999105930328, 2.0, 0.5, 5, Rand.Range(0, 651431), QualityMode.Medium);
			}
			float num = frostNoise.GetValue(c);
			num += 1f;
			num *= 0.5f;
			if (num < 0.5f)
			{
				num = 0.5f;
			}
			float depthToAdd = baseAmount * num;

			map.GetComponent<FrostGrid>().AddDepth(c, depthToAdd);
		}




		public static float MeltAmountAt(float temperature)
		{
			if (temperature < 0f)
			{
				return 0f;
			}
			if (temperature < 10f)
			{
				return temperature * temperature * 0.0058f * 0.1f;
			}
			return temperature * 0.0058f;
		}

	}
}