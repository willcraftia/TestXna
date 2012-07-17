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

        TiledNoiseMap tiledNoiseMap = new TiledNoiseMap();

        DemoHeightMapSource heightMap;

        Terrain terrain;

        public DemoPartition(DemoPartitionContext context)
        {
            this.context = context;

            tiledNoiseMap.Width = context.HeightMapWidth;
            tiledNoiseMap.Height = context.HeightMapHeight;
            tiledNoiseMap.GetValue2 = context.GetNoiseValue;

            heightMap = new DemoHeightMapSource(context.GraphicsDevice);
            heightMap.TiledNoiseMap = tiledNoiseMap;

            terrain = new Terrain(context.GraphicsDevice, context.Settings);
            terrain.HeightMap = heightMap;
        }

        public override void LoadContent()
        {
            // Set the current noise bounds.
            tiledNoiseMap.SetBounds(
                context.NoiseMinX + X * context.NoiseWidth,
                context.NoiseMinY + Y * context.NoiseHeight,
                context.NoiseWidth,
                context.NoiseHeight);
            // Build noise values.
            tiledNoiseMap.Build();

            // Build the height map.
            heightMap.Build();

            // TODO: modified QuadTree and Node to avoid GC.
            // Build the terrain.
            terrain.Build();
        }

        public override void UnloadContent()
        {
            heightMap.Dispose();
            //terrain.Dispose();
        }

        public override void Draw(GameTime gameTime)
        {
            var terrainOffset = new Vector3(X, 0, Y);
            terrainOffset.X *= (context.HeightMapWidth - 1);
            terrainOffset.Z *= (context.HeightMapHeight - 1);
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
    }
}
