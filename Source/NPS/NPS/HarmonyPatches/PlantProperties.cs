using System;
using Verse;
using Harmony;
using RimWorld;

namespace TKKN_NPS
{
	//add a frost overlay onto plants and stuff
	[HarmonyPatch(typeof(PlantProperties), "PostLoadSpecial")]
	public static class PostLoadSpecialPatch
	{
		[HarmonyPostfix]
		public static void Postfix(Plant __instance, ThingDef parentDef)
		{
			if (parentDef.HasModExtension<ThingWeatherReaction>())
			{
				ThingWeatherReaction mod = parentDef.GetModExtension<ThingWeatherReaction>();

				if (!mod.frostGraphicPath.NullOrEmpty())
				{
					string id = parentDef.defName + "frost";
					LongEventHandler.ExecuteWhenFinished(delegate
					{
						__instance.Map.GetComponent<Watcher>().graphicHolder.Add(id, GraphicDatabase.Get(parentDef.graphicData.graphicClass, mod.frostGraphicPath, parentDef.graphic.Shader, parentDef.graphicData.drawSize, parentDef.graphicData.color, parentDef.graphicData.colorTwo));
					});
				}
			}

		}
	}
}