﻿using HarmonyLib;
using System.Reflection;
using Verse;
using RimWorld;

namespace TKKN_NPS
{
	[StaticConstructorOnStartup]
	class HarmonyMain
	{
		
		static HarmonyMain()
		{
			var harmony = new Harmony("com.github.tkkntkkn.Natures-Pretty-Sweet");
			harmony.PatchAll(Assembly.GetExecutingAssembly());

		}
	}
}
