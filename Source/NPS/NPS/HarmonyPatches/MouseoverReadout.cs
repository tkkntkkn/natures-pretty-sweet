using System;
using Verse;
using Harmony;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace TKKN_NPS
{


	[HarmonyPatch(typeof(MouseoverReadout))]
	[HarmonyPatch("MouseoverReadoutOnGUI")]
	class PatchMouseoverReadout
	{
		static void Postfix()
		{
			//			Log.Warning("Window: " + window);
			IntVec3 c = UI.MouseCell();
			if (!c.InBounds(Find.VisibleMap))
			{
				return;
			}
			Rect rect;
			Vector2 BotLeft = new Vector2(15f, 65f);
			float num = 38f;
			Zone zone = c.GetZone(Find.VisibleMap);
			if (zone != null)
			{
				num += 19f;
			}
			float depth = Find.VisibleMap.snowGrid.GetDepth(c);
			if (depth > 0.03f)
			{
				num += 19f;
			}
			List<Thing> thingList = c.GetThingList(Find.VisibleMap);
			for (int i = 0; i < thingList.Count; i++)
			{
				Thing thing = thingList[i];
				if (thing.def.category != ThingCategory.Mote)
				{
					num += 19f;
				}
			}
			RoofDef roof = c.GetRoof(Find.VisibleMap);
			if (roof != null)
			{
				num += 19f;
			}
			if (Settings.showTempReadout)
			{
				rect = new Rect(BotLeft.x, (float)UI.screenHeight - BotLeft.y - num, 999f, 999f);
				string label3 = "C: x-" + c.x.ToString() + " y-" + c.y.ToString() + " z-" + c.z.ToString();
				Widgets.Label(rect, label3);
				num += 19f;

				Map map = Find.VisibleMap;
				Watcher watcher = map.GetComponent<Watcher>();
				cellData cell = watcher.cellWeatherAffects[c];
				rect = new Rect(BotLeft.x, (float)UI.screenHeight - BotLeft.y - num, 999f, 999f);
				string label2 = "Temperature: " + cell.temperature;
				Widgets.Label(rect, label2);
				num += 19f;

				rect = new Rect(BotLeft.x, (float)UI.screenHeight - BotLeft.y - num, 999f, 999f);
				string label4 = "Cell Info: Base Terrain " + cell.baseTerrain.defName + " | Wet " + cell.isWet.ToString() + " | Melt " + cell.isMelt.ToString() + " | Flooded " + cell.isFlooded.ToString() + " | Frozen " + cell.isFrozen.ToString() + " | Thawed " + cell.isThawed.ToString() + " | Getting Wet? " + cell.gettingWet.ToString() ;
				Widgets.Label(rect, label4);
				num += 19f;

				rect = new Rect(BotLeft.x, (float)UI.screenHeight - BotLeft.y - num, 999f, 999f);
				string label5 = "Cell Info: howWet " + cell.howWet.ToString() +  " | How Wet (Plants) " + cell.howWetPlants.ToString();
				if (cell.weather != null)
				{
					if (cell.weather.wetTerrain != null)
					{
						label5 += " | T Wet " + cell.weather.wetTerrain.defName;
					}
					if (cell.weather.dryTerrain != null)
					{
						label5 += " | T Dry " + cell.weather.dryTerrain.defName;
					}
					if (cell.weather.freezeTerrain != null)
					{
						label5 += " | T Freeze " + cell.weather.freezeTerrain.defName;
					}
				}
				if (cell.originalTerrain != null)
				{
					label5 += " | Orig Terrain " + cell.originalTerrain.defName;
				}
				Widgets.Label(rect, label5);
				num += 19f;

			}


			depth = Find.VisibleMap.GetComponent<FrostGrid>().GetDepth(c);
			if (depth > 0.01f)
			{
				rect = new Rect(BotLeft.x, (float)UI.screenHeight - BotLeft.y - num, 999f, 999f);
				FrostCategory frostCategory = FrostUtility.GetFrostCategory(depth);
				string label2 = FrostUtility.GetDescription(frostCategory);
				Widgets.Label(rect, label2);
			//	Widgets.Label(rect, label2 + " " + depth.ToString());
				num += 19f;
			}

		}
	}
}