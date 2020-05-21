using System;
using Harmony;
using RimWorld;
using System.Collections.Generic;
using Verse;
using System.Linq;
using System.Text;
using RimWorld.Planet;

namespace TKKN_NPS.HarmonyPatches
{
	[HarmonyPatch(typeof(WorldGenStep_Terrain), "BiomeFrom", new Type[] { typeof(Tile), typeof(int) })]
	class PatchBiomeFrom
	{
	   [HarmonyPostfix]
		public static void Postfix(Tile ws, int tileID, ref BiomeDef __result)
		{
			List<BiomeDef> allDefsListForReading = DefDatabase<BiomeDef>.AllDefsListForReading;
			BiomeDef biomeDef = null;
			float num = 0f;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				Log.ErrorOnce(allDefsListForReading[i].defName, i);
				BiomeDef biomeDef2 = allDefsListForReading[i];
				if (biomeDef2.implemented)
				{
					float score = biomeDef2.Worker.GetScore(ws, tileID);
					Log.ErrorOnce(score.ToString(), i + 100);
					if (score > num || biomeDef == null)
					{
						biomeDef = biomeDef2;
						num = score;
					}
				}
			}
		}
	}
}
