using UnityEngine;
using RimWorld;
using Verse;


namespace TKKN_NPS
{
	[StaticConstructorOnStartup]
	public class WeatherOverlay_ThickFog : SkyOverlay
	{
		public static readonly Material FogOverlayWorld = new Material(MatLoader.LoadMat("Weather/FogOverlayWorld"));

		public WeatherOverlay_ThickFog()
        {
			
			worldOverlayMat = WeatherOverlay_ThickFog.FogOverlayWorld;

			worldOverlayPanSpeed1 = 0.0003f;
            worldOverlayPanSpeed2 = 0.0001f;
            worldPanDir1 = new Vector2(1f, 1f);
            worldPanDir2 = new Vector2(1f, -1f);

        }
    }
	
	[StaticConstructorOnStartup]
	public class WeatherOverlay_DustStorm : SkyOverlay
	{
		public static readonly Material material = new Material(MatLoader.LoadMat("Weather/FogOverlayWorld"));

		public WeatherOverlay_DustStorm()
		{

			worldOverlayMat = WeatherOverlay_DustStorm.material;
			OverlayColor = new Color(0.57f, 0.34f, 0.10f);

			worldOverlayPanSpeed1 = 0.0003f;
			worldOverlayPanSpeed2 = 0.0001f;
			worldPanDir1 = new Vector2(1f, 1f);
			worldPanDir2 = new Vector2(1f, -1f);

		}
	}

	[StaticConstructorOnStartup]
	public class WeatherOverlay_DustStormHeavy : SkyOverlay
	{
		private static readonly Material material;

		static WeatherOverlay_DustStormHeavy()
		{
			WeatherOverlay_DustStormHeavy.material = new Material(MatLoader.LoadMat("Weather/SnowOverlayWorld"));
		}

		public WeatherOverlay_DustStormHeavy()
		{
			worldOverlayMat = WeatherOverlay_DustStormHeavy.material;

			OverlayColor = new Color(0.57f, 0.34f, 0.10f);

			worldOverlayPanSpeed1 = 0.018f;
			worldPanDir1 = new Vector2(-1f, -0.26f);
			worldPanDir1.Normalize();

			worldOverlayPanSpeed2 = 0.022f;
			worldPanDir2 = new Vector2(-1f, -0.24f);
			worldPanDir2.Normalize();
		}
	}


	[StaticConstructorOnStartup]
	public class WeatherOverlay_LavaSmoke : SkyOverlay
	{
		public static Material material = new Material(MatLoader.LoadMat("Weather/FogOverlayWorld"));

		public WeatherOverlay_LavaSmoke()
		{
			worldOverlayMat = WeatherOverlay_LavaSmoke.material;
			OverlayColor = new Color(0.64f, 0.35f, 0.26f);

			worldOverlayPanSpeed1 = 0.0003f; 
			worldOverlayPanSpeed2 = 0.0001f;
			worldPanDir1 = new Vector2(1f, 1f);
			worldPanDir2 = new Vector2(1f, -1f);
		}
	}
	
}
