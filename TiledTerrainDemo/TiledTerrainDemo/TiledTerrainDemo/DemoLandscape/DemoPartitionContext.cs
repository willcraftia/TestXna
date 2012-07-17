#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TiledTerrainDemo.CDLOD;
using TiledTerrainDemo.Noise;

#endregion

namespace TiledTerrainDemo.DemoLandscape
{
    public sealed class DemoPartitionContext
    {
        Settings settings;

        // noise parameters for debug.
        int noiseSeed = 300;

        ImprovedPerlinNoise noise = new ImprovedPerlinNoise();

        SumFractal fractal = new SumFractal();

        DefaultVisibleRanges visibleRanges;

        public GraphicsDevice GraphicsDevice { get; private set; }

        public ContentManager Content { get; private set; }

        public int HeightMapWidth { get; private set; }

        public int HeightMapHeight { get; private set; }

        public float NoiseMinX { get; private set; }

        public float NoiseMinY { get; private set; }

        public float NoiseWidth { get; private set; }

        public float NoiseHeight { get; private set; }

        public Settings Settings
        {
            get { return settings; }
        }

        public TerrainRenderer TerrainRenderer { get; private set; }

        public Selection Selection { get; private set; }

        #region Debug

        public int TotalSelectedNodeCount { get; set;}

        public int DrawPartitionCount { get; set; }

        #endregion

        public DemoPartitionContext(
            GraphicsDevice graphicsDevice, ContentManager content,
            int heightMapWidth, int heightMapHeight,
            float noiseMinX, float noiseMinY, float noiseWidth, float noiseHeight,
            Settings settings)
        {
            GraphicsDevice = graphicsDevice;
            Content = content;
            HeightMapWidth = heightMapWidth;
            HeightMapHeight = heightMapHeight;
            NoiseMinX = noiseMinX;
            NoiseMinY = noiseMinY;
            NoiseWidth = noiseWidth;
            NoiseHeight = noiseHeight;

            this.settings = settings;

            noise.Seed = noiseSeed;
            fractal.Noise3 = noise.Noise;

            visibleRanges = new DefaultVisibleRanges(settings);
            visibleRanges.Initialize();

            TerrainRenderer = new TerrainRenderer(GraphicsDevice, Content, settings);
            TerrainRenderer.InitializeMorphConsts(visibleRanges);

            Selection = new Selection(settings, visibleRanges);
        }

        public float GetNoiseValue(float x, float y)
        {
            return MathHelper.Clamp(fractal.GetValue(x, 0, y), -1, 1);
        }
    }
}
