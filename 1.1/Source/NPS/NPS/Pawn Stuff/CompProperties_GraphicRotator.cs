using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Noise;

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
			if (this.ticks % this.Props.howOften == 0)
			{
				Pawn pawn = this.parent as Pawn;
				if (pawn.pather.Moving)
				{
					//get the direction it's moving
					if (pawn.pather.curPath == null || pawn.pather.curPath.NodesLeftCount < 1)
					{
						return;
					}
					IntVec3 c = pawn.pather.nextCell - pawn.Position;
					if (c.x > 0)
					{
						curAngle += this.Props.howManyDegrees;
					}
					else if (c.x < 0)
					{
						curAngle -= this.Props.howManyDegrees;
					}
					else if (c.z > 0)
					{
						curAngle += this.Props.howManyDegrees;
					}
					else
					{
						curAngle -= this.Props.howManyDegrees;
					}
					if (curAngle > 360)
					{
						curAngle = 360 - curAngle;
					}
					else if (curAngle < 0)
					{
						curAngle = 360 + curAngle;
					}
					pawn.Drawer.renderer.wiggler.SetToCustomRotation(curAngle);
				}
			}
		}

	}
}