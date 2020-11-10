using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.Noise;
using TKKN_NPS.Workers;

namespace TKKN_NPS
{
	// COMP PROPERTIES
    public abstract class CompProperties_Springs : CompProperties
    {
        public int radius = 6;
        public int AOE = 15;
        public float temperature = 1f;
        public TerrainDef wetTile;
        public TerrainDef dryTile;
		public TerrainDef deepTile;
		public ThingDef spawnProp;
        public int weight;
        public string commonBiome;
        public int howOftenToChange;
		public bool canReproduce;
		public int borderSize;
		public int startingRadius;

	}

	public class CompProperties_SpringWater : CompProperties_Springs
    {
        public CompProperties_SpringWater()
        {
            compClass = typeof(SpringComp);
		}
	}

    public class CompProperties_SpringLava : CompProperties_Springs
	{
        public CompProperties_SpringLava()
        {

			compClass = typeof(LavaComp);
		}
    }

	//THING COMPS

	public abstract class SpringCompAbstract : ThingComp
	{

		public bool specialFX = false;
		public abstract void SpecialCellAffects(IntVec3 c);
		public abstract void SpringTerrain(IntVec3 c);
		public abstract bool DoBorder(IntVec3 c);

		public abstract void FillBorder();

		public virtual void SpecialFXAffect(IntVec3 c)
		{
			SpringTerrain(c);
			bool FX = specialFX;
			specialFX = false;
			if (!FX)
			{
				return;
			}
		}
	}

	public class SpringComp : SpringCompAbstract
	{
		public ModuleBase terrainNoise;

		public bool spawnThings = false;
		public string terrainType = "wet";

		public string biomeName;
		public int makeAnotherAt = 400;
		public int age = 0;
		public string status = "spawning";
		public float width = 0;

		public CompProperties_Springs Props => (CompProperties_Springs)props;

		private List<IntVec3> affectableCells = new List<IntVec3>();
		private List<IntVec3> boundaryCells = new List<IntVec3>();
		private List<IntVec3> boundaryCellsRough = new List<IntVec3>();
		private List<IntVec3> affectableCellsAtmosphere = new List<IntVec3>();

		public int GetID()
		{
			string numOnly = parent.ThingID.Replace(parent.def.defName, "");
			return Int32.Parse(numOnly);
		}
		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			makeAnotherAt = Props.howOftenToChange * 4000;
			if (respawningAfterLoad)
			{
				SpringData savedData = parent.Map.GetComponent<Watcher>().activeSprings[GetID()];
				if (savedData != null)
				{
					biomeName = savedData.biomeName;
					makeAnotherAt = savedData.makeAnotherAt;
					age = savedData.age;
					status = savedData.status;
					width = savedData.width;
				}
			}
			else
			{
				status = "spawning";
				biomeName = parent.Map.Biome.defName;
				width = Props.startingRadius;

				SpringData savedData = new SpringData
				{
					springID = parent.ThingID,
					biomeName = biomeName,
					makeAnotherAt = makeAnotherAt,
					age = age,
					status = status,
					width = width
				};

				parent.Map.GetComponent<Watcher>().activeSprings.Add(GetID(), savedData);
			}
			ChangeShape();
			CompTickRare();
			if (!respawningAfterLoad) {
				FillBorder();
			}
		}

		public override void CompTickRare()
		{
			base.CompTickRare();
			spawnThings = false;
			age += 250;

			if (Props.howOftenToChange > 0 && age > Props.howOftenToChange && age % Props.howOftenToChange == 0)
			{
				ChangeShape();
			}

			float radius = Props.radius;
			if (status == "spawning")
			{
				radius = width;
				width += .5f;
				if (radius == Props.radius)
				{
					status = "stable";
				}
			}

			float makeAnother = (age/6000)/1000;
			if (Props.canReproduce && Rand.Value + makeAnother > .01f)
			{
				//see if we're going to add another spring spawner.
				status = "expand";
				makeAnotherAt += Props.weight;
			}

			if (status != "despawn")
			{
				if (status != "stable")
				{
					SetCellsToAffect();
					foreach (IntVec3 cell in affectableCells)
					{
						terrainType = "wet";
						AffectCell(cell);
						SpecialFXAffect(cell);
					}
					foreach (IntVec3 cell in boundaryCellsRough) {						
						AffectCell(cell);
						SpecialFXAffect(cell);
					}
					foreach (IntVec3 cell in boundaryCells)
					{
						if (Rand.Value > .1)
						{
							terrainType = "dry";
							AffectCell(cell);
						}
					}
				}
				else
				{
					foreach (IntVec3 cell in affectableCells)
					{
						SpecialFXAffect(cell);
					}

				}
				foreach (IntVec3 cell in affectableCellsAtmosphere)
				{
					AtmosphereAffectCell(cell);
				}
			}

			if (Props.canReproduce && status == "despawn")
			{
				parent.Map.GetComponent<Watcher>().activeSprings.Remove(GetID());
				parent.Destroy();
				return;
			}
			CheckIfDespawn();
			SaveValues();
		}

