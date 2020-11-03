using RimWorld;
using System;
using Verse;
using System.Collections.Generic;


namespace TKKN_NPS
{
	public class ElementSpawnDef : Def
	{
		public ThingDef thingDef;
		public List<string> forbiddenBiomes;
		public List<string> allowedBiomes;

		public float commonality;

	}
}
