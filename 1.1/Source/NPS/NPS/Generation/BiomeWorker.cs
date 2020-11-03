using RimWorld;
using RimWorld.Planet;
using Verse;


namespace TKKN_NPS
{    
    class NPS_BiomeWorker_Desert : BiomeWorker_Desert
    {
        public override float GetScore(Tile tile, int id)
        {
			if (base.GetScore(tile, id) > 0)
			{
				if (Rand.Value > .006)
				{
					return 0f;
				}

				return (float)(tile.temperature + 15);

			}
			return 0f;
		}
    }
    class NPS_BiomeWorker_Oasis : BiomeWorker_Desert
    {
        public override float GetScore(Tile tile, int id)
        {
			if (base.GetScore(tile, id) > 0)
			{
				if (Rand.Value > .006)
				{
					return 0f;
				}

				return (float)(tile.temperature + 20);

			}
			return 0f;
		}
	}
    class NPS_BiomeWorker_Redwoods : BiomeWorker_BorealForest
    {
        public override float GetScore(Tile tile, int id)
        {
			if (tile.WaterCovered)
			{
				return -100f;
			}
			if (tile.temperature < -10f || tile.temperature > 10f)
			{
				return 0f;
			}
			if (tile.rainfall < 1100f)
			{
				return 0f;
			}
			return 40f;
		}
    }
    /*
    class NPS_BiomeWorker_Sequioa : BiomeWorker_TemperateForest
    {
        public override float GetScore(Tile tile)
        {
            if (tile.WaterCovered)
            {
                return -100f;
            }
            if (tile.temperature < -10.0)
            {
                return 0f;
            }
            if (tile.rainfall < 600.0)
            {
                return 0f;
            }

			if (Rand.Value > .0001)
			{
				return 0f;
			}
			return (float)(16.0 + (tile.temperature - 7.0) + (tile.rainfall - 600.0) / 180.0);
        }
    }
	*/
    class NPS_BiomeWorker_LavaFields : BiomeWorker_TropicalRainforest
    {
        public override float GetScore(Tile tile, int id)
        {
			if (base.GetScore(tile, id) > 0)
			{
				if (Rand.Value > .009)
				{
					return 0f;
				}

				return (float)(32.0 + (tile.temperature - 20.0) * 3.5 + (tile.rainfall - 600.0) / 165.0);

			}
			return 0f;

		}
	}

	class NPS_BiomeWorker_Savanna : BiomeWorker_TemperateForest
	{
		public override float GetScore(Tile tile, int id)
		{
			//keep this the same, just make it fail more often. When it fails, shrubland will be rendered, instead.
				if (tile.WaterCovered)
			{
				return -100f;
			}
			if (tile.temperature < 24f || tile.temperature > 30f)
			{
				return 0f;
			}
			if (tile.rainfall < 1400f || tile.rainfall >= 2000f)
			{
				return 0f;
			}

			return 22.5f + (tile.temperature - 7f) + 30 + (tile.rainfall - 0f) / 180f;
		}
	}

	class NPS_BiomeWorker_Prairie : BiomeWorker_AridShrubland
    {
        public override float GetScore(Tile tile, int id)
        {
			//keep this the same, just make it fail more often. When it fails, shrubland will be rendered, instead.
			/*
			if (tile.WaterCovered)
			{
				return -100f;
			}
			if (tile.temperature < -10f)
			{
				return 0f;
			}
			if ((tile.rainfall < 600f) || tile.rainfall >= 2000f)
			{
				return 0f;
			}

			return 22.5f + (tile.temperature - 22f) * 2.2f + (tile.rainfall - 600f) / 100f;
			*/
			if (tile.WaterCovered)
			{
				return -100f;
			}
			if (tile.temperature < -10f || tile.temperature > 22)
			{
				return 0f;
			}
			if (tile.rainfall < 900f || tile.rainfall >= 1300f)
			{
				return 0f;
			}
			if (tile.hilliness != Hilliness.Flat)
			{
				return 0f;
			}

			return 22.5f + (tile.temperature - 20f) * 6.2f + (tile.rainfall - 0f) / 100f;
		}
    }
	/*
    class NPS_BiomeWorker_AridShrubland : BiomeWorker_AridShrubland
    {
        public override float GetScore(Tile tile)
        {
            //keep this the same, just make it fail more often. When it fails, shrubland will be rendered, instead.
            if (tile.WaterCovered)
            {
                return -100f;
            }
            if (tile.temperature < -10f)
            {
                return 0f;
            }
            if (tile.rainfall < 600f || tile.rainfall >= 2000f)
            {
                return 0f;
            }

            if (g.passOrFail(19))
            {

                return 0f; ;
            }

            return 22.5f + (tile.temperature - 20f) * 2.2f + (tile.rainfall - 600f) / 100f;
        }
    }
	*/
}
