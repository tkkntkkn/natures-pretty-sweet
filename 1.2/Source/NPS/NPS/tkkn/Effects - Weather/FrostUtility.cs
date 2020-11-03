using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace TKKN_NPS
{
	class FrostUtility
	{
		public static FrostCategory GetFrostCategory(float FrostDepth)
		{
			if (FrostDepth < 0.1f)
			{
				return FrostCategory.None;
			}
			if (FrostDepth < 0.3f)
			{
				return FrostCategory.Frost;
			}
			if (FrostDepth < 0.5f)
			{
				return FrostCategory.Thin;
			}
			if (FrostDepth < 0.7f)
			{
				return FrostCategory.Medium;
			}
			return FrostCategory.Thick;
		}

		public static string GetDescription(FrostCategory category)
		{
			switch (category)
			{
				case FrostCategory.None:
					return "FrostNone".Translate();
				case FrostCategory.Dusting:
					return "FrostDusting".Translate();
				case FrostCategory.Thin:
					return "FrostThin".Translate();
				case FrostCategory.Medium:
					return "FrostMedium".Translate();
				case FrostCategory.Thick:
					return "FrostThick".Translate();
				default:
					return "Frost";
			}
		}

		public static int MovementTicksAddOn(FrostCategory category)
		{
			switch (category)
			{
				case FrostCategory.None:
					return 0;
				case FrostCategory.Dusting:
					return 0;
				case FrostCategory.Thin:
					return 0;
				case FrostCategory.Medium:
					return 1;
				case FrostCategory.Thick:
					return 2;
				default:
					return 0;
			}
		}		
	}
}
