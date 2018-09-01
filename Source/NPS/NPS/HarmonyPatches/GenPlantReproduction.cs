using System.Collections.Generic;
using RimWorld;
using Verse;
using Harmony;
using System.Reflection;
using System.Reflection.Emit;
namespace TKKN_NPS
{

	[HarmonyPatch(typeof(WildPlantSpawner))]
	[HarmonyPatch("CheckSpawnWildPlantAt")]
	public static class WildPlantSpawner_CheckSpawnWildPlantAt
	{
		static bool CheckSpawnWildPlantAt(IntVec3 c, float plantDensity, float wholeMapNumDesiredPlants, bool setRandomGrowth = false)
		{
			if (!(plantDensity <= 0.0) && c.GetPlant(this.map) == null && c.GetCover(this.map) == null && c.GetEdifice(this.map) == null && !(this.map.fertilityGrid.FertilityAt(c) <= 0.0) && PlantUtility.SnowAllowsPlanting(c, this.map))
			{
			
			}
		}
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			// do something
			int startIndex = -1, callvirt = 0;

			var codes = new List<CodeInstruction>(instructions);
			for (int i = 0; i < codes.Count; i++)
			{//callvirt
				if (codes[i].opcode == OpCodes.Callvirt)
				{
					callvirt++;
					if (callvirt >= 24)
					{
						//this place the index directly on IL_004a: callvirt instance float32 RimWorld.FertilityGrid::FertilityAt(valuetype Verse.IntVec3)
						startIndex = i + 1;
						Log.Message("24th Callvirt found");
						break;
					}

				}
			}


			if (startIndex > -1)
			{

				Log.Message("Value call at " + codes[startIndex + 2]);
				Log.Message("Found new object call at " + codes[startIndex + 3]);

				instruction = codes[i];
				instruction.operand = typeof(WildPlantSpawner_CheckSpawnWildPlantAt).GetMethod(
						nameof(WildPlantSpawner_CheckSpawnWildPlantAt.CheckSpawnWildPlantAt), BindingFlags.Static | BindingFlags.NonPublic);


				//Create a constructor for IL code
				var constructorInfo = typeof(CustomNameChange).GetConstructor(
					  new[] { typeof(Pawn) });

				//Replace index 3 IL_041c: newobj instance void Verse.Dialog_ChangeNameTriple::.ctor(class Verse.Pawn) to call our new class

				codes[startIndex + 3] = new CodeInstruction(OpCodes.Newobj, constructorInfo);           //codes[startIndex + 3].opcode = OpCodes.Nop;
				Log.Message("Found new object call at " + codes[startIndex + 3]);
			}

			return codes;
			//return null;
		}
	}





	[HarmonyPatch(typeof(WildPlantSpawner))]
	[HarmonyPatch("CheckSpawnWildPlantAt")]
	class PatchCanEverPlantAt
	{

		[HarmonyPostfix]
		public static void Postfix(WildPlantSpawner __instance, IntVec3 c, float plantDensity, float wholeMapNumDesiredPlants, bool setRandomGrowth, ref bool __result)
		{
			if (__result == false) {
				Map map = Traverse.Create(__instance).Field("map").GetValue<Map>();

				if (!(plantDensity <= 0.0) && c.GetPlant(map) == null && c.GetCover(map) == null && c.GetEdifice(map) == null && PlantUtility.SnowAllowsPlanting(c, map))
				{

					//check if we're on water or salt.
					TerrainDef terrain = c.GetTerrain(map);
					BiomeSeasonalSettings biomeSet = map.Biome.GetModExtension<BiomeSeasonalSettings>();
					if (terrain.HasTag("TKKN_SpecialPlants") && biomeSet.specialPlants != null)
					{
						//spawn any special plants
						WildPlantSpawner.tmpPossiblePlants


					this.CalculatePlantsWhichCanGrowAt(c, WildPlantSpawner.tmpPossiblePlants, cavePlants, plantDensity);
						if (!WildPlantSpawner.tmpPossiblePlants.Any())
						{
							return false;
						}
						this.CalculateDistancesToNearbyClusters(c);
						WildPlantSpawner.tmpPossiblePlantsWithWeight.Clear();
						for (int i = 0; i < WildPlantSpawner.tmpPossiblePlants.Count; i++)
						{
							float value = this.PlantChoiceWeight(WildPlantSpawner.tmpPossiblePlants[i], c, WildPlantSpawner.distanceSqToNearbyClusters, wholeMapNumDesiredPlants, plantDensity);
							WildPlantSpawner.tmpPossiblePlantsWithWeight.Add(new KeyValuePair<ThingDef, float>(WildPlantSpawner.tmpPossiblePlants[i], value));
						}
						KeyValuePair<ThingDef, float> keyValuePair = default(KeyValuePair<ThingDef, float>);
						if (!((IEnumerable<KeyValuePair<ThingDef, float>>)WildPlantSpawner.tmpPossiblePlantsWithWeight).TryRandomElementByWeight<KeyValuePair<ThingDef, float>>((Func<KeyValuePair<ThingDef, float>, float>)((KeyValuePair<ThingDef, float> x) => x.Value), out keyValuePair))
						{
							return false;
						}
						Plant plant = (Plant)ThingMaker.MakeThing(keyValuePair.Key, null);
						if (setRandomGrowth)
						{
							plant.Growth = Rand.Range(0.07f, 1f);
							if (plant.def.plant.LimitedLifespan)
							{
								plant.Age = Rand.Range(0, Mathf.Max(plant.def.plant.LifespanTicks - 50, 0));
							}
						}
						GenSpawn.Spawn(plant, c, map, WipeMode.Vanish);
						return true;
					}

					PlantDef = c.GetPlant(map);
					if (__result == true && plantDef != null && c != null && map != null)
					{
						BiomeDef biome = map.Biome;
						BiomeSeasonalSettings biomeSettings = biome.GetModExtension<BiomeSeasonalSettings>();
						if (biomeSettings != null)
						{
							if (!biomeSettings.canPutOnTerrain(c, plantDef, map))
							{
								__result = false;
							}
						}
					}
				}

			}
		}

		private void CalculatePlantsWhichCanGrowAt(IntVec3 c, List<ThingDef> outPlants, Map map, float plantDensity)
		{
			outPlants.Clear();
			List<ThingDef> allWildPlants = map.Biome.AllWildPlants;
			for (int j = 0; j < allWildPlants.Count; j++)
			{
				ThingDef thingDef = allWildPlants[j];
				if (thingDef.CanEverPlantAt(c, map))
				{
					if (thingDef.plant.wildOrder == map.Biome.LowestWildAndCavePlantOrder)
					{
						goto IL_00f8;
					}
					float num = 7f;
					if (thingDef.plant.GrowsInClusters)
					{
						num = Math.Max(num, (float)((float)thingDef.plant.wildClusterRadius * 1.5));
					}
					if (this.EnoughLowerOrderPlantsNearby(c, plantDensity, num, thingDef))
						goto IL_00f8;
				}
				continue;
				IL_00f8:
				outPlants.Add(thingDef);
			}
		}

	}
}