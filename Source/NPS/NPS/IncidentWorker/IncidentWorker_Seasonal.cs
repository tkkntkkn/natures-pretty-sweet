using RimWorld;
using Verse;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace TKKN_NPS
{

	public class IncidentWorker_Dustdevil : IncidentWorker
	{
		private const int MinDistanceFromMapEdge = 30;

		private const float MinWind = 1f;

		protected override bool CanFireNowSub(IIncidentTarget target)
		{
			Map map = (Map)target;
			return map.weatherManager.CurWindSpeedFactor >= 1f;
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			CellRect cellRect = CellRect.WholeMap(map).ContractedBy(30);
			if (cellRect.IsEmpty)
			{
				cellRect = CellRect.WholeMap(map);
			}
			IntRange devils = new IntRange(1, 5);
			int randomInRange = devils.RandomInRange;
			for (int i = 0; i < randomInRange; i++) {
				IntVec3 loc;
				if (!CellFinder.TryFindRandomCellInsideWith(cellRect, (IntVec3 x) => this.CanSpawnDustDevilAt(x, map), out loc))
				{
					return false;
				}
				DustDevil t = (DustDevil)GenSpawn.Spawn(ThingDefOf.TKKN_DustDevil, loc, map);
				base.SendStandardLetter(t, new string[0]);
			}
			return true;
		}

		private bool CanSpawnDustDevilAt(IntVec3 c, Map map)
		{
			if (c.Fogged(map))
			{
				return false;
			}
			int num = GenRadial.NumCellsInRadius(7f);
			for (int i = 0; i < num; i++)
			{
				IntVec3 c2 = c + GenRadial.RadialPattern[i];
				if (c2.InBounds(map))
				{
					if (this.AnyPawnOfPlayerFactionAt(c2, map))
					{
						return false;
					}
				}
			}
			return true;
		}

		private bool AnyPawnOfPlayerFactionAt(IntVec3 c, Map map)
		{
			List<Thing> thingList = c.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				Pawn pawn = thingList[i] as Pawn;
				if (pawn != null && pawn.Faction == Faction.OfPlayer)
				{
					return true;
				}
			}
			return false;
		}
	}
}

namespace TKKN_NPS
{

	public class IncidentWorker_WildFlowerBloom : IncidentWorker_Bloom
	{
		public string label = "TKKN_NPS_WildFlowerBloom".Translate();
		public string text = "TKKN_NPS_WildFlowerBloomTxt".Translate();
		public ThingDef thingDef;
		
	}
}

namespace TKKN_NPS
{

	public class IncidentWorker_Superbloom : IncidentWorker_Bloom
	{

		public string label = "TKKN_NPS_Superbloom".Translate();
		public string text = "TKKN_NPS_SuperbloomTxt".Translate();
		public ThingDef thingDef;

	
	}
}

namespace TKKN_NPS
{

	public class IncidentWorker_Bloom : IncidentWorker
	{
		public string label;
		public string text;
		public ThingDef thingDef;

		protected override bool TryExecuteWorker(IncidentParms parms)
		{

			if (!Settings.allowPlantEffects)
			{
				return false;
			}

			Map map = (Map)parms.target;
			IntVec3 intVec = CellFinder.RandomNotEdgeCell(15, map);

			//can the biome support it?
			bool canBloomHere = true;
			BiomeSeasonalSettings biomeSettings = map.Biome.GetModExtension<BiomeSeasonalSettings>();
			List<ThingDef> bloomPlants = biomeSettings.bloomPlants.ToList<ThingDef>();
			if (bloomPlants.Count == 0)
			{
				return false;
			}

			if (!canBloomHere)
			{
				return false;
			}



			Find.LetterStack.ReceiveLetter(this.label, this.text, LetterDefOf.NeutralEvent);


			return true;
		}
	}
}