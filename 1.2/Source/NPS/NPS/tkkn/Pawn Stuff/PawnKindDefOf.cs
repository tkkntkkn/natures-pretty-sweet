using Verse;
using RimWorld;

namespace TKKN_NPS
{
	[DefOf]
	public static class PawnKindDefOf
	{
		public static PawnKindDef TKKN_crab;

		static PawnKindDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(PawnKindDefOf));
		}
	}
}
