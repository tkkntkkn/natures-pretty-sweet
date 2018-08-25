using System;
using Verse;
using Harmony;
using RimWorld;

namespace TKKN_NPS
{
	//load the extra plant graphics
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
				if (!mod.droughtGraphicPath.NullOrEmpty())
				{
					string id = parentDef.defName + "drought";
					LongEventHandler.ExecuteWhenFinished(delegate
					{
						__instance.Map.GetComponent<Watcher>().graphicHolder.Add(id, GraphicDatabase.Get(parentDef.graphicData.graphicClass, mod.droughtGraphicPath, parentDef.graphic.Shader, parentDef.graphicData.drawSize, parentDef.graphicData.color, parentDef.graphicData.colorTwo));
					});

				}
				if (!mod.floweringGraphicPath.NullOrEmpty())
				{
					string id = parentDef.defName + "flowering";
					LongEventHandler.ExecuteWhenFinished(delegate
					{
						__instance.Map.GetComponent<Watcher>().graphicHolder.Add(id, GraphicDatabase.Get(parentDef.graphicData.graphicClass, mod.floweringGraphicPath, parentDef.graphic.Shader, parentDef.graphicData.drawSize, parentDef.graphicData.color, parentDef.graphicData.colorTwo));
					});
				}
			}

		}
	}
}