using Verse;
using RimWorld;

namespace TKKN_NPS
{

	public abstract class CompProperties_GraphicRotatorCon : CompProperties
	{
		public int howManyDegrees = 5;
		public int howOften = 100;
	}

	public class CompProperties_GraphicRotator : CompProperties_GraphicRotatorCon
	{
		public CompProperties_GraphicRotator()
		{
			this.compClass = typeof(Comp_GraphicRotator);
		}
	}
	public class Comp_GraphicRotator : ThingComp
	{
		public int ticks = 0;
		public int curAngle = 0;
		public int turnDegree = 0;

		public CompProperties_GraphicRotator Props
		{
			get
			{
				return (CompProperties_GraphicRotator)this.props;
			}
		}

		public override void CompTick()
		{
			this.ticks++;
			if (this.ticks % this.Props.howOften != 0)
			{
				this.turnDegree = 0;
			}
			else
			{
				this.turnDegree = this.Props.howManyDegrees;
			}
		}

			public float getCurrentAngle()
		{
			Pawn pawn = this.parent as Pawn;
			if (Find.TickManager.Paused)
			{
				return curAngle;
			}
			if (pawn.pather.Moving)
			{
				//get the direction it's moving
				if (pawn.pather.curPath == null || pawn.pather.curPath.NodesLeftCount < 1)
				{
					return -1f;
				}
				IntVec3 c = pawn.pather.nextCell - pawn.Position;
				if (c.x > 0)
				{
					curAngle += turnDegree;
				}
				else if (c.x < 0)
				{
					curAngle += turnDegree;
				}
				else if (c.z > 0)
				{
					curAngle += turnDegree;
				}
				else
				{
					curAngle -= turnDegree;
				}
				if (curAngle > 360)
				{
					curAngle = 360 - curAngle;
				}
				else if (curAngle < 0)
				{
					curAngle = 360 + curAngle;
				}
				return curAngle;
			}
			return -1f;

		}

	}
		public abstract class CompProperties_HeaterCon : CompProperties
    {
        public int temperature = 6;
        public int howOften = 15;
    }

    public class CompProperties_Heater : CompProperties_HeaterCon
	{
        public CompProperties_Heater()
        {
            this.compClass = typeof(Comp_Heater);
        }
    }
    public class Comp_Heater : ThingComp
    {
        public int ticks = 0;

        public CompProperties_Heater Props
        {
            get
            {
                return (CompProperties_Heater)this.props;
            }
        }

        public override void CompTick()
        {
            this.ticks++;
            if (this.ticks % this.Props.howOften == 0)
            {
                GenTemperature.PushHeat(this.parent, this.Props.temperature);
                MoteMaker.ThrowFireGlow(this.parent.Position, this.parent.Map, 1);
                MoteMaker.ThrowSmoke(this.parent.Position.ToVector3(), this.parent.Map, 1);

            }
        }

    }
}