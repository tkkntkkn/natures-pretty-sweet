using Harmony;
using System.Reflection;
using Verse;
using RimWorld;

namespace TKKN_NPS
{
	[StaticConstructorOnStartup]
	class HarmonyMain
	{
		
		static HarmonyMain()
		{
			var harmony = HarmonyInstance.Create("com.github.tkkntkkn.Natures-Pretty-Sweet");
			harmony.PatchAll(Assembly.GetExecutingAssembly());

		}
	}
}
