#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Framework.Noise;
using TiledTerrainDemo.CDLOD;

#endregion

namespace TiledTerrainDemo.DemoLandscape
{
    public sealed class DemoPartitionContext
    {
        Settings settings;

        // noise parameters for debug.
        int noiseSeed = 300;

        #region Noise and fractal test

        ClassicPerlin classicPerlin = new ClassicPerlin();
        Perlin perlin = new Perlin();
        Simplex simplex = new Simplex();

        PerlinFractal perlinFractal = new PerlinFractal();

        // Musgrave fractal.
        SumFractal sumFractal = new SumFractal();
        Multifractal multifractal = new Multifractal();
        Heterofractal heterofractal = new Heterofractal();
        HybridMultifractal hybridMultifractal = new HybridMultifractal();
        RidgedMultifractal ridgedMultifractal = new RidgedMultifractal();
        SinFractal sinFractal = new SinFractal();
        // ---

        Billow billow = new Billow();

        #endregion

        #region Voronoi test

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

        #region Recording

        Perlin recNoise = new Perlin();
        SumFractal recBaseTerrain = new SumFractal();
        ScaleBias recFinalTerrain = new ScaleBias();

        #endregion

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
            IVisibleRanges visibleRanges,
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

            classicPerlin.Seed = noiseSeed;
            classicPerlin.Reseed();
            perlin.Seed = noiseSeed;
            perlin.Reseed();
            simplex.Seed = noiseSeed;
            simplex.Reseed();

            //var noise = perlinNoise;
            var noise = perlin;
            //var noise = simplexNoise;
            //var noise = voronoi;

            perlinFractal.Source = noise.Sample;
            sumFractal.Source = noise.Sample;
            multifractal.Source = noise.Sample;
            heterofractal.Source = noise.Sample;
            hybridMultifractal.Source = noise.Sample;
            ridgedMultifractal.Source = noise.Sample;
            sinFractal.Source = noise.Sample;
            billow.Source = noise.Sample;

            #endregion

            #region Vorononi test

            voronoi.Seed = noiseSeed;
            voronoi.Frequency = 1;
            voronoi.VoronoiType = VoronoiType.First;
            voronoi.Metrics = Metrics.Squared;
            //voronoi.DistanceEnabled = true;

            #endregion

            #region Noise combination test

            var testBaseNoise = perlin;

            mountainTerrain.Source = testBaseNoise.Sample;
            baseFlatTerrain.Source = testBaseNoise.Sample;
            baseFlatTerrain.Frequency = 2.0f;
            flatTerrain.Source = baseFlatTerrain.Sample;
            flatTerrain.Scale = 0.525f;
            flatTerrain.Bias = -0.75f;
            terrainType.Source = testBaseNoise.Sample;
            terrainSelector.Controller = terrainType.Sample;
            terrainSelector.Source0 = (x, y, z) => { return mountainTerrain.Sample(x, y, z) * 1.25f - 1; };
            terrainSelector.Source1 = flatTerrain.Sample;
            terrainSelector.LowerBound = 0;
            terrainSelector.UpperBound = 1000;
            terrainSelector.EdgeFalloff = 0.125f;
            perturbTerrain.Source = terrainSelector.Sample;
            perturbTerrain.Frequency = 4;
            perturbTerrain.Power = 0.125f;
            finalTerrain.Source = perturbTerrain.Sample;
            finalTerrain.Bias = 0.8f;

            #endregion

            #region Recording

            recNoise.Seed = noiseSeed;
            recBaseTerrain.Source = recNoise.Sample;
            recBaseTerrain.OctaveCount = 7;
            recFinalTerrain.Source = recBaseTerrain.Sample;
            recFinalTerrain.Scale = 2.5f;

            #endregion

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
            //return perlin.Sample(x, y, z);
            //return simplex.Sample(x, y, z);
            //return voronoi.Sample(x, y, z);
            //return sumFractal.Sample(x, y, z) * 2.5f + 0.3f;
            // take down.
            //return multifractal.Sample(x, y, z) - 1;
            // take down.
            //return heterofractal.Sample(x, y, z) - 1;
            // take down.
            //return hybridMultifractal.Sample(x, y, z) - 1;
            // take down.
            //return ridgedMultifractal.Sample(x, y, z) - 1;
            //return sinFractal.Sample(x, y, z);
            //return billow.Sample(x, y, z);

            // Noise combination test.
            //return finalTerrain.Sample(x, y, z);

            // for recoding.
            return recFinalTerrain.Sample(x, y, z);
        }
    }
}
