using System;
using Verse;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TKKN_NPS.SaveData;
using TKKN_NPS.Workers;

namespace TKKN_NPS
{


	[HarmonyPatch(typeof(MouseoverReadout))]
	[HarmonyPatch("MouseoverReadoutOnGUI")]
	class PatchMouseoverReadout
	{
		static void Postfix()
		{
			IntVec3 c = UI.MouseCell();
			Map map = Find.CurrentMap;
			if (!c.InBounds(map))
			{
				return;
			}
			Rect rect;
			Vector2 BotLeft = new Vector2(15f, 65f);
			float num = 38f;
			Zone zone = c.GetZone(map);
			if (zone != null)
			{
				num += 19f;
			}
			float depth = map.snowGrid.GetDepth(c);
			if (depth > 0.03f)
			{
				num += 19f;
			}
			List<Thing> thingList = c.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				Thing thing = thingList[i];
				if (thing.def.category != ThingCategory.Mote)
				{
					num += 19f;
				}
			}
			RoofDef roof = c.GetRoof(map);
			if (roof != null)
			{
				num += 19f;
			}
			if (Settings.showDevReadout)
			{
				Watcher watcher = Worker.GetWatcher(map);
				if (!watcher.cellWeatherAffects.TryGetValue(c, out CellData cell))
				{
					return;
				}

				rect = new Rect(BotLeft.x, (float)UI.screenHeight - BotLeft.y - num, 999f, 999f);
				string label3 = "C: x-" + c.x.ToString() + " y-" + c.y.ToString() + " z-" + c.z.ToString() + " | Tick -" + Find.TickManager.TicksGame.ToString();
				Widgets.Label(rect, label3);
				num += 19f;

				rect = new Rect(BotLeft.x, (float)UI.screenHeight - BotLeft.y - num, 999f, 999f);
				string label2 = "Temperature: " + cell.temperature + "  |  Humidity:" + Math.Round(cell.humidity, 2);

				//wetness debugging
				// label2 += " | Snow Rate: " + map.weatherManager.curWeather.snowRate.ToString() + " | Rain Rate:" + map.weatherManager.curWeather.rainRate + " | Cell AdjustWet " + WeatherBaseWorker.AdjustWetBy(cell).ToString() + " | Roofed | " + map.roofGrid.Roofed(c).ToString() + " | isWet: " + cell.IsWet.ToString();

				//temperature debugging
				// label2 += " | isCold: " + cell.IsCold.ToString();

				//temperature debuggin
				//				label2 += " | Roofed | " + map.roofGrid.Roofed(c).ToString();


				//flood debugging
								label2 += " | Cell's Flood Level: " + String.Join(", ", cell.floodLevel)  + " | Flood Level: " + FloodWorker.GetMaxFlood(FloodWorker.GetFloodType(map, watcher.floodThreat), FloodWorker.howManyFloodSteps);
				//tide debugging
								label2 += " | Cell's Tide Step: " + cell.tideStep.ToString() + " | Active Tide Step: " + watcher.tideLevel.ToString();

				Widgets.Label(rect, label2);
				num += 19f;

				rect = new Rect(BotLeft.x, (float)UI.screenHeight - BotLeft.y - num, 999f, 999f);
				string label5 = "Cell Info: Base Terrain: " + cell.baseTerrain.defName;

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
					if (cell.weather.floodTerrain != null)
					{
						label5 += " | T Flood " + cell.weather.floodTerrain.defName;
					}
					
				}
				if (cell.originalTerrain != null)
				{
					label5 += " | Orig Terrain " + cell.originalTerrain.defName;
				}
				Widgets.Label(rect, label5);
				num += 19f;

				rect = new Rect(BotLeft.x, (float)UI.screenHeight - BotLeft.y - num, 999f, 999f);
				string label6 = "TKKN_Wet: " + cell.currentTerrain.HasTag("TKKN_Wet") + " | HowWet:" + cell.HowWet.ToString() + " | How Packed:" + cell.howPacked.ToString() + " | Orig Terr Fresh" + TerrainWorker.IsFreshWaterTerrain(cell.originalTerrain).ToString();

		//		label6 += " | TKKN_Lava: " + cell.currentTerrain.HasTag("TKKN_Lava") + " | IsLava: " + TerrainWorker.IsLava(cell.currentTerrain) + " QTY Lava Cells" + (watcher.cellWeatherAffects.Select(key => key.Value).Where(cellTest => TerrainWorker.IsLava(cellTest.currentTerrain) == true).Count().ToString());
//				label6 += " | TKKN_Swim: " + cell.currentTerrain.HasTag("TKKN_Swim");
//				label6 += " | TKKN_Ocean: " + cell.currentTerrain.HasTag("TKKN_Ocean");
				Widgets.Label(rect, label6);
				num += 19f;
				



			}


			depth = map.GetComponent<FrostGrid>().GetDepth(c);
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

