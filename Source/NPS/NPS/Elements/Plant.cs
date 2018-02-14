using System;
using Verse;
using Harmony;
using RimWorld;

namespace TKKN_NPS
{
	//add a frost overlay onto plants and stuff
	[HarmonyPatch(typeof(Plant))]
	[HarmonyPatch("Graphic", PropertyMethod.Getter)]
	class PatchPlant
	{

		[HarmonyPatch(typeof(Plant))]
		[HarmonyPatch("Graphic", PropertyMethod.Getter)]
		public static class PlantGraphicPatch
		{

			[HarmonyPostfix]
			public static void Postfix(Plant __instance, ref Graphic __result)
			{
				string id = __instance.def.defName + "frost";
				if (!__instance.def.HasModExtension<ThingWeatherReaction>())
				{
					return;
				}
				ThingWeatherReaction mod = __instance.def.GetModExtension<ThingWeatherReaction>();

				if (__instance.Map.GetComponent<FrostGrid>().GetDepth(__instance.Position) <= 0.4f)
				{
					return;
				}
				//if it's not leafless
				if (__instance.def.plant.leaflessGraphic != __result)
				{
					if (Watcher.graphicHolder.ContainsKey(id))
					{
						//only load the image once.
						__result = Watcher.graphicHolder[id];
					}
					else
					{
						Watcher.graphicHolder.Add(id, GraphicDatabase.Get(__instance.def.graphicData.graphicClass, mod.frostGraphicPath, __instance.def.graphic.Shader, __instance.def.graphicData.drawSize, __instance.def.graphicData.color, __instance.def.graphicData.colorTwo));
					}
				}
			}
		}
	}

}
