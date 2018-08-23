using System;
using Verse;
using Harmony;
using RimWorld;

namespace TKKN_NPS
{
	//add a frost overlay onto plants and stuff
	[HarmonyPatch(typeof(Plant))]
	[HarmonyPatch("Graphic", PropertyMethod.Getter)]
	public static class PatchGraphicPlant
	{

		[HarmonyPostfix]
		public static void Postfix(Plant __instance, ref Graphic __result)
		{
			//return;
			if (!Settings.showCold)
			{
				return;
			}
			string id = __instance.def.defName;

			if (!__instance.def.HasModExtension<ThingWeatherReaction>())
			{
				return;
			}

			ThingWeatherReaction mod = __instance.def.GetModExtension<ThingWeatherReaction>();

			string path = "";

			//get snow graphic
			if (__instance.Map.snowGrid.GetDepth(__instance.Position) >= 0.5f)
			{
				if (!String.IsNullOrEmpty(mod.snowGraphicPath))
				{
					id += "snow";
					path = mod.snowGraphicPath;
				}
			} else if (__instance.Map.GetComponent<FrostGrid>().GetDepth(__instance.Position) >= 0.6f)
			{
				if (!String.IsNullOrEmpty(mod.frostGraphicPath))
				{
					id += "frost";
					path = mod.frostGraphicPath;
				}
			}

			if (String.IsNullOrEmpty(path)) {
				return;
			}
			//if it's leafless
			if (__instance.def.plant.leaflessGraphic == __result)
			{
				id += "leafless";
				path = path.Replace("Frosted", "Frosted/Leafless");
				path = path.Replace("Snow", "Snow/Leafless");
				path += "_Leafless";
			}
			else if(__instance.def.blockWind)
			{
				//make it so snow doesn't fall under the tree until it's leafless.
			//	__instance.Map.snowGrid.AddDepth(__instance.Position, -.05f);
						
			}


			if (!__instance.Map.GetComponent<Watcher>().graphicHolder.ContainsKey(id))
			{
				//only load the image once.
				__instance.Map.GetComponent<Watcher>().graphicHolder.Add(id, GraphicDatabase.Get(__instance.def.graphicData.graphicClass, path, __instance.def.graphic.Shader, __instance.def.graphicData.drawSize, __instance.def.graphicData.color, __instance.def.graphicData.colorTwo));
			}
			if (__instance.Map.GetComponent<Watcher>().graphicHolder.ContainsKey(id))
			{
				//only load the image once.
				__result = __instance.Map.GetComponent<Watcher>().graphicHolder[id];
			}

		}

	}

}
