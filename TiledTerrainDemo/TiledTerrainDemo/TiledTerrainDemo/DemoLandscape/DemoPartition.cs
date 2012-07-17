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

        NoiseMap noiseMap = new NoiseMap();

        DemoHeightMapSource heightMap = new DemoHeightMapSource();

        Terrain terrain;

        public DemoPartition(DemoPartitionContext context)
        {
            this.context = context;

            noiseMap.Width = context.HeightMapWidth;
            noiseMap.Height = context.HeightMapHeight;
            noiseMap.GetValue2 = context.GetNoiseValue;

            heightMap.Width = context.HeightMapWidth;
            heightMap.Height = context.HeightMapHeight;
            heightMap.NoiseMap = noiseMap;

            terrain = new Terrain(context.GraphicsDevice, context.Settings);
        }

        public override void LoadContent()
        {
            // Set the current noise bounds.
            noiseMap.SetBounds(
                context.NoiseMinX + X * context.NoiseWidth,
                context.NoiseMinY + Y * context.NoiseHeight,
                context.NoiseWidth,
                context.NoiseHeight);

            noiseMap.Build();

            // TODO: modified QuadTree and Node to avoid GC.
            terrain.Initialize(heightMap);
        }

        public override void UnloadContent()
        {
            terrain.Dispose();
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
