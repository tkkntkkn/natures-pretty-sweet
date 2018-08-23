using System;
using RimWorld;
using Verse;
using Harmony;
using UnityEngine;
using System.Collections.Generic;

namespace TKKN_NPS
{
	class PatchGraphic
	{

		[HarmonyPatch(typeof(Graphic_Shadow))]
		[HarmonyPatch("DrawWorker")]
		public static class PatchDrawWorker
		{

			[HarmonyPrefix]
			public static bool Prefix(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
			{
				Pawn pawn = thing as Pawn;
				if (pawn is Pawn && pawn != null){
					if (pawn.Position.IsValid)
					{
						TerrainDef terrain = pawn.Position.GetTerrain(pawn.Map);
						if (terrain != null && terrain.HasTag("Water") && terrain.defName.ToLower().Contains("deep"))
						{
							return false;
						}
					}
				}
				return true;
			}
		}
	}
}