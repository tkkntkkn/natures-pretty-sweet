using Harmony;
using Verse;
using System;
using RimWorld;
using Verse.AI;

namespace TKKN_NPS
{
	[HarmonyPatch(typeof(Pawn_PathFollower))]
	[HarmonyPatch("StartPath")]
	public static class PatchStartPath
	{
		[HarmonyPrefix]
		public static bool Prefix(Pawn_PathFollower __instance)
		{
			Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
			if (pawn.RaceProps.Animal)
			{
				TerrainDef terrain = pawn.Position.GetTerrain(pawn.Map);
				if (terrain.HasTag("TKKN_Swim") || terrain.HasTag("TKKN_Lava"))
				{
					if (!PatchStartPath.PawnCanOccupy(pawn.Position, pawn) && !PatchStartPath.TryRecoverFromUnwalkablePosition(true, pawn)) {
						return false;
					}
					//just get it out
//					Log.Warning(pawn.def.defName + " start path ");
//					Log.Warning(PatchStartPath.PawnCanOccupy(pawn.Position, pawn).ToString());
//					Log.Warning(PatchStartPath.TryRecoverFromUnwalkablePosition(true, pawn).ToString());
				}
			}
			return true;
		}

		public static bool TryRecoverFromUnwalkablePosition(bool error, Pawn pawn)
		{
			bool flag = false;
			for (int i = 0; i < GenRadial.RadialPattern.Length; i++)
			{
				IntVec3 intVec = pawn.Position + GenRadial.RadialPattern[i];
				if (PatchStartPath.PawnCanOccupy(intVec, pawn))
				{
					if (intVec == pawn.Position)
					{
						return true;
					}
					if (error)
					{
						Log.Warning(pawn + " on unwalkable cell " + pawn.Position + ". Teleporting to " + intVec, false);
					}
					pawn.Position = intVec;
					pawn.Notify_Teleported(true, false);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				pawn.Destroy(DestroyMode.Vanish);
				Log.Error(pawn.ToStringSafe() + " on unwalkable cell " + pawn.Position + ". Could not find walkable position nearby. Destroyed.", false);
			}
			return flag;
		}

		public static bool PawnCanOccupy(IntVec3 c, Pawn pawn)
		{
			if (!c.Walkable(pawn.Map))
			{
				return false;
			}

			Building edifice = c.GetEdifice(pawn.Map);
			if (edifice != null)
			{
				Building_Door building_Door = edifice as Building_Door;
				if (building_Door != null && !building_Door.PawnCanOpen(pawn) && !building_Door.Open)
				{
					return false;
				}
			}

			if (pawn.RaceProps.Animal)
			{
				if (c.GetTerrain(pawn.Map).HasTag("TKKN_Swim") || c.GetTerrain(pawn.Map).HasTag("TKKN_Lava"))
				{
					return false;
				}
			}
			return true;
		}


	}

	/*
	[HarmonyPatch(typeof(Pawn_PathFollower))]
	[HarmonyPatch("TryRecoverFromUnwalkablePosition")]
	public static class PatchTryRecoverFromUnwalkablePosition
	{
		[HarmonyPostfix]
		public static void Postfix(Pawn_PathFollower __instance, bool error, bool __result)
		{
			bool flag = false;
			Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
			for (int i = 0; i < GenRadial.RadialPattern.Length; i++)
			{
				IntVec3 intVec = pawn.Position + GenRadial.RadialPattern[i];
				if (PatchTryRecoverFromUnwalkablePosition.PawnCanOccupy(intVec, pawn))
				{
					if (intVec == pawn.Position)
					{
						Log.Warning(pawn.def.defName + " intvec = position");
						return;
					}
					Log.Warning(error.ToString());
					if (error)
					{
						Log.Warning(pawn + " on unwalkable cell " + pawn.Position + ". Teleporting to " + intVec, false);
					}
					Log.Warning(pawn.def.defName + " teleporting?");
					pawn.Position = intVec;
					pawn.Notify_Teleported(true, false);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				pawn.Destroy(DestroyMode.Vanish);
				Log.Error(pawn.ToStringSafe() + " on unwalkable cell " + pawn.Position + ". Could not find walkable position nearby. Destroyed.", false);
			}
			return;
		}

		public static bool PawnCanOccupy(IntVec3 c, Pawn pawn)
		{
			if (!c.Walkable(pawn.Map))
			{
				return false;
			}

			Building edifice = c.GetEdifice(pawn.Map);
			if (edifice != null)
			{
				Building_Door building_Door = edifice as Building_Door;
				if (building_Door != null && !building_Door.PawnCanOpen(pawn) && !building_Door.Open)
				{
					return false;
				}
			}

			if (pawn.RaceProps.Animal)
			{
				if (c.GetTerrain(pawn.Map).HasTag("TKKN_Swim") || c.GetTerrain(pawn.Map).HasTag("TKKN_Lava"))
				{
					Log.Warning("pawn " + pawn.def.defName + " cannot occupy ");
					return false;
				}
			}
			return true;
		}
	}


	[HarmonyPatch(typeof(Pawn_PathFollower))]
	[HarmonyPatch("PawnCanOccupy")]
	public static class PatchPawnCanOccupy
	{
		[HarmonyPostfix]
		public static void Postfix(Pawn_PathFollower __instance, IntVec3 c, bool __result)
		{
			if (__result == false)
			{
				return;
			}
			Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
			if (!pawn.RaceProps.Animal)
			{
				return;
			}
			if (c.GetTerrain(pawn.Map).HasTag("TKKN_Swim") || c.GetTerrain(pawn.Map).HasTag("TKKN_Lava"))
			{
			//	Log.Warning("pawn " + pawn.def.defName + " cannot occupy ");
				__result = false;
			}
		}

	}

	[HarmonyPatch(typeof(Pawn_PathFollower))]
	[HarmonyPatch("IsNextCellWalkable")]
	public static class PatchIsNextCellWalkable
	{
	
	[HarmonyPrefix]
	public static bool Prefix(Pawn_PathFollower __instance, bool __result)
	{
		Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
		if (pawn.RaceProps.Animal && pawn.Position.GetTerrain(pawn.Map).HasTag("TKKN_Swim"))
		{
			Log.Warning("pawn " + pawn.def.defName + " going false ");
			__result = false;
			return false;
		}
		return true;

	}	/*
		[HarmonyPrefix]
		public static bool Prefix(IntVec3 c, Pawn_PathFollower __instance, bool __result)
		{
			Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
			if (pawn.RaceProps.Animal && c.GetTerrain(pawn.Map).HasTag("TKKN_Swim"))
			{
				Log.Warning("pawn " + pawn.def.defName + " going false ");
				__result = false;
				return false;
			}
			return true;
		}
		*/
	//}
}
