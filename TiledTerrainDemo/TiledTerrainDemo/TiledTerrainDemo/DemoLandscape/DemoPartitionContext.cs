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

        PerlinNoise perlinNoise = new PerlinNoise();
        ImprovedPerlinNoise improvedPerlinNoise = new ImprovedPerlinNoise();

        SumFractal sumFractal = new SumFractal();
        Turbulence turbulence = new Turbulence();
        Multifractal multifractal = new Multifractal();
        Heterofractal heterofractal = new Heterofractal();
        HybridMultifractal hybridMultifractal = new HybridMultifractal();
        RidgedMultifractal ridgedMultifractal = new RidgedMultifractal();
        SinFractal sinFractal = new SinFractal();

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

            perlinNoise.Seed = noiseSeed;
            improvedPerlinNoise.Seed = noiseSeed;

            //var noise = perlinNoise;
            var noise = improvedPerlinNoise;

            sumFractal.Noise3 = noise.Noise;
            turbulence.Noise3 = noise.Noise;
            multifractal.Noise3 = noise.Noise;
            heterofractal.Noise3 = noise.Noise;
            hybridMultifractal.Noise3 = noise.Noise;
            ridgedMultifractal.Noise3 = noise.Noise;
            sinFractal.Noise3 = noise.Noise;

            visibleRanges = new DefaultVisibleRanges(settings);
            visibleRanges.Initialize();

            TerrainRenderer = new TerrainRenderer(GraphicsDevice, Content, settings);
            TerrainRenderer.InitializeMorphConsts(visibleRanges);

            Selection = new Selection(settings, visibleRanges);
        }

        public float GetNoiseValue(float x, float y)
        {
            return sumFractal.GetValue(x, 0, y);
            //return turbulence.GetValue(x, 0, y);
            // take down.
            //return multifractal.GetValue(x, 0, y) - 1;
            // take down.
            //return heterofractal.GetValue(x, 0, y) - 1;
            // take down.
            //return hybridMultifractal.GetValue(x, 0, y) - 1;
            // take down.
            //return ridgedMultifractal.GetValue(x, 0, y) - 1;
            //return sinFractal.GetValue(x, 0, y);
        }
    }
}
