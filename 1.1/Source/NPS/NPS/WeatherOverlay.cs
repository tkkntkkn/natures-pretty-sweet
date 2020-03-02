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
			
			this.worldOverlayMat = WeatherOverlay_ThickFog.FogOverlayWorld;

			this.worldOverlayPanSpeed1 = 0.0003f;
            this.worldOverlayPanSpeed2 = 0.0001f;
            this.worldPanDir1 = new Vector2(1f, 1f);
            this.worldPanDir2 = new Vector2(1f, -1f);

        }
    }
	
	[StaticConstructorOnStartup]
	public class WeatherOverlay_DustStorm : SkyOverlay
	{
		public static readonly Material material = new Material(MatLoader.LoadMat("Weather/FogOverlayWorld"));

		public WeatherOverlay_DustStorm()
		{

			this.worldOverlayMat = WeatherOverlay_DustStorm.material;
			this.OverlayColor = new Color(0.57f, 0.34f, 0.10f);

			this.worldOverlayPanSpeed1 = 0.0003f;
			this.worldOverlayPanSpeed2 = 0.0001f;
			this.worldPanDir1 = new Vector2(1f, 1f);
			this.worldPanDir2 = new Vector2(1f, -1f);

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
			this.worldOverlayMat = WeatherOverlay_DustStormHeavy.material;

			this.OverlayColor = new Color(0.57f, 0.34f, 0.10f);

			this.worldOverlayPanSpeed1 = 0.018f;
			this.worldPanDir1 = new Vector2(-1f, -0.26f);
			this.worldPanDir1.Normalize();

			this.worldOverlayPanSpeed2 = 0.022f;
			this.worldPanDir2 = new Vector2(-1f, -0.24f);
			this.worldPanDir2.Normalize();
		}
	}


	[StaticConstructorOnStartup]
	public class WeatherOverlay_LavaSmoke : SkyOverlay
	{
		public static Material material = new Material(MatLoader.LoadMat("Weather/FogOverlayWorld"));

		public WeatherOverlay_LavaSmoke()
		{
			this.worldOverlayMat = WeatherOverlay_LavaSmoke.material;
			this.OverlayColor = new Color(0.64f, 0.35f, 0.26f);

			this.worldOverlayPanSpeed1 = 0.0003f; 
			this.worldOverlayPanSpeed2 = 0.0001f;
			this.worldPanDir1 = new Vector2(1f, 1f);
			this.worldPanDir2 = new Vector2(1f, -1f);
		}
	}
	
}