		public void SetCellsToAffect()
		{
			if (status == "stable")
			{
				return;
			}
			IntVec3 pos = parent.Position;
			Map map = parent.Map;
			affectableCells.Clear();
			boundaryCellsRough.Clear();
			boundaryCells.Clear();
			affectableCellsAtmosphere.Clear();
			if (!pos.InBounds(map))
			{
				return;
			}
			int maxArea = (int) Math.Round(width + Props.borderSize + 5);

			Region region = pos.GetRegion(map, RegionType.Set_Passable);
			if (region == null)
			{
				return;
			}
			RegionTraverser.BreadthFirstTraverse(region, (Region from, Region r) => r.door == null, delegate (Region r)
			{
				foreach (IntVec3 current in r.Cells)
				{
					if (current.InHorDistOf(pos, width))
					{
						affectableCells.Add(current);
					}
					else if (current.InHorDistOf(pos, width + 2))
					{
						boundaryCellsRough.Add(current);
					}
					else if (current.InHorDistOf(pos, width + Props.borderSize + 1))
					{
						boundaryCells.Add(current);
					}
					else if (current.InHorDistOf(pos, width + Props.borderSize + 5))
					{
						affectableCellsAtmosphere.Add(current);
					}
				}
				return false;
			}, maxArea, RegionType.Set_Passable);
			return;
		}

		public void SaveValues()
		{
			SpringData savedData = parent.Map.GetComponent<Watcher>().activeSprings[GetID()];
			if (savedData != null)
			{
				savedData.biomeName = biomeName;
				savedData.makeAnotherAt = makeAnotherAt;
				savedData.age = age;
				savedData.status = status;
				savedData.width = width;
			}
		}

		public void ChangeShape()
		{
			if (Props.howOftenToChange == 0)
			{
				return;
			}
			ModuleBase moduleBase = new Perlin(1.1, 1, 5, 3, Props.radius, QualityMode.Medium);
			moduleBase = new ScaleBias(0.2, 0.2, moduleBase);

			ModuleBase moduleBase2 = new DistFromAxis(2);
			moduleBase2 = new ScaleBias(.2, .2, moduleBase2);
			moduleBase2 = new Clamp(0, 1, moduleBase2);

			terrainNoise = new Add(moduleBase, moduleBase2);

		}

		public override void SpringTerrain(IntVec3 loc)
		{
			if (terrainNoise == null)
			{
				terrainType = "dry";
				return;
			}
			float value = terrainNoise.GetValue(loc);
			value = value / Props.radius;
			int dif = (int) Math.Floor(value);
			value = value - dif;


			if (value < .1)
			{
				specialFX = true;
			}
			if (value < .8f)
			{
				terrainType = "wet";
				return;
			}

			if (value < .85f)
			{
				spawnThings = true;
			}

			terrainType = "dry";
		}

		public void CheckIfDespawn()
		{
			if (biomeName == Props.commonBiome)
			{
				if (Rand.Value < .0001f)
				{
					status = "despawn";
				}
			}
			else
			{
				if (Rand.Value < .001f)
				{
					status = "despawn";
				}
			}
		}

		public override void FillBorder()
		{
			Map map = parent.Map;
			List<ThingDef> list = map.Biome.AllWildPlants.ToList<ThingDef>();
			list.Add(Props.spawnProp);
			float num = map.Biome.plantDensity;
			foreach (IntVec3 c in boundaryCellsRough.InRandomOrder(null))
			{
				GenPlants(c, map, list);
			}
			foreach (IntVec3 c in boundaryCells.InRandomOrder(null))
			{
				GenPlants(c, map, list);
			}
		}

		private void GenPlants(IntVec3 c, Map map, List<ThingDef> list)
		{
			if (c.GetEdifice(map) == null && c.GetCover(map) == null)
			{
				IEnumerable<ThingDef> source = from def in list
											   where def.CanEverPlantAt(c, map)
											   select def;

				SpawnWorker.SpawnSpecialPlants(c, map);
				SpawnWorker.SpawnPlant(source, c, map);
			}
		}



