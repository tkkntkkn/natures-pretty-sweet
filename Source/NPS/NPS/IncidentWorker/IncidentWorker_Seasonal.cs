using RimWorld;
using Verse;
using System.Linq;
using System.Collections.Generic;
using Verse.AI;
using Verse.AI.Group;

namespace TKKN_NPS
{

	public class IncidentWorker_TumbleweedMigration : IncidentWorker
	{
		private static readonly IntRange AnimalsCount = new IntRange(50, 70);

		protected override bool CanFireNowSub(IIncidentTarget target)
		{
			Map map = (Map)target;
			PawnKindDef pawnKindDef;
			IntVec3 intVec;
			IntVec3 intVec2;
			return this.TryFindAnimalKind(map.Tile, out pawnKindDef) && this.TryFindStartAndEndCells(map, out intVec, out intVec2);
		}
		private bool TryFindAnimalKind(int tile, out PawnKindDef animalKind)
		{

			return (from k in DefDatabase<PawnKindDef>.AllDefs
					where k.defName == "TKKN_Tumbleweed"
					select k).TryRandomElementByWeight((PawnKindDef x) => x.RaceProps.wildness, out animalKind); ;
		}
		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			PawnKindDef pawnKindDef;
			if (!this.TryFindAnimalKind(map.Tile, out pawnKindDef))
			{
				return false;
			}
			IntVec3 intVec;
			IntVec3 near;
			if (!this.TryFindStartAndEndCells(map, out intVec, out near))
			{
				return false;
			}
			Rot4 rot = Rot4.FromAngleFlat((map.Center - intVec).AngleFlat);
			List<Pawn> list = this.GenerateAnimals(pawnKindDef, map.Tile);
			for (int i = 0; i < list.Count; i++)
			{
				Pawn newThing = list[i];
				IntVec3 loc = CellFinder.RandomClosewalkCellNear(intVec, map, 10, null);
				GenSpawn.Spawn(newThing, loc, map, rot, false);
			}
			LordMaker.MakeNewLord(null, new LordJob_ExitMapNear(near, LocomotionUrgency.Walk, 12f, false, false), map, list);
			string text = string.Format(this.def.letterText, pawnKindDef.GetLabelPlural(-1)).CapitalizeFirst();
			string label = string.Format(this.def.letterLabel, pawnKindDef.GetLabelPlural(-1).CapitalizeFirst());
			Find.LetterStack.ReceiveLetter(label, text, this.def.letterDef, list[0], null);
			return true;
		}
		private bool TryFindStartAndEndCells(Map map, out IntVec3 start, out IntVec3 end)
		{
			if (!RCellFinder.TryFindRandomPawnEntryCell(out start, map, CellFinder.EdgeRoadChance_Animal, null))
			{
				end = IntVec3.Invalid;
				return false;
			}
			end = IntVec3.Invalid;
			for (int i = 0; i < 8; i++)
			{
				IntVec3 startLocal = start;
				IntVec3 intVec;
				if (!CellFinder.TryFindRandomEdgeCellWith((IntVec3 x) => map.reachability.CanReach(startLocal, x, PathEndMode.OnCell, TraverseMode.NoPassClosedDoors, Danger.Deadly), map, CellFinder.EdgeRoadChance_Ignore, out intVec))
				{
					break;
				}
				if (!end.IsValid || intVec.DistanceToSquared(start) > end.DistanceToSquared(start))
				{
					end = intVec;
				}
			}
			return end.IsValid;
		}

		private List<Pawn> GenerateAnimals(PawnKindDef animalKind, int tile)
		{
			int randomInRange = IncidentWorker_TumbleweedMigration.AnimalsCount.RandomInRange;
			List<Pawn> list = new List<Pawn>();
			for (int i = 0; i < randomInRange; i++)
			{
				PawnGenerationRequest request = new PawnGenerationRequest(animalKind, null, PawnGenerationContext.NonPlayer, tile, false, false, false, false, true, false, 1f, false, true, true, false, false, false, false, null, null, null, null, null, null, null);
				Pawn item = PawnGenerator.GeneratePawn(request);
				list.Add(item);
			}
			return list;
		}
	}


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