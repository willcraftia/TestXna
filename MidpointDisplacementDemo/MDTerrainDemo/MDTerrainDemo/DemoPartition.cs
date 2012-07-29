#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Landscape;
using Willcraftia.Xna.Framework.Terrain;
using Willcraftia.Xna.Framework.Terrain.CDLOD;

#endregion

namespace MDTerrainDemo
{
    public sealed class DemoPartition : Partition
    {
        DemoPartitionContext context;

        CDLODSettings settings;

        Map<float> heightMap;

        CDLODTerrain terrain;

        Texture2D texture;

        MidpointDisplacement md = new MidpointDisplacement();

        public DemoPartition(DemoPartitionContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            this.context = context;

            settings = context.Settings;

            heightMap = new Map<float>(settings.HeightMapWidth, settings.HeightMapHeight);

            terrain = new CDLODTerrain(context.Settings);
            terrain.HeightMap = heightMap;

            texture = new Texture2D(context.GraphicsDevice, settings.HeightMapWidth, settings.HeightMapWidth, false, SurfaceFormat.Single);

            md.Destination = heightMap;
            md.Seed = context.MDSeed;
        }

        public override void Draw(GameTime gameTime)
        {
            var partitionPosition = Position;
            context.Selection.TerrainOffset = new Vector3(partitionPosition.X, 0, partitionPosition.Y);

            // select.
            terrain.Select(context.Selection);

            // set the height map texture.
            context.Selection.HeightMapTexture = texture;

            #region Debug

            context.TotalSelectedNodeCount += context.Selection.SelectedNodeCount;
            context.DrawPartitionCount += 1;

            #endregion

            context.TerrainRenderer.Draw(gameTime, context.Selection);
        }

        protected override void LoadContentOverride()
        {
            md.BoundX = X * (settings.HeightMapWidth - 1);
            md.BoundY = Y * (settings.HeightMapHeight - 1);
            md.Build();

            Erosion.ErodeThermal(heightMap, 0.5f, 5);

            texture.SetData(heightMap.Values);

            // Build the terrain.
            terrain.Build();

            base.LoadContentOverride();
        }

        protected override void DisposeOverride(bool disposing)
        {
            if (disposing)
            {
                texture.Dispose();
            }

            base.DisposeOverride(disposing);
        }
    }
}
