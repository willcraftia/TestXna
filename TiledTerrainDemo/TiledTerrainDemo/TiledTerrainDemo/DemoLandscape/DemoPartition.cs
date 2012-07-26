#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Noise;
using TiledTerrainDemo.CDLOD;
using TiledTerrainDemo.Landscape;

#endregion

namespace TiledTerrainDemo.DemoLandscape
{
    public sealed class DemoPartition : Partition
    {
        DemoPartitionContext context;

        CDLODSettings settings;

        DemoHeightMapSource heightMap;

        CDLODTerrain terrain;

        public DemoPartition(DemoPartitionContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            this.context = context;

            settings = context.Settings;

            heightMap = new DemoHeightMapSource(context.GraphicsDevice, settings);
            heightMap.NoiseSource = context.Noise;

            terrain = new CDLODTerrain(context.Settings);
            terrain.HeightMap = heightMap;
        }

        public override void Draw(GameTime gameTime)
        {
            var partitionPosition = Position;
            context.Selection.TerrainOffset = new Vector3(partitionPosition.X, 0, partitionPosition.Y);

            // select.
            terrain.Select(context.Selection);

            // set the height map texture.
            context.Selection.HeightMapTexture = heightMap.Texture;

            #region Debug

            context.TotalSelectedNodeCount += context.Selection.SelectedNodeCount;
            context.DrawPartitionCount += 1;

            #endregion

            context.TerrainRenderer.Draw(gameTime, context.Selection);
        }

        protected override void LoadContentOverride()
        {
            // Build noise values with the noise bounds for this partition.
            heightMap.Bounds = new Bounds
            {
                X = context.NoiseMinX + X * context.NoiseWidth,
                Y = context.NoiseMinY + Y * context.NoiseHeight,
                Width = context.NoiseWidth,
                Height = context.NoiseHeight
            };
            heightMap.Build();
            //tiledNoiseMap.Erode(16 / (float) context.Settings.HeightMapWidth, 10);

            // Build the terrain.
            terrain.Build();

            base.LoadContentOverride();
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
