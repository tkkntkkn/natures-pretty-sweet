using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.Noise;

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
            this.compClass = typeof(SpringComp);
		}
	}

    public class CompProperties_SpringLava : CompProperties_Springs
	{
        public CompProperties_SpringLava()
        {

			this.compClass = typeof(LavaComp);
		}
    }

	//THING COMPS

	public abstract class SpringCompAbstract : ThingComp
	{

		public bool specialFX = false;
		public abstract void specialCellAffects(IntVec3 c);
		public abstract void springTerrain(IntVec3 c);
		public abstract bool doBorder(IntVec3 c);

		public abstract void fillBorder();

		public virtual void specialFXAffect(IntVec3 c)
		{
			this.springTerrain(c);
			bool FX = this.specialFX;
			this.specialFX = false;
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
		public bool specialFX = false;

		public string biomeName;
		public int makeAnotherAt = 400;
		public int age = 0;
		public string status = "spawning";
		public float width = 0;

		public CompProperties_Springs Props
		{
			get
			{
				return (CompProperties_Springs)this.props;
			}
		}

		private List<IntVec3> affectableCells = new List<IntVec3>();
		private List<IntVec3> boundaryCells = new List<IntVec3>();
		private List<IntVec3> boundaryCellsRough = new List<IntVec3>();
		private List<IntVec3> affectableCellsAtmosphere = new List<IntVec3>();

		public int getID()
		{
			string numOnly = this.parent.ThingID.Replace(this.parent.def.defName, "");
			return Int32.Parse(numOnly);
		}
		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			this.makeAnotherAt = this.Props.howOftenToChange * 4000;
			if (respawningAfterLoad)
			{
				springData savedData = this.parent.Map.GetComponent<Watcher>().activeSprings[this.getID()];
				if (savedData != null)
				{
					this.biomeName = savedData.biomeName;
					this.makeAnotherAt = savedData.makeAnotherAt;
					this.age = savedData.age;
					this.status = savedData.status;
					this.width = savedData.width;
				}
			}
			else
			{
				this.status = "spawning";
				this.biomeName = this.parent.Map.Biome.defName;
				this.width = this.Props.startingRadius;

				springData savedData = new springData();
				savedData.springID = this.parent.ThingID;
				savedData.biomeName = this.biomeName;
				savedData.makeAnotherAt = this.makeAnotherAt;
				savedData.age = this.age;
				savedData.status = this.status;
				savedData.width = this.width;

				this.parent.Map.GetComponent<Watcher>().activeSprings.Add(this.getID(), savedData);
			}
			this.changeShape();
			this.CompTickRare();
			if (!respawningAfterLoad) {
				this.fillBorder();
			}
		}

		public override void CompTickRare()
		{
			base.CompTickRare();
			this.spawnThings = false;
			this.age += 250;

			if (this.Props.howOftenToChange > 0 && this.age > this.Props.howOftenToChange && this.age % this.Props.howOftenToChange == 0)
			{
				this.changeShape();
			}

			float radius = this.Props.radius;
			if (this.status == "spawning")
			{
				radius = this.width;
				this.width += .5f;
				if (radius == this.Props.radius)
				{
					this.status = "stable";
				}
			}

			float makeAnother = (this.age/6000)/1000;
			if (this.Props.canReproduce && Rand.Value + makeAnother > .01f)
			{
				//see if we're going to add another spring spawner.
				this.status = "expand";
				this.makeAnotherAt += this.Props.weight;
			}

			if (this.status != "despawn")
			{
				if (this.status != "stable")
				{
					this.setCellsToAffect();
					foreach (IntVec3 cell in this.affectableCells)
					{
						this.terrainType = "wet";
						this.AffectCell(cell);
						this.specialFXAffect(cell);
					}
					foreach (IntVec3 cell in this.boundaryCellsRough) {						
						this.AffectCell(cell);
						this.specialFXAffect(cell);
					}
					foreach (IntVec3 cell in this.boundaryCells)
					{
						if (Rand.Value > .1)
						{
							this.terrainType = "dry";
							this.AffectCell(cell);
						}
					}
				}
				else
				{
					foreach (IntVec3 cell in this.affectableCells)
					{
						this.specialFXAffect(cell);
					}

				}
				foreach (IntVec3 cell in this.affectableCellsAtmosphere)
				{
					this.atmosphereAffectCell(cell);
				}
			}

			if (this.Props.canReproduce && this.status == "despawn")
			{
				this.parent.Map.GetComponent<Watcher>().activeSprings.Remove(this.getID());
				this.parent.Destroy();
				return;
			}
			this.checkIfDespawn();
			this.saveValues();
		}

		public void setCellsToAffect()
		{
			if (this.status == "stable")
			{
				return;
			}
			IntVec3 pos = this.parent.Position;
			Map map = this.parent.Map;
			this.affectableCells.Clear();
			this.boundaryCellsRough.Clear();
			this.boundaryCells.Clear();
			this.affectableCellsAtmosphere.Clear();
			if (!pos.InBounds(map))
			{
				return;
			}
			int maxArea = (int) Math.Round(this.width + this.Props.borderSize + 5);

			Region region = pos.GetRegion(map, RegionType.Set_Passable);
			if (region == null)
			{
				return;
			}
			RegionTraverser.BreadthFirstTraverse(region, (Region from, Region r) => r.door == null, delegate (Region r)
			{
				foreach (IntVec3 current in r.Cells)
				{
					if (current.InHorDistOf(pos, this.width))
					{
						this.affectableCells.Add(current);
					}
					else if (current.InHorDistOf(pos, this.width + 2))
					{
						this.boundaryCellsRough.Add(current);
					}
					else if (current.InHorDistOf(pos, this.width + this.Props.borderSize + 1))
					{
						this.boundaryCells.Add(current);
					}
					else if (current.InHorDistOf(pos, this.width + this.Props.borderSize + 5))
					{
						this.affectableCellsAtmosphere.Add(current);
					}
				}
				return false;
			}, maxArea, RegionType.Set_Passable);
			return;
		}

		public void saveValues()
		{
			springData savedData = this.parent.Map.GetComponent<Watcher>().activeSprings[this.getID()];
			if (savedData != null)
			{
				savedData.biomeName = this.biomeName;
				savedData.makeAnotherAt = this.makeAnotherAt;
				savedData.age = this.age;
				savedData.status = this.status;
				savedData.width = this.width;
			}
		}

		public void changeShape()
		{
			if (this.Props.howOftenToChange == 0)
			{
				return;
			}
			ModuleBase moduleBase = new Perlin(1.1, 1, 5, 3, this.Props.radius, QualityMode.Medium);
			moduleBase = new ScaleBias(0.2, 0.2, moduleBase);

			ModuleBase moduleBase2 = new DistFromAxis(2);
			moduleBase2 = new ScaleBias(.2, .2, moduleBase2);
			moduleBase2 = new Clamp(0, 1, moduleBase2);

			this.terrainNoise = new Add(moduleBase, moduleBase2);

		}

		public override void springTerrain(IntVec3 loc)
		{
			if (this.terrainNoise == null)
			{
				this.terrainType = "dry";
				return;
			}
			float value = this.terrainNoise.GetValue(loc);
			value = value / this.Props.radius;
			int dif = (int) Math.Floor(value);
			value = value - dif;


			if (value < .1)
			{
				this.specialFX = true;
			}
			if (value < .8f)
			{
				this.terrainType = "wet";
				return;
			}

			if (value < .85f)
			{
				this.spawnThings = true;
			}

			this.terrainType = "dry";
		}

		public void checkIfDespawn()
		{
			if (this.biomeName == this.Props.commonBiome)
			{
				if (Rand.Value < .0001f)
				{
					this.status = "despawn";
				}
			}
			else
			{
				if (Rand.Value < .001f)
				{
					this.status = "despawn";
				}
			}
		}

		public override void fillBorder()
		{
			Map map = this.parent.Map;
			List<ThingDef> list = map.Biome.AllWildPlants.ToList<ThingDef>();
			list.Add(this.Props.spawnProp);
			float num = map.Biome.plantDensity;
			foreach (IntVec3 c in this.boundaryCellsRough.InRandomOrder(null))
			{
				this.genPlants(c, map, list);
			}
			foreach (IntVec3 c in this.boundaryCells.InRandomOrder(null))
			{
				this.genPlants(c, map, list);
			}
		}

		private void genPlants(IntVec3 c, Map map, List<ThingDef> list)
		{
			if (c.GetEdifice(map) == null && c.GetCover(map) == null)
			{
				IEnumerable<ThingDef> source = from def in list
						where def.CanEverPlantAt(c, map)
						select def;

				if (source.Any<ThingDef>())
				{

					ThingDef thingDef = source.RandomElementByWeight((ThingDef x) => this.PlantChoiceWeight(x, map));
					Plant plant = (Plant)ThingMaker.MakeThing(thingDef, null);
					plant.Growth = Rand.Range(0.07f, 1f);
					if (plant.def.plant.LimitedLifespan)
					{
						plant.Age = Rand.Range(0, Mathf.Max(plant.def.plant.LifespanTicks - 50, 0));
					}
					GenSpawn.Spawn(plant, c, map);

				}
			}
		}

		private float PlantChoiceWeight(ThingDef def, Map map)
		{
			float num = map.Biome.CommonalityOfPlant(def);
			return num * def.plant.wildClusterWeight;
		}

		public void AffectCell(IntVec3 c)
		{
			bool isSpawnCell = false;
			if (!c.InBounds(this.parent.Map))
			{
				return;
			}
			if (this.terrainType == "")
			{
				this.springTerrain(c);
			}

			if (this.status != "despawn")
			{
				if (c == this.parent.Position)
				{
					isSpawnCell = true;
					this.terrainType = "wet";
				}
				//double check we're not adding a border into another instance.
				if (this.terrainType == "dry")
				{
					
					if (!this.doBorder(c))
					{
						this.terrainType = "wet";
					}
				}

			}

			if (this.terrainType == "dry")
			{
				int num = c.GetThingList(this.parent.Map).Count;
				//spawn whatever special items surround this thing.
				this.parent.Map.terrainGrid.SetTerrain(c, this.Props.dryTile);

				if (num == 0 && this.spawnThings == true && this.Props.spawnProp != null)
				{
					ThingDef def = this.Props.spawnProp;
					GenSpawn.Spawn(def, c, this.parent.Map);
				}
				this.spawnThings = false;
			}
			else
			{
				this.specialCellAffects(c);
			}
			if (this.status == "expand")
			{
				if (!isSpawnCell)
				{
					ThingWithComps anotherSpring = (ThingWithComps)GenSpawn.Spawn(ThingMaker.MakeThing(this.parent.def, null), c, this.parent.Map);
				}
				this.status = "stable";
			}

			if (this.parent.Map.GetComponent<Watcher>().cellWeatherAffects.ContainsKey(c))
			{
				this.parent.Map.GetComponent<Watcher>().cellWeatherAffects[c].baseTerrain = c.GetTerrain(this.parent.Map);
			}

			this.terrainType = "";
		}

		public override bool doBorder(IntVec3 c)
		{
			TerrainDef currentTerrain = c.GetTerrain(this.parent.Map);
			if (currentTerrain == this.Props.wetTile) {
				return false;
			}
			if (currentTerrain.HasTag("TKKN_Wet"))
			{
				return false;
			}
			return true;
		}

		protected void atmosphereAffectCell(IntVec3 c)
		{
			GenTemperature.PushHeat(this.parent, this.Props.temperature);			
		}

		public void AffectPawns(IntVec3 c)
		{
			List<Thing> pawns = c.GetThingList(this.parent.Map);
			for (int i = 0; i < pawns.Count(); i++)
			{

				Pawn pawn = pawns[i] as Pawn;
				if (pawn != null)
				{
					//    pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.NearNPS, null);
				}
			}
		}

		public override void specialCellAffects(IntVec3 c)
		{
			//set terrain
			if (this.terrainType == "wet")
			{
				this.parent.Map.terrainGrid.SetTerrain(c, this.Props.wetTile);
			}
			else if (this.terrainType == "deep")
			{
				this.parent.Map.terrainGrid.SetTerrain(c, this.Props.deepTile);
			}
			FilthMaker.RemoveAllFilth(c, this.parent.Map);
		}

		public override void specialFXAffect(IntVec3 c)
		{
			base.specialFXAffect(c);
		}


	}

	public class LavaComp : SpringComp
	{
		public override void specialCellAffects(IntVec3 c)
		{
			base.specialCellAffects(c);
			if (this.terrainType == "wet")
			{
				this.parent.Map.GetComponent<Watcher>().lavaCellsList.Add(c);
			}
			else
			{
				this.parent.Map.GetComponent<Watcher>().lavaCellsList.Remove(c);

			}
		}

		public override void fillBorder()
		{
		}

		public new void changeShape()
		{
			ModuleBase moduleBase = new Perlin(1.1, 2, 0.5, 2, Rand.Range(0, this.Props.radius), QualityMode.Medium);
			moduleBase = new ScaleBias(0.2, 0.2, moduleBase);

			ModuleBase moduleBase2 = new DistFromAxis(new FloatRange(0, this.Props.radius).RandomInRange);
			moduleBase2 = new ScaleBias(.2, .2, moduleBase2);
			moduleBase2 = new Clamp(0, 1, moduleBase2);

			this.terrainNoise = new Add(moduleBase, moduleBase2);

		}

		public override bool doBorder(IntVec3 c)
		{
			TerrainDef currentTerrain = c.GetTerrain(this.parent.Map);
			if (currentTerrain == this.Props.wetTile)
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
			Map map2 = this.Map;
			BiomeDef biome = map.Biome;
		}

	}
}






