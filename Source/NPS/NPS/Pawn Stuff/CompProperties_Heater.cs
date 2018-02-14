using Verse;
using RimWorld;

namespace TKKN_NPS
{
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