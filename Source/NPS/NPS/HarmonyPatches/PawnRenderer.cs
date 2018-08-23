using System;
using RimWorld;
using Verse;
using Harmony;
using UnityEngine;
using System.Collections.Generic;

namespace TKKN_NPS
{
	[HarmonyPatch(typeof(PawnRenderer), "RenderPawnInternal", new Type[] { typeof(Vector3), typeof(Quaternion), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode), typeof(bool), typeof(bool) }), StaticConstructorOnStartup]
	class PatchRenderPawnInternal
	{
		[HarmonyPrefix]
		public static void Prefix(ref bool renderBody, PawnRenderer __instance)
		{
			Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
			if (pawn == null || !pawn.Position.IsValid || pawn.Dead)
			{
				return;
			}
			TerrainDef terrain = pawn.Position.GetTerrain(pawn.MapHeld);
			if (!terrain.HasTag("Water"))
			{
				return;
			}

			if (!Find.TickManager.Paused)
			{
				PatchRenderPawnInternal.dyingCheck(pawn, terrain);
			}

			if (terrain.defName.ToLower().Contains("deep"))
			{
				#region draw
				renderBody = false;

				#endregion

			}
			return;

		}

		public static void dyingCheck(Pawn pawn, TerrainDef terrain)
		{
			//drowning == immobile and in water
			if (pawn.health.Downed && terrain.HasTag("Water"))
			{
				float damage = 1f;
				//if they're awake, take less damage
				if (!pawn.health.capacities.CanBeAwake)
				{
					damage = .5f;
				}

				//heavier clothing hurts them more
				List<Apparel> apparel = pawn.apparel.WornApparel;
				float weight = 0f;
				for (int i = 0; i < apparel.Count; i++) {
					weight += apparel[i].HitPoints / 100;
				}
				damage += weight / 10;
				HealthUtility.AdjustSeverity(pawn, HediffDef.Named("TKKN_Drowning"), damage);
			}
		}
	}
}