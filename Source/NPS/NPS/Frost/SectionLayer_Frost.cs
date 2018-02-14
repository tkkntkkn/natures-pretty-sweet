using System;
using UnityEngine;
using Verse;

namespace TKKN_NPS
{

	internal class SectionLayer_Frost : SectionLayer
	{
		private float[] vertDepth = new float[9];

		private static readonly Color32 ColorClear = new Color32(194, 219, 249, 0); // 194, 219, 249

		private static readonly Color32 ColorWhite = new Color32(194, 219, 249, 120);

		public override bool Visible
		{
			get
			{
				return true;
			}
		}

		public SectionLayer_Frost(Section section) : base(section)
		{
			this.relevantChangeTypes = MapMeshFlag.Snow;
		}

		private bool Filled(int index)
		{
			Building building = base.Map.edificeGrid[index];
			return building != null && building.def.Fillage == FillCategory.Full;
		}

		public override void Regenerate()
		{
			LayerSubMesh subMesh = base.GetSubMesh(Verse.MatBases.Snow);
			//LayerSubMesh subMesh = base.GetSubMesh(MatBases.Frost); // for some reason the custom one was causing a huge memory issue :(
			if (subMesh.mesh.vertexCount == 0)
			{

//				SectionLayerGeometryMaker_Solid.MakeBaseGeometry(this.section, subMesh, AltitudeLayer.MoteLow);
				SectionLayerGeometryMaker_Solid.MakeBaseGeometry(this.section, subMesh, AltitudeLayer.ItemImportant); //so frost forms over items/plants

			}
			subMesh.Clear(MeshParts.Colors);

			float[] depthGridDirect_Unsafe = base.Map.GetComponent<FrostGrid>().DepthGridDirect_Unsafe;
			CellRect cellRect = this.section.CellRect;
			int num = base.Map.Size.z - 1;
			int num2 = base.Map.Size.x - 1;
			bool flag = false;

			CellIndices cellIndices = base.Map.cellIndices;
			for (int i = cellRect.minX; i <= cellRect.maxX; i++) // this is what renders it all blobby, I think
			{
				for (int j = cellRect.minZ; j <= cellRect.maxZ; j++)
				{
					float num3 = depthGridDirect_Unsafe[cellIndices.CellToIndex(i, j)];
					int num4 = cellIndices.CellToIndex(i, j - 1);
					float num5 = (j <= 0) ? num3 : depthGridDirect_Unsafe[num4];
					num4 = cellIndices.CellToIndex(i - 1, j - 1);
					float num6 = (j <= 0 || i <= 0) ? num3 : depthGridDirect_Unsafe[num4];
					num4 = cellIndices.CellToIndex(i - 1, j);
					float num7 = (i <= 0) ? num3 : depthGridDirect_Unsafe[num4];
					num4 = cellIndices.CellToIndex(i - 1, j + 1);
					float num8 = (j >= num || i <= 0) ? num3 : depthGridDirect_Unsafe[num4];
					num4 = cellIndices.CellToIndex(i, j + 1);
					float num9 = (j >= num) ? num3 : depthGridDirect_Unsafe[num4];
					num4 = cellIndices.CellToIndex(i + 1, j + 1);
					float num10 = (j >= num || i >= num2) ? num3 : depthGridDirect_Unsafe[num4];
					num4 = cellIndices.CellToIndex(i + 1, j);
					float num11 = (i >= num2) ? num3 : depthGridDirect_Unsafe[num4];
					num4 = cellIndices.CellToIndex(i + 1, j - 1);
					float num12 = (j <= 0 || i >= num2) ? num3 : depthGridDirect_Unsafe[num4];
					this.vertDepth[0] = (num5 + num6 + num7 + num3) / 4f;
					this.vertDepth[1] = (num7 + num3) / 2f;
					this.vertDepth[2] = (num7 + num8 + num9 + num3) / 4f;
					this.vertDepth[3] = (num9 + num3) / 2f;
					this.vertDepth[4] = (num9 + num10 + num11 + num3) / 4f;
					this.vertDepth[5] = (num11 + num3) / 2f;
					this.vertDepth[6] = (num11 + num12 + num5 + num3) / 4f;
					this.vertDepth[7] = (num5 + num3) / 2f;
					this.vertDepth[8] = num3;
					for (int k = 0; k < 9; k++)
					{
						if (this.vertDepth[k] > 0.01f)
						{
							flag = true;
						}
						subMesh.colors.Add(SectionLayer_Frost.FrostDepthColor(this.vertDepth[k]));
					}
				}
			}
			if (flag)
			{
				subMesh.disabled = false;
				subMesh.FinalizeMesh(MeshParts.Colors);
			}
			else
			{
				subMesh.disabled = true;
			}
		}




		private static Color32 FrostDepthColor(float FrostDepth)
		{
			return Color32.Lerp(SectionLayer_Frost.ColorClear, SectionLayer_Frost.ColorWhite, FrostDepth);
		}
	}
}
