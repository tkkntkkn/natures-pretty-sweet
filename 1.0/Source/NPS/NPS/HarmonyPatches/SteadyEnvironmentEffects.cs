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
	[HarmonyPatch(typeof(SteadyEnvironmentEffects))]
	[HarmonyPatch("DoCellSteadyEffects")]
	class PatchDoCellSteadyEffects
	{

		static void Postfix(SteadyEnvironmentEffects __instance, IntVec3 c)
		{
			
			if (c == null)
			{
				return;
			}

			Map map = HarmonyMain.MapFieldInfo.GetValue(__instance) as Map;


			if (Rand.Value < .6)
			{
				Watcher watcher = map.GetComponent<Watcher>();
				watcher.doCellEnvironment(c);
			}
			
		}

		
	}
}