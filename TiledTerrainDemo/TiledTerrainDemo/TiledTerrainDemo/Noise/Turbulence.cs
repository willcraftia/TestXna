#region Using

using System;

#endregion

namespace TiledTerrainDemo.Noise
{
    public sealed class Turbulence : Fractal
    {
        protected override float GetValueOverride(float x, float y, float z)
        {
            x *= frequency;
            y *= frequency;
            z *= frequency;

            float value = 0;

            for (int i = 0; i < octaveCount; i++)
            {
                value += Math.Abs(Noise(x, y, z)) * spectralWeights[i];

                x *= lacunarity;
                y *= lacunarity;
                z *= lacunarity;
            }

            return value;
        }
    }
}
