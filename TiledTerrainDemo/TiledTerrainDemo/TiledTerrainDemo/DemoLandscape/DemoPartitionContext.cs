#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Noise;
using Willcraftia.Xna.Framework.Terrain.CDLOD;

#endregion

namespace TiledTerrainDemo.DemoLandscape
{
    public sealed class DemoPartitionContext
    {
        CDLODSettings settings;

        public GraphicsDevice GraphicsDevice { get; private set; }

        public ContentManager Content { get; private set; }

        public float NoiseMinX { get; private set; }

        public float NoiseMinY { get; private set; }

        public float NoiseWidth { get; private set; }

        public float NoiseHeight { get; private set; }

        public SampleSourceDelegate NoiseSource { get; private set; }

        public CDLODSettings Settings
        {
            get { return settings; }
        }

        public DemoTerrainRenderer TerrainRenderer { get; private set; }

        public CDLODSelection Selection { get; private set; }

        #region Debug

        public int TotalSelectedNodeCount { get; set;}

        public int DrawPartitionCount { get; set; }

        #endregion

        public DemoPartitionContext(
            GraphicsDevice graphicsDevice, ContentManager content, CDLODSettings settings,
            ICDLODVisibleRanges visibleRanges, SampleSourceDelegate noiseSource,
            float noiseMinX, float noiseMinY, float noiseWidth, float noiseHeight)
        {
            GraphicsDevice = graphicsDevice;
            Content = content;
            this.settings = settings;
            NoiseSource = noiseSource;
            NoiseMinX = noiseMinX;
            NoiseMinY = noiseMinY;
            NoiseWidth = noiseWidth;
            NoiseHeight = noiseHeight;

            TerrainRenderer = new DemoTerrainRenderer(GraphicsDevice, Content, settings);
            TerrainRenderer.InitializeMorphConsts(visibleRanges);

            var heightColors = new HeightColorCollection();
            // default settings.
            heightColors.AddColor(-1.0000f, Color.Navy);
            heightColors.AddColor(-0.2500f, Color.Blue);
            heightColors.AddColor( 0.0000f, Color.LightSeaGreen);
            heightColors.AddColor( 0.0625f, Color.Khaki);
            heightColors.AddColor( 0.1250f, Color.DarkGreen);
            heightColors.AddColor( 0.3750f, Color.DarkGoldenrod);
            heightColors.AddColor( 0.7500f, Color.Gray);
            heightColors.AddColor( 1.0000f, Color.White);

            TerrainRenderer.InitializeHeightColors(heightColors);

            Selection = new CDLODSelection(settings, visibleRanges);
        }

        public float Noise(float x, float y, float z)
        {
            return NoiseSource(x, y, z);
        }
    }
}
