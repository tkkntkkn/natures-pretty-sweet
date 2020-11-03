using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Noise;
using TKKN_NPS.SaveData;


namespace TKKN_NPS.Workers
{
	class FrostWorker : Worker
	{
		public static void DoFrost(CellData cell)
		{
			//frost effect
			float frostRate = 0.46f * (-1 * cell.temperature / 10);
			CreepFrostAt(cell.location, frostRate, cell.map);

		}

		public static void CreepFrostAt(IntVec3 c, float baseAmount, Map map)
		{
			Watcher watcher = GetWatcher(map);
			if (watcher.frostNoise == null)
			{
				watcher.frostNoise = new Perlin(0.039999999105930328, 2.0, 0.5, 5, Rand.Range(0, 651431), QualityMode.Medium);
			}
			float num = watcher.frostNoise.GetValue(c);
			num += 1f;
			num *= 0.5f;
			float depthToAdd = baseAmount * num;

			FrostGrid frostGrid = GetFrostGrid(map);
			frostGrid.AddDepth(c, depthToAdd);
		}

		public static FrostGrid GetFrostGrid(Map map)
		{
			if (map == null)
			{
				Log.Error("Called GetMapComponent on a null map");
				return null;
			}

			if (map.GetComponent<FrostGrid>() == null)
			{
				map.components.Add(new FrostGrid(map));
			}
			return map.GetComponent<FrostGrid>();
		}
	}
}
