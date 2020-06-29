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
	class Worker
	{
		public static Watcher GetWatcher(Map map)
		{
			if (map == null)
			{
				Log.Error("Called GetMapComponent on a null map");
				return null;
			}

			if (map.GetComponent<Watcher>() == null)
			{
				map.components.Add(new Watcher(map));
			}
			return map.GetComponent<Watcher>();
		}
	}
}
