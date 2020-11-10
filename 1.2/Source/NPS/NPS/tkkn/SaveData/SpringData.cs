using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TKKN_NPS
{
	public class SpringData : IExposable
	{
		public string springID;
		public string biomeName;
		public int makeAnotherAt = 0;
		public int age = 0;
		public string status = "spawning";
		public float width = 0;


		public void ExposeData()
		{
			Scribe_Values.Look<string>(ref springID, "springID", "", true);
			Scribe_Values.Look<string>(ref biomeName, "biomeName", "", true);
			Scribe_Values.Look<int>(ref makeAnotherAt, "makeAnotherAt", 0, true);
			Scribe_Values.Look<int>(ref age, "age", 0, true);
			Scribe_Values.Look<string>(ref status, "status", "", true);
			Scribe_Values.Look<float>(ref width, "width", 0, true);
		}

	}
}
