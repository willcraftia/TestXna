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

        PerlinFractal perlinFractal = new PerlinFractal();
        SumFractal sumFractal = new SumFractal();
        Turbulence turbulence = new Turbulence();
        Multifractal multifractal = new Multifractal();
        Heterofractal heterofractal = new Heterofractal();
        HybridMultifractal hybridMultifractal = new HybridMultifractal();
        RidgedMultifractal ridgedMultifractal = new RidgedMultifractal();
        SinFractal sinFractal = new SinFractal();
        Billow billow = new Billow();

        ScaleBias scaleBias = new ScaleBias();
        Select select = new Select();

        DefaultVisibleRanges visibleRanges;

        public GraphicsDevice GraphicsDevice { get; private set; }

        public ContentManager Content { get; private set; }

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
            GraphicsDevice graphicsDevice, ContentManager content, Settings settings,
            float noiseMinX, float noiseMinY, float noiseWidth, float noiseHeight)
        {
            GraphicsDevice = graphicsDevice;
            Content = content;
            this.settings = settings;
            NoiseMinX = noiseMinX;
            NoiseMinY = noiseMinY;
            NoiseWidth = noiseWidth;
            NoiseHeight = noiseHeight;

            perlinNoise.Seed = noiseSeed;
            improvedPerlinNoise.Seed = noiseSeed;

            //var noise = perlinNoise;
            var noise = improvedPerlinNoise;

            sumFractal.Noise = noise.GetValue;
            multifractal.Noise = noise.GetValue;
            heterofractal.Noise = noise.GetValue;
            hybridMultifractal.Noise = noise.GetValue;
            ridgedMultifractal.Noise = noise.GetValue;
            sinFractal.Noise = noise.GetValue;
            billow.Noise = noise.GetValue;

            scaleBias.Noise = billow.GetValue;
            scaleBias.Scale = 0.125f;
            scaleBias.Bias = -0.75f;
            //scaleBias.Bias = 0.5f;

            perlinFractal.Noise = noise.GetValue;
            perlinFractal.Frequency = 0.5f;
            perlinFractal.Persistence = 0.25f;

            select.ControllerNoise = perlinFractal.GetValue;
            //select.Noise0 = ridgedMultifractal.GetValue;
            select.Noise0 = (x, y, z) => { return ridgedMultifractal.GetValue(x, y, z) * 1.25f - 1; };
            select.Noise1 = scaleBias.GetValue;
            select.LowerBound = 0;
            select.UpperBound = 1000;
            select.EdgeFalloff = 0.125f;

            turbulence.Noise = select.GetValue;
            turbulence.Frequency = 4;
            turbulence.Power = 0.125f;

            visibleRanges = new DefaultVisibleRanges(settings);
            visibleRanges.Initialize();

            TerrainRenderer = new TerrainRenderer(GraphicsDevice, Content, settings);
            TerrainRenderer.InitializeMorphConsts(visibleRanges);

            Selection = new Selection(settings, visibleRanges);
        }

        public float Noise(float x, float y, float z)
        {
            //return sumFractal.GetValue(x, y, z);
            // take down.
            //return multifractal.GetValue(x, y, z) - 1;
            // take down.
            //return heterofractal.GetValue(x, y, z) - 1;
            // take down.
            //return hybridMultifractal.GetValue(x, y, z) - 1;
            // take down.
            //return ridgedMultifractal.GetValue(x, y, z) - 1;
            //return sinFractal.GetValue(x, y, z);
            //return billow.GetValue(x, y, z);
            //return scaleBias.GetValue(x, y, z);
            //return select.GetValue(x, y, z);
            return turbulence.GetValue(x, y, z);
        }
    }
}
