#region Using

using System;
using Willcraftia.Framework.Noise;

#endregion

namespace PerlinNoiseDemo
{
    public sealed class Granite : INoiseModule
    {
        Perlin noise = new Perlin();

        Billow primaryGranite = new Billow();

        Voronoi baseGrains = new Voronoi();

        ScaleBias scaledGrains = new ScaleBias();

        Add combinedGranite = new Add();

        Turbulence finalGranite = new Turbulence();

        public void Initialize()
        {
            primaryGranite.Noise = noise.GetValue;
            primaryGranite.Frequency = 8;
            primaryGranite.Persistence = 0.625f;
            primaryGranite.Lacunarity = 2.18359375f;
            primaryGranite.OctaveCount = 6;

            baseGrains.Frequency = 16;
            baseGrains.Metrics = Metrics.Real;
            baseGrains.DistanceEnabled = true;

            scaledGrains.Noise = baseGrains.GetValue;
            scaledGrains.Scale = 0.5f;
            scaledGrains.Bias = 0;

            combinedGranite.Noise0 = primaryGranite.GetValue;
            combinedGranite.Noise1 = scaledGrains.GetValue;

            finalGranite.Noise = combinedGranite.GetValue;
            finalGranite.Frequency = 4;
            finalGranite.Power = 1 / 8.0f;
            finalGranite.Roughness = 6;
        }

        public float GetValue(float x, float y, float z)
        {
            return finalGranite.GetValue(x, y, z);
        }
    }
}
