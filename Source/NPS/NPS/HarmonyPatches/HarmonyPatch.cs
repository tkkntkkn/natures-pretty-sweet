using Harmony;
using System.Reflection;
using Verse;
using RimWorld;

namespace TKKN_NPS
{
	[StaticConstructorOnStartup]
	class HarmonyMain
	{
		public static FieldInfo MapFieldInfo;
		public static FieldInfo MapFieldInfoPlant;
		
		static HarmonyMain()
		{
			var harmony = HarmonyInstance.Create("com.github.tkkntkkn.Natures-Pretty-Sweet");
			harmony.PatchAll(Assembly.GetExecutingAssembly());

			MapFieldInfo = typeof(SteadyAtmosphereEffects).GetField("map", BindingFlags.NonPublic | BindingFlags.Instance);
			MapFieldInfoPlant = typeof(Plant).GetField("map", BindingFlags.NonPublic | BindingFlags.Instance);
		}
	}
}
