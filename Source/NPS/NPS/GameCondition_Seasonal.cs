using System.Linq;
using System.Collections.Generic;
using Verse;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;

namespace TKKN_NPS
{
	public class GameCondition_Drought : GameCondition
	{
		public int tempAdjust = 10;
		public string floodOverride = "low";
	}
}
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
		public float howManyBlooms;
		public void DoCellSteadyEffects(IntVec3 c)
		{
			//must be outdoors.
			Map map = base.SingleMap;
			BiomeSeasonalSettings biomeSettings = base.SingleMap.Biome.GetModExtension<BiomeSeasonalSettings>();
			List<ThingDef> bloomPlants = biomeSettings.bloomPlants.ToList<ThingDef>();
			if (bloomPlants.Count == 0)
			{
				return;
			}

			Room room = c.GetRoom(base.SingleMap, RegionType.Set_All);
			if (room != null)
			{
				return;
			}


			TerrainDef terrain = c.GetTerrain(base.SingleMap);
			if (terrain.fertility == 0)
			{
				return;
			}
			
			if (!c.Roofed(base.SingleMap) && c.GetEdifice(base.SingleMap) == null && c.GetCover(base.SingleMap) == null)
			{
				List<Thing> thingList = c.GetThingList(base.SingleMap);
				bool planted = false;
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
							planted = true;
						}
					}
				}
				if (planted)
				{
					return;
				}
			}
			else
			{
				return;
			}

			if (Rand.Value < 0.65f * base.SingleMap.fertilityGrid.FertilityAt(c) * this.howManyBlooms)
			{
				IEnumerable<ThingDef> source = from def in bloomPlants
												where def.CanEverPlantAt(c, base.SingleMap)
												select def;
				if (source.Any<ThingDef>())
				{
					ThingDef thingDef = source.RandomElement();
					Plant plant = (Plant)ThingMaker.MakeThing(thingDef, null);
					plant.Growth = 1;
					GenSpawn.Spawn(plant, c, base.SingleMap);
				}
			}
		}
	}
}
