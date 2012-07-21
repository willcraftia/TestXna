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

        // visible ranges.
        int finestNodeSize = 5;

        #region Noise and fractal test

        PerlinNoise perlinNoise = new PerlinNoise();
        ImprovedPerlinNoise improvedPerlinNoise = new ImprovedPerlinNoise();
        SimplexNoise simplexNoise = new SimplexNoise();

        PerlinFractal perlinFractal = new PerlinFractal();
        SumFractal sumFractal = new SumFractal();
        Multifractal multifractal = new Multifractal();
        Heterofractal heterofractal = new Heterofractal();
        HybridMultifractal hybridMultifractal = new HybridMultifractal();
        RidgedMultifractal ridgedMultifractal = new RidgedMultifractal();
        SinFractal sinFractal = new SinFractal();
        Billow billow = new Billow();
        Voronoi voronoi = new Voronoi();

        #endregion

        #region Noise combination test

        RidgedMultifractal mountainTerrain = new RidgedMultifractal();
        Billow baseFlatTerrain = new Billow();
        ScaleBias flatTerrain = new ScaleBias();
        Select terrainSelector = new Select();
        PerlinFractal terrainType = new PerlinFractal();
        Turbulence perturbTerrain = new Turbulence();
        ScaleBias finalTerrain = new ScaleBias();

        #endregion

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

        public DemoTerrainRenderer TerrainRenderer { get; private set; }

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

            #region Noise and fractal test

            perlinNoise.Seed = noiseSeed;
            perlinNoise.Reseed();
            improvedPerlinNoise.Seed = noiseSeed;
            improvedPerlinNoise.Reseed();
            simplexNoise.Seed = noiseSeed;
            simplexNoise.Reseed();
            voronoi.Seed = noiseSeed;

            //var noise = perlinNoise;
            var noise = improvedPerlinNoise;
            //var noise = simplexNoise;
            //var noise = voronoi;

            perlinFractal.Noise = noise.GetValue;
            sumFractal.Noise = noise.GetValue;
            multifractal.Noise = noise.GetValue;
            heterofractal.Noise = noise.GetValue;
            hybridMultifractal.Noise = noise.GetValue;
            ridgedMultifractal.Noise = noise.GetValue;
            sinFractal.Noise = noise.GetValue;
            billow.Noise = noise.GetValue;

            #endregion

            #region Noise combination test

            var testBaseNoise = improvedPerlinNoise;

            mountainTerrain.Noise = testBaseNoise.GetValue;
            baseFlatTerrain.Noise = testBaseNoise.GetValue;
            baseFlatTerrain.Frequency = 2.0f;
            flatTerrain.Noise = baseFlatTerrain.GetValue;
            flatTerrain.Scale = 0.525f;
            flatTerrain.Bias = -0.75f;
            terrainType.Noise = testBaseNoise.GetValue;
            terrainSelector.ControllerNoise = terrainType.GetValue;
            terrainSelector.Noise0 = (x, y, z) => { return mountainTerrain.GetValue(x, y, z) * 1.25f - 1; };
            terrainSelector.Noise1 = flatTerrain.GetValue;
            terrainSelector.LowerBound = 0;
            terrainSelector.UpperBound = 1000;
            terrainSelector.EdgeFalloff = 0.125f;
            perturbTerrain.Noise = terrainSelector.GetValue;
            perturbTerrain.Frequency = 4;
            perturbTerrain.Power = 0.125f;
            finalTerrain.Noise = perturbTerrain.GetValue;
            finalTerrain.Bias = 0.8f;

            #endregion

            visibleRanges = new DefaultVisibleRanges(settings);
            visibleRanges.FinestNodeSize = finestNodeSize;
            visibleRanges.Initialize();

            TerrainRenderer = new DemoTerrainRenderer(GraphicsDevice, Content, settings);
            TerrainRenderer.InitializeMorphConsts(visibleRanges);

            var heightColors = new HeightColorCollection();
            // default settings.
            heightColors.AddColor(-1.0000f, new Color(  0,   0, 128, 255));
            heightColors.AddColor(-0.2500f, new Color(  0,   0, 255, 255));
            heightColors.AddColor( 0.0000f, new Color(  0, 128, 255, 255));
            heightColors.AddColor( 0.0625f, new Color(240, 240,  64, 255));
            heightColors.AddColor( 0.1250f, new Color( 32, 160,   0, 255));
            heightColors.AddColor( 0.3750f, new Color(224, 224,   0, 255));
            heightColors.AddColor( 0.7500f, new Color(128, 128, 128, 255));
            heightColors.AddColor( 1.0000f, new Color(255, 255, 255, 255));

            TerrainRenderer.InitializeHeightColors(heightColors);

            Selection = new Selection(settings, visibleRanges);
        }

        public float Noise(float x, float y, float z)
        {
            //return improvedPerlinNoise.GetValue(x, y, z);
            //return simplexNoise.GetValue(x, y, z);
            //return voronoi.GetValue(x, y, z);
            return sumFractal.GetValue(x, y, z) * 2.5f;
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

            // Noise combination test.
            //return finalTerrain.GetValue(x, y, z);
        }
    }
}
