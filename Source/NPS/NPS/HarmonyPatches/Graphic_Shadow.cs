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
					if (pawn.RaceProps.Humanlike && pawn.Position.IsValid)
					{
						TerrainDef terrain = pawn.Position.GetTerrain(pawn.MapHeld);
						if (terrain != null && terrain.HasTag("TKKN_Swim"))
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