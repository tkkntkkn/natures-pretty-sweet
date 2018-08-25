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
	[HarmonyPatch(typeof(GenTemperature), "ComfortableTemperatureRange", new Type[] { typeof(Pawn)})]
	class PatchComfortableTemperatureRange
	{

		[HarmonyPostfix]
		public static void Postfix(Pawn p, ref FloatRange __result)
		{
			if (Find.TickManager.Paused)
			{
				return;
			}
			if (!p.RaceProps.Humanlike)
			{
				return;
			}
			HediffDef hediffDef = HediffDefOf.TKKN_Wetness;
			Hediff_Wetness wetness = p.health.hediffSet.GetFirstHediffOfDef(hediffDef) as Hediff_Wetness;
			if (wetness != null)
			{
				int setTo = PatchComfortableTemperatureRange.getOffSet(wetness);
				if (setTo > 0) {
					//they are comfortable only at higher temp
					FloatRange old = __result;
					__result.min += setTo;
					__result.max += setTo;
					return;
				}

				return;
			}

		}

		public static int getOffSet(Hediff_Wetness wetness){
			int setTo = 40;
			if (wetness.Severity == 0)
			{
				setTo = 0;
			}
			else if(wetness.Severity < .04)
			{
				setTo = 5;

			}
			else if (wetness.Severity < .2)
			{
				setTo = 10;
			}
			 else if (wetness.Severity < .35)
			{
				setTo = 25;
			}
				
			return setTo;

		}
	}
}

