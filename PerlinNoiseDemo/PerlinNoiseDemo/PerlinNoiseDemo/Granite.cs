#region Using

using System;
using Willcraftia.Framework.Noise;

#endregion

namespace PerlinNoiseDemo
{
    public sealed class Granite : IModule
    {
        Perlin noise = new Perlin();

        Billow primaryGranite = new Billow();

        Voronoi baseGrains = new Voronoi();

        ScaleBias scaledGrains = new ScaleBias();

        Add combinedGranite = new Add();

        Turbulence finalGranite = new Turbulence();

        public void Initialize()
        {
            primaryGranite.Source = noise.Sample;
            primaryGranite.Frequency = 8;
            primaryGranite.Persistence = 0.625f;
            primaryGranite.Lacunarity = 2.18359375f;
            primaryGranite.OctaveCount = 6;

            baseGrains.Frequency = 16;
            baseGrains.Metrics = Metrics.Real;
            baseGrains.DistanceEnabled = true;

            scaledGrains.Source = baseGrains.Sample;
            scaledGrains.Scale = 0.5f;
            scaledGrains.Bias = 0;

            combinedGranite.Source0 = primaryGranite.Sample;
            combinedGranite.Source1 = scaledGrains.Sample;

            finalGranite.Source = combinedGranite.Sample;
            finalGranite.Frequency = 4;
            finalGranite.Power = 1 / 8.0f;
            finalGranite.Roughness = 6;
        }

        public float Sample(float x, float y, float z)
        {
            return finalGranite.Sample(x, y, z);
        }
    }
}
