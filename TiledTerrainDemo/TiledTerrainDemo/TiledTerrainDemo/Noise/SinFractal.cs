#region Using

using System;

#endregion

namespace TiledTerrainDemo.Noise
{
    public sealed class SinFractal : Fractal
    {
        protected override float GetValueOverride(float x, float y, float z)
        {
            x *= frequency;
            y *= frequency;
            z *= frequency;

            float value = 0;

            for (int i = 0; i < octaveCount; i++)
            {
                var signal = Noise(x, y, z) * spectralWeights[i];
                if (signal < 0) signal = -signal;
                value += signal;

                x *= lacunarity;
                y *= lacunarity;
                z *= lacunarity;
            }

            return value;
        }
    }
}
