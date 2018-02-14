using System;
using UnityEngine;
using Verse;

namespace TKKN_NPS
{
	[StaticConstructorOnStartup]
	public static class MatBases
	{
		public static Texture frostTexture = ContentFinder<Texture2D>.Get("TKKN_NPS/Temperature/Frost");
//		public static Material Frost = new Material(Verse.MatBases.Snow);
		public static Material Frost {
			get {
				Material frost = new Material(Verse.MatBases.Snow);
				frost.mainTexture = MatBases.frostTexture;
				/*
				Log.ErrorOnce(frost.shader.name, 123);
				Color col = frost.color;
				col.a = .5f;
				frost.color = col;
				//	frost.shader = ShaderDatabase.Cutout;

				/*
				Material frost = new Material(Verse.MatBases.Snow);
				//			subMesh.material.SetTexture("_TKKNFrostTex", ContentFinder<Texture2D>.Get(GenFilePaths.TexturesFolder+"TKKN_NPS/Surfaces/Ice.png", true));
				frost.mainTexture = ContentFinder<Texture2D>.Get("TKKN_NPS/Temperature/Frost");
				frost.shader = Shader.Find(Verse.ShaderType.Transparent);
			//	Color col = frost.mainTexture.color;
			//	col.a = .5f;
			//	frost.mainTexture.color = col;

	*/
			return frost;
		}
	}
	}
}
