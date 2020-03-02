using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TKKN_NPS
{
	public class springData : IExposable
	{
		public string springID;
		public string biomeName;
		public int makeAnotherAt = 0;
		public int age = 0;
		public string status = "spawning";
		public float width = 0;


		public void ExposeData()
		{
			Scribe_Values.Look<string>(ref this.springID, "springID", "", true);
			Scribe_Values.Look<string>(ref this.biomeName, "biomeName", "", true);
			Scribe_Values.Look<int>(ref this.makeAnotherAt, "makeAnotherAt", 0, true);
			Scribe_Values.Look<int>(ref this.age, "age", 0, true);
			Scribe_Values.Look<string>(ref this.status, "status", "", true);
			Scribe_Values.Look<float>(ref this.width, "width", 0, true);
		}

	}
}