		public void AffectCell(IntVec3 c)
		{
			bool isSpawnCell = false;
			if (!c.InBounds(parent.Map))
			{
				return;
			}
			if (terrainType == "")
			{
				SpringTerrain(c);
			}

			if (status != "despawn")
			{
				if (c == parent.Position)
				{
					isSpawnCell = true;
					terrainType = "wet";
				}
				//double check we're not adding a border into another instance.
				if (terrainType == "dry")
				{
					
					if (!DoBorder(c))
					{
						terrainType = "wet";
					}
				}

			}

			if (terrainType == "dry")
			{
				int num = c.GetThingList(parent.Map).Count;
				//spawn whatever special items surround this thing.
				parent.Map.terrainGrid.SetTerrain(c, Props.dryTile);

				if (num == 0 && spawnThings == true && Props.spawnProp != null)
				{
					ThingDef def = Props.spawnProp;
					GenSpawn.Spawn(def, c, parent.Map);
				}
				spawnThings = false;
			}
			else
			{
				SpecialCellAffects(c);
			}
			if (status == "expand")
			{
				if (!isSpawnCell)
				{
					ThingWithComps anotherSpring = (ThingWithComps)GenSpawn.Spawn(ThingMaker.MakeThing(parent.def, null), c, parent.Map);
				}
				status = "stable";
			}

			if (parent.Map.GetComponent<Watcher>().cellWeatherAffects.ContainsKey(c))
			{
				parent.Map.GetComponent<Watcher>().cellWeatherAffects[c].baseTerrain = c.GetTerrain(parent.Map);
			}

			terrainType = "";
		}

		public override bool DoBorder(IntVec3 c)
		{
			TerrainDef currentTerrain = c.GetTerrain(parent.Map);
			if (currentTerrain == Props.wetTile) {
				return false;
			}
			if (currentTerrain.HasTag("TKKN_Wet"))
			{
				return false;
			}
			return true;
		}

		protected void AtmosphereAffectCell(IntVec3 c)
		{
			GenTemperature.PushHeat(parent, Props.temperature);			
		}

		public void AffectPawns(IntVec3 c)
		{
			List<Thing> pawns = c.GetThingList(parent.Map);
			for (int i = 0; i < pawns.Count(); i++)
			{

				if (pawns[i] is Pawn pawn)
				{
					//    pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.NearNPS, null);
				}
			}
		}

		public override void SpecialCellAffects(IntVec3 c)
		{
			//set terrain
			if (terrainType == "wet")
			{
				parent.Map.terrainGrid.SetTerrain(c, Props.wetTile);
			}
			else if (terrainType == "deep")
			{
				parent.Map.terrainGrid.SetTerrain(c, Props.deepTile);
			}
			FilthMaker.RemoveAllFilth(c, parent.Map);
		}

		public override void SpecialFXAffect(IntVec3 c)
		{
			base.SpecialFXAffect(c);
		}


	}

	public class LavaComp : SpringComp
	{
		public override void SpecialCellAffects(IntVec3 c)
		{
			base.SpecialCellAffects(c);
			if (terrainType == "wet")
			{
				parent.Map.GetComponent<Watcher>().lavaCellsList.Add(c);
			}
			else
			{
				parent.Map.GetComponent<Watcher>().lavaCellsList.Remove(c);

			}
		}

		public override void FillBorder()
		{
		}

		public new void ChangeShape()
		{
			ModuleBase moduleBase = new Perlin(1.1, 2, 0.5, 2, Rand.Range(0, Props.radius), QualityMode.Medium);
			moduleBase = new ScaleBias(0.2, 0.2, moduleBase);

			ModuleBase moduleBase2 = new DistFromAxis(new FloatRange(0, Props.radius).RandomInRange);
			moduleBase2 = new ScaleBias(.2, .2, moduleBase2);
			moduleBase2 = new Clamp(0, 1, moduleBase2);

			terrainNoise = new Add(moduleBase, moduleBase2);

		}

		public override bool DoBorder(IntVec3 c)
		{
			TerrainDef currentTerrain = c.GetTerrain(parent.Map);
			if (currentTerrain == Props.wetTile)
			{
				return false;
			}

			return true;
		}
	}

	public class Spring : ThingWithComps
	{
		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
		}
	
	}

	public class Lava_Spring : ThingWithComps
	{
		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			Map map2 = Map;
			BiomeDef biome = map.Biome;
		}

	}
}






