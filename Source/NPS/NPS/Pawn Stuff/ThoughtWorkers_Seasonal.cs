using Verse;
using RimWorld;

namespace TKKN_NPS
{
    class TKKN_WildFlowerBloom : ThoughtWorker
    {

        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            return ThoughtState.ActiveAtStage(0);
        }
    }
}

namespace TKKN_NPS
{
	class ThoughtWorker_Superbloom : ThoughtWorker
	{

		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			return ThoughtState.ActiveAtStage(0);
		}
	}
}
