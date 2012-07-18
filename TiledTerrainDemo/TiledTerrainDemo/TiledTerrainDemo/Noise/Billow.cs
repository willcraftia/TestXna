#region Using

using System;

#endregion

namespace TiledTerrainDemo.Noise
{
    public sealed class Billow/* : Fractal*/
    {
        float frequency = 1;

        float lacunarity = 2;

        float persistence = 0.5f;

        int octaveCount = 6;

        public NoiseDelegate Noise { get; set; }

        public float Frequency
        {
            get { return frequency; }
            set { frequency = value; }
        }

        public float Lacunarity
        {
            get { return lacunarity; }
            set { lacunarity = value; }
        }

        public float Persistence
        {
            get { return persistence; }
            set { persistence = value; }
        }

        public int OctaveCount
        {
            get { return octaveCount; }
            set { octaveCount = value; }
        }

        public float GetValue(float x, float y, float z)
        {
            float value = 0;
            float amplitude = 1;

            x *= frequency;
            y *= frequency;
            z *= frequency;

            for (int i = 0; i < octaveCount; i++)
            {
                var signal = Noise(x, y, z);
                signal = 2 * Math.Abs(signal) - 1;
                value += signal * amplitude;

                x *= lacunarity;
                y *= lacunarity;
                z *= lacunarity;

                amplitude *= persistence;
            }

            value += 0.5f;

            return value;
        }

        //protected override float GetValueOverride(float x, float y, float z)
        //{
        //    x *= frequency;
        //    y *= frequency;
        //    z *= frequency;

        //    float value = 0;

        //    for (int i = 0; i < octaveCount; i++)
        //    {
        //        var signal = Noise(x, y, z);
        //        signal = (2 * Math.Abs(signal) - 1) * spectralWeights[i];
        //        value += signal;

        //        x *= lacunarity;
        //        y *= lacunarity;
        //        z *= lacunarity;
        //    }

        //    value += 0.5f;

        //    return value;
        //}
    }
}
