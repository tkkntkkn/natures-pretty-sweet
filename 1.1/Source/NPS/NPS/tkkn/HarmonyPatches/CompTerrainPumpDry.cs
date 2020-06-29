using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

/*
namespace TKKN_NPS.tkkn.HarmonyPatches
{
	[HarmonyPatch(typeof(CompTerrainPumpDry))]
	[HarmonyPatch("AffectCell")]
	public static class PatchAffectCell
	{
		[HarmonyPrefix]
		public static bool Prefix(Map map, IntVec3 c)
		{
			cellData cell = map.GetComponent<Watcher>().GetCell(c);
			TerrainDef terrain = c.GetTerrain(map);
			Traverse comp = Traverse.Create<CompTerrainPumpDry>();
			TerrainDef terrainToDryTo = comp.Method("GetTerrainToDryTo", map, terrain).GetValue<TerrainDef>();
			if (terrainToDryTo == null)
			{
				//use the base terrain, in case the cell has been flooded and frozen or something like that.
				terrain = cell.baseTerrain;
				terrainToDryTo = comp.Method("GetTerrainToDryTo", map, terrain).GetValue<TerrainDef>();
			}
			return false;
		}
	}
}
*/