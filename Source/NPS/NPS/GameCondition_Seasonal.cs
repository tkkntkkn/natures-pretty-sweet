using System.Linq;
using System.Collections.Generic;
using Verse;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;

namespace TKKN_NPS
{
	public class GameCondition_WildFlowerBloom : GameCondition_TKKNBlooms
	{
		public float howManyBlooms = 1.5f;
	} 
}

namespace TKKN_NPS
{
	public class GameCondition_Superbloom : GameCondition_TKKNBlooms
	{
		public float howManyBlooms = 3;
	}
}

namespace TKKN_NPS
{
	public class GameCondition_TKKNBlooms : GameCondition
	{
		public float howManyBlooms = 1;
		public override void DoCellSteadyEffects(IntVec3 c)
		{
			bool canBloomHere = true;
			//must be outdoors.

			BiomeSeasonalSettings biomeSettings = base.Map.Biome.GetModExtension<BiomeSeasonalSettings>();
			List<ThingDef> bloomPlants = biomeSettings.bloomPlants.ToList<ThingDef>();
			if (bloomPlants.Count == 0)
			{
				return;
			}

			if (!canBloomHere)
			{
				return;
			}

			Room room = c.GetRoom(base.Map, RegionType.Set_All);
			if (room != null)
			{
				return;
			}


			TerrainDef terrain = c.GetTerrain(base.Map);
			if (terrain.fertility == 0)
			{
				return;
			}

			if (!c.Roofed(base.Map))
			{
				List<Thing> thingList = c.GetThingList(base.Map);
				for (int i = 0; i < thingList.Count; i++)
				{
					Thing thing = thingList[i];
					if (thing is Plant)
					{
						if (Rand.Value < 0.0065f)
						{
							Plant plant = (Plant)ThingMaker.MakeThing(thing.def, null);
							if (plant.def.plant.LimitedLifespan && plant.def.statBases.GetStatOffsetFromList(StatDefOf.Beauty) > 3 && plant.def.ingestible.foodType != FoodTypeFlags.Tree)
							{
								plant.Growth = 1;
							}
						}
					}
					else if (thing.def.category == ThingCategory.Item)
					{
						canBloomHere = false;
					}
				}
			}
			else
			{
				return;
			}

			if (canBloomHere && Rand.Value < 0.065f * base.Map.fertilityGrid.FertilityAt(c) * this.howManyBlooms)
			{
				if (c.GetEdifice(base.Map) == null && c.GetCover(base.Map) == null)
				{
					IEnumerable<ThingDef> source = from def in bloomPlants
												   where def.CanEverPlantAt(c, base.Map)
												   select def;
					if (source.Any<ThingDef>())
					{
						ThingDef thingDef = source.RandomElement();
						int randomInRange = thingDef.plant.wildClusterSizeRange.RandomInRange;
						for (int j = 0; j < randomInRange; j++)
						{
							IntVec3 c2;
							if (j == 0)
							{
								c2 = c;
							}
							else if (!GenPlantReproduction.TryFindReproductionDestination(c, thingDef, SeedTargFindMode.MapGenCluster, base.Map, out c2))
							{
								break;
							}
							Plant plant = (Plant)ThingMaker.MakeThing(thingDef, null);
							plant.Growth = 1;
							GenSpawn.Spawn(plant, c2, base.Map);
						}
					}
				}
			}
		}
	}
}
