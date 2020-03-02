using System;
using UnityEngine;
using Verse;

namespace TKKN_NPS
{
	public sealed class FrostGrid : MapComponent
	{

		public float[] depthGrid;

		private double totalDepth;

		public const float MaxDepth = 1f;
		public const float currentFrost = 0f;

		internal float[] DepthGridDirect_Unsafe
		{
			get
			{
				return this.depthGrid;
			}
		}

		public float TotalDepth
		{
			get
			{
				return (float)this.totalDepth;
			}
		}

		public FrostGrid(Map map) : base(map)
		{
			this.map = map;
			this.depthGrid = new float[map.cellIndices.NumGridCells];
		}


		private bool CanHaveFrost(int ind)
		{
			Building building = this.map.edificeGrid[ind];

			if (building != null && !map.GetComponent<FrostGrid>().CanCoexistWithFrost(building.def))
			{
				return false;
			}

			TerrainDef terrainDef = this.map.terrainGrid.TerrainAt(ind);
			if (terrainDef.HasModExtension<TerrainWeatherReactions>())
			{
				return terrainDef.GetModExtension<TerrainWeatherReactions>().holdFrost;
			}
			else
			{
				return terrainDef.holdSnow;
				//return terrainDef.affordances.Contains(TerrainAffordance.Light);

			}
		}

		public bool CanCoexistWithFrost(ThingDef def)
		{
			return def.category != ThingCategory.Building; // || def.Fillage != FillCategory.Full;
		}

		public void AddDepth(IntVec3 c, float depthToAdd)
		{
			if (!c.InBounds(this.map)) {
				return;
			}
			int num = this.map.cellIndices.CellToIndex(c);
			float num2 = this.depthGrid[num];
			if (num2 <= 0f && depthToAdd < 0f)
			{
				return;
			}
			if (num2 >= 0.999f && depthToAdd > 1f)
			{
				return;
			}
			if (!this.CanHaveFrost(num))
			{
				this.depthGrid[num] = 0f;
				return;
			}
			float num3 = num2 + depthToAdd;
			num3 = Mathf.Clamp(num3, 0f, 1f);
			float num4 = num3 - num2;
			this.totalDepth += (double)num4;
			if (Mathf.Abs(num4) > 0.0001f)
			{
				this.depthGrid[num] = num3;
				this.CheckVisualOrPathCostChange(c, num2, num3);
								
			}
		}

		public void SetDepth(IntVec3 c, float newDepth)
		{
			if (!c.InBounds(this.map)){
				return;
			}
			int num = this.map.cellIndices.CellToIndex(c);
			if (!this.CanHaveFrost(num))
			{
				this.depthGrid[num] = 0f;
				return;
			}
			newDepth = Mathf.Clamp(newDepth, 0f, 1f);
			float num2 = this.depthGrid[num];
			this.depthGrid[num] = newDepth;
			float num3 = newDepth - num2;
			this.totalDepth += (double)num3;
			this.CheckVisualOrPathCostChange(c, num2, newDepth);
		}

		private void CheckVisualOrPathCostChange(IntVec3 c, float oldDepth, float newDepth)
		{
			this.map.GetComponent<Watcher>().cellWeatherAffects[c].frostLevel = newDepth;
			if (!Mathf.Approximately(oldDepth, newDepth))
			{
				if (Mathf.Abs(oldDepth - newDepth) > 0.15f || Rand.Value < 0.0125f)
				{
					this.map.mapDrawer.MapMeshDirty(c, MapMeshFlag.Snow, true, false);
					this.map.mapDrawer.MapMeshDirty(c, MapMeshFlag.Things, true, false);
				}
				else if (newDepth == 0f)
				{
					this.map.mapDrawer.MapMeshDirty(c, MapMeshFlag.Snow, true, false);
				}
			}
		}

		public float GetDepth(IntVec3 c)
		{
			if (!c.InBounds(this.map))
			{
				return 0f;
			}
			return this.depthGrid[this.map.cellIndices.CellToIndex(c)];
		}

		public FrostCategory GetCategory(IntVec3 c)
		{
			return FrostUtility.GetFrostCategory(this.GetDepth(c));
		}
	}
}
