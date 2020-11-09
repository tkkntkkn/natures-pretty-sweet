using RimWorld;
using System;
using UnityEngine;
using Verse;
using HarmonyLib;
using TKKN_NPS.SaveData;
namespace TKKN_NPS
{
	//swap out plant graphics based on seasonal effects
	[HarmonyPatch(typeof(Plant))]
	[HarmonyPatch("Graphic", MethodType.Getter)]
	public static class PatchGraphicPlant
	{

		[HarmonyPostfix]
		public static void Postfix(Plant __instance, ref Graphic __result)
		{
			string id = __instance.def.defName;

			if (!__instance.def.HasModExtension<ThingWeatherReaction>())
			{
				return;
			}
			
			ThingWeatherReaction mod = __instance.def.GetModExtension<ThingWeatherReaction>();
			Map map = __instance.Map;

			string path = "";

			//get flowering or drought graphic if it's over 70
			if (__instance.AmbientTemperature > 21)
			{
				Watcher watcher = map.GetComponent<Watcher>();
				CellData cell = watcher.GetCell(__instance.Position);
				if (cell != null)
				{
					Vector2 location = Find.WorldGrid.LongLatOf(__instance.MapHeld.Tile);
					Season season = GenDate.Season((long)Find.TickManager.TicksAbs, location);

					float minWetnessToLive = (__instance.def.plant.fertilityMin * 5) + (__instance.def.plant.fertilitySensitivity * 2f);
					float minWetnessToFlower = (__instance.def.plant.fertilityMin * 5) + (__instance.def.plant.fertilitySensitivity * 3f);


					if (!String.IsNullOrEmpty(mod.floweringGraphicPath) && ((cell.HowWet > minWetnessToFlower) || season == Season.Spring))
					{
						id += "flowering";
						path = mod.floweringGraphicPath;
					}

					if (cell.HowWet < minWetnessToLive){
						id += "drought";
						if (!String.IsNullOrEmpty(mod.droughtGraphicPath))
						{
							path = mod.droughtGraphicPath;
						}
						else if(__instance.def.plant.leaflessGraphic != null)
						{
							path = __instance.def.plant.leaflessGraphic.path;
						}
					} 
				}
			}
			if (path != "")
			{
				if (!map.GetComponent<Watcher>().graphicHolder.ContainsKey(id))
				{
					//only load the image once.
					map.GetComponent<Watcher>().graphicHolder.Add(id, GraphicDatabase.Get(__instance.def.graphicData.graphicClass, path, __instance.def.graphic.Shader, __instance.def.graphicData.drawSize, __instance.def.graphicData.color, __instance.def.graphicData.colorTwo));
				}
				if (map.GetComponent<Watcher>().graphicHolder.ContainsKey(id))
				{
					__result = map.GetComponent<Watcher>().graphicHolder[id];
				}
				return;
			}

			if (Settings.showCold)
			{
				//get snow graphic
				if (map.snowGrid.GetDepth(__instance.Position) >= 0.5f)
				{
					if (__instance.def.plant.leaflessGraphic == __result && !String.IsNullOrEmpty(mod.snowLeaflessGraphicPath))
					{
						id += "snowleafless";
						path = mod.snowLeaflessGraphicPath;

					}
					else if (!String.IsNullOrEmpty(mod.snowGraphicPath))
					{
						id += "snow";
						path = mod.snowGraphicPath;
					}
				}
				else if (map.GetComponent<FrostGrid>().GetDepth(__instance.Position) >= 0.6f)
				{
					if (__instance.def.plant.leaflessGraphic == __result && !String.IsNullOrEmpty(mod.frostLeaflessGraphicPath))
					{
						id += "frostleafless";
						path = mod.frostLeaflessGraphicPath;

					}
					else if (!String.IsNullOrEmpty(mod.frostGraphicPath))
					{
						id += "frost";
						path = mod.frostGraphicPath;
					}
				}

				if (String.IsNullOrEmpty(path))
				{
					return;
				}
				//if it's leafless
				if (__instance.def.plant.leaflessGraphic != __result && __instance.def.blockWind)
				{
					//make it so snow doesn't fall under the tree until it's leafless.
					map.snowGrid.AddDepth(__instance.Position, -.05f);

				}
			}


			if (!map.GetComponent<Watcher>().graphicHolder.ContainsKey(id))
			{
				//only load the image once.
				map.GetComponent<Watcher>().graphicHolder.Add(id, GraphicDatabase.Get(__instance.def.graphicData.graphicClass, path, __instance.def.graphic.Shader, __instance.def.graphicData.drawSize, __instance.def.graphicData.color, __instance.def.graphicData.colorTwo));
			}
			if (map.GetComponent<Watcher>().graphicHolder.ContainsKey(id))
			{
				__result = map.GetComponent<Watcher>().graphicHolder[id];
			}
			return;



		}


	}

}