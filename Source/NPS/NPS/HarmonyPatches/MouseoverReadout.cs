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
			if (!Settings.showTempReadout)
			{
				cellData cell = Find.VisibleMap.GetComponent<Watcher>().cellWeatherAffects[c];
				rect = new Rect(BotLeft.x, (float)UI.screenHeight - BotLeft.y - num, 999f, 999f);
				string label2 = "Temperature: " + cell.location.GetTemperature(Find.VisibleMap);
				Widgets.Label(rect, label2);
				num += 19f;
			}

			rect = new Rect(BotLeft.x, (float)UI.screenHeight - BotLeft.y - num, 999f, 999f);
			string label3 = "C: x-" + c.x.ToString() + " y-" + c.y.ToString() + " z-" + c.z.ToString();
			Widgets.Label(rect, label3);
			num += 19f;
			
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