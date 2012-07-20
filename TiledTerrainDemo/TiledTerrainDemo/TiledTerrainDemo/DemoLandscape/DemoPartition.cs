#region Using

using System;
using Microsoft.Xna.Framework;
using TiledTerrainDemo.CDLOD;
using TiledTerrainDemo.Landscape;
using TiledTerrainDemo.Noise;

#endregion

namespace TiledTerrainDemo.DemoLandscape
{
    public sealed class DemoPartition : Partition
    {
        DemoPartitionContext context;

        Settings settings;

        TiledNoiseMap tiledNoiseMap = new TiledNoiseMap();

        DemoHeightMapSource heightMap;

        Terrain terrain;

        public DemoPartition(DemoPartitionContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            this.context = context;

            settings = context.Settings;

            tiledNoiseMap.Width = settings.HeightMapWidth;
            tiledNoiseMap.Height = settings.HeightMapHeight;
            tiledNoiseMap.OverlapSize = settings.HeightMapOverlapSize;
            tiledNoiseMap.Noise = context.Noise;

            heightMap = new DemoHeightMapSource(context.GraphicsDevice);
            heightMap.TiledNoiseMap = tiledNoiseMap;

            terrain = new Terrain(context.Settings);
            terrain.HeightMap = heightMap;
        }

        public override void LoadContent()
        {
            // Build noise values with the noise bounds for this partition.
            tiledNoiseMap.SetBounds(
                context.NoiseMinX + X * context.NoiseWidth,
                context.NoiseMinY + Y * context.NoiseHeight,
                context.NoiseWidth,
                context.NoiseHeight);
            tiledNoiseMap.Build();
            //tiledNoiseMap.Erode(16 / (float) context.Settings.HeightMapWidth, 10);

            // Build the height map.
            heightMap.Build();

            // Build the terrain.
            terrain.Build();
        }

        public override void Draw(GameTime gameTime)
        {
            var terrainOffset = new Vector3(X, 0, Y);
            terrainOffset.X *= (settings.HeightMapWidth - 1);
            terrainOffset.Z *= (settings.HeightMapHeight - 1);
            terrainOffset *= context.Settings.PatchScale;

            context.Selection.TerrainOffset = terrainOffset;

            // select.
            terrain.Select(context.Selection);

            #region Debug

            context.TotalSelectedNodeCount += context.Selection.SelectedNodeCount;
            context.DrawPartitionCount += 1;

            #endregion

            context.TerrainRenderer.Draw(gameTime, context.Selection);
        }

        protected override void DisposeOverride(bool disposing)
        {
            if (disposing)
            {
                heightMap.Dispose();
            }

            base.DisposeOverride(disposing);
        }
    }
}
