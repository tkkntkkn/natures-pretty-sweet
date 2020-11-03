using System;
using Verse;
using RimWorld;

namespace TKKN_NPS
{
	[DefOf]
	public static class GameConditionDefOf
	{
		public static GameConditionDef TKKN_NPS;

		static GameConditionDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(GameConditionDefOf));
		}
	}
}
