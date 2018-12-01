using Harmony;
using Verse;
using System;

namespace TKKN_NPS
{
	[HarmonyPatch(typeof(CellFinder))]
	[HarmonyPatch("TryRandomClosewalkCellNear")]
	public static class PatchTryRandomClosewalkCellNear
	{

		[HarmonyPrefix]
		public static bool Prefix(IntVec3 root, Map map, int radius, out IntVec3 result, Predicate<IntVec3> extraValidator, bool __result)
		{

			// don't enter on deep water
			__result = CellFinder.TryFindRandomReachableCellNear(root, map, (float)radius, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), (Predicate<IntVec3>)((IntVec3 c) => c.Standable(map) && !c.GetTerrain(map).HasTag("TKKN_Lava") && !c.GetTerrain(map).HasTag("TKKN_Swim")  && (extraValidator == null || extraValidator(c))), (Predicate<Region>)null, out result, 999999);
//			Log.Warning("result " + result.ToString());
			return false;
		}
	}

	[HarmonyPatch(typeof(CellFinder))]
	[HarmonyPatch("RandomCell")]
	public static class PatchRandomCell
	{

		[HarmonyPostfix]

		public static void Postfix(Map map, IntVec3 __result)
		{
			//trying to remove swim cells and lava cells from this, hopefully it'll stop a lot of stuff.
			TerrainDef terrain = __result.GetTerrain(map);
			if (terrain != null && (terrain.HasTag("TKKN_Swim") || terrain.HasTag("TKKN_Lava"))){
				//find a new cell
				while (true)
				{
					IntVec3 size = map.Size;
					int newX = Rand.Range(0, size.x);
					IntVec3 size2 = map.Size;
					IntVec3 cell = new IntVec3(newX, 0, Rand.Range(0, size2.z));

					TerrainDef cellTerrain = cell.GetTerrain(map);
					if (cellTerrain != null && !cellTerrain.HasTag("TKKN_Swim") && !cellTerrain.HasTag("TKKN_Lava")){
						__result = cell;
						break;
					}
				}
			}

		}
	}

}
