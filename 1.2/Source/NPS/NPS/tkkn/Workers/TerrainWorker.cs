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
	class TerrainWorker
	{


		/// <summary>
		/// Checks if the terrain is lava
		/// </summary>

		static public bool IsLava(CellData cell)
		{
			return IsLava(cell.currentTerrain);
		}

		/// <summary>
		/// Checks if the terrain is lava
		/// </summary>
		static public bool IsLava(TerrainDef terrain)
		{
			return terrain != null && terrain.HasTag("TKKN_Lava");
		}

		/// <summary>
		/// Checks if the terrain is water
		/// </summary>
		static public bool IsWaterTerrain(TerrainDef terrain)
		{
			//&& IsLava(terrain))
			return terrain != null  && (terrain.HasTag("TKKN_Wet") || terrain.defName.Contains("Water"));

		}


		/// <summary>
		/// Checks if the terrain is fresh water
		/// </summary>
		static public bool IsFreshWaterTerrain(TerrainDef terrain)
		{
			return IsWaterTerrain(terrain) && !terrain.defName.Contains("Ocean");
		}

		/// <summary>
		/// Checks if the terrain is fresh water
		/// </summary>
		static public bool IsLand(TerrainDef terrain)
		{
			//TKKN_SandBeachWetSalt - this is to keep the areas where the river meets the sea neater.
			return !IsWaterTerrain(terrain);
		}

		/// <summary>
		/// Checks if the terrain is Ocean by matching to TKKN_Ocean. Accepts a bool that returns only shallow Ocean water
		/// </summary>
		static public bool IsOceanTerrain(TerrainDef terrain, bool ignoreDeep = false)
		{
			if (terrain == null ) {
				return false;
			}

			if (terrain.HasTag("TKKN_Ocean"))
			{
				Log.Warning(terrain.HasTag("ShallowWater").ToString());
			}
			if (ignoreDeep && !terrain.defName.Contains("Shallow"))
			{
				return false;
			}
			return terrain.defName.Contains("Ocean");
		}

		/// <summary>
		/// Checks if the terrain is sand
		/// </summary>
		static public bool IsSandTerrain(TerrainDef terrain)
		{
			return terrain != null && terrain.defName == "Sand" || terrain.defName == "TKKN_SandBeachWetSalt";
		}
	}
}
