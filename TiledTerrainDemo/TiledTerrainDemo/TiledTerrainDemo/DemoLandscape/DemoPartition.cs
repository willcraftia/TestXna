﻿#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

        bool heightMapTextureDirty;

        Texture2D heightMapTexture;

        public DemoPartition(DemoPartitionContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            this.context = context;

            settings = context.Settings;

            tiledNoiseMap.Width = settings.HeightMapWidth;
            tiledNoiseMap.Height = settings.HeightMapHeight;
            tiledNoiseMap.OverlapSize = settings.HeightMapOverlapSize;
            tiledNoiseMap.Noise = context.Noise;

            heightMap = new DemoHeightMapSource();
            heightMap.TiledNoiseMap = tiledNoiseMap;

            terrain = new Terrain(context.Settings);
            terrain.HeightMap = heightMap;
        }

        public override void Draw(GameTime gameTime)
        {
            RefreshHeightMapTexture();

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

        protected override void LoadContentOverride()
        {
            // Build noise values with the noise bounds for this partition.
            tiledNoiseMap.SetBounds(
                context.NoiseMinX + X * context.NoiseWidth,
                context.NoiseMinY + Y * context.NoiseHeight,
                context.NoiseWidth,
                context.NoiseHeight);
            tiledNoiseMap.Build();
            //tiledNoiseMap.Erode(16 / (float) context.Settings.HeightMapWidth, 10);

            // heightMapTexture is dirty.
            heightMapTextureDirty = true;

            // Build the terrain.
            terrain.Build();

            base.LoadContentOverride();
        }

        protected override void DisposeOverride(bool disposing)
        {
            if (disposing)
            {
                if (heightMapTexture != null) heightMapTexture.Dispose();
            }

            base.DisposeOverride(disposing);
        }

        void RefreshHeightMapTexture()
        {
            if (!heightMapTextureDirty) return;

            int w = tiledNoiseMap.ActualWidth;
            int h = tiledNoiseMap.ActualHeight;

            if (heightMapTexture == null || heightMapTexture.Width != w || heightMapTexture.Height != h)
            {
                if (heightMapTexture != null) heightMapTexture.Dispose();

                heightMapTexture = new Texture2D(context.GraphicsDevice, w, h, false, SurfaceFormat.Single);
            }

            //if (heightMapTexture != null) heightMapTexture.Dispose();
            //heightMapTexture = new Texture2D(context.GraphicsDevice, w, h, false, SurfaceFormat.Single);

            heightMapTexture.SetData(tiledNoiseMap.Values);

            heightMap.Texture = heightMapTexture;

            heightMapTextureDirty = false;
        }
    }
}
