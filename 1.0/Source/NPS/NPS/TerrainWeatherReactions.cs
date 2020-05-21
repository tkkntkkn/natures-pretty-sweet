using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
namespace TKKN_NPS
{
	public class TerrainWeatherReactions : DefModExtension
	{
		public TerrainDef tideTerrain;
		public TerrainDef floodTerrain;
		public TerrainDef wetTerrain;
		public TerrainDef freezeTerrain;
		public TerrainDef dryTerrain; //perm fix for wet soils getting bugged
		public TerrainDef baseOverride; //twmp fix for issue where wet soils weren't turning back to dry
		public int freezeAt;
		public int wetAt;
		public bool isSalty;
		public bool holdFrost;
		public float temperatureAdjust;

	}
}
