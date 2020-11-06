using Verse;
using System.Collections.Generic;

namespace TKKN_NPS
{
	class ThingWeatherReaction : DefModExtension
	{
		public string frostGraphicPath = "";
		public string snowGraphicPath = "";
		public string frostLeaflessGraphicPath = "";
		public string snowLeaflessGraphicPath = "";
		public string floweringGraphicPath = "";
		public string droughtGraphicPath = "";
		public Graphic frostGraphicData = null;

		public List<TerrainDef> allowedTerrains = null;
		public List<TerrainDef> forbiddenTerrains = null;

	}
}
