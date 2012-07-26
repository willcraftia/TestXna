#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Noise
{
    public sealed class Billow : IModule
    {
        float frequency = 1;

        float lacunarity = 2;

        float persistence = 0.5f;

        int octaveCount = 6;

        public SampleSourceDelegate Source { get; set; }

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

        public float Sample(float x, float y, float z)
        {
            float value = 0;
            float amplitude = 1;

            x *= frequency;
            y *= frequency;
            z *= frequency;

            for (int i = 0; i < octaveCount; i++)
            {
                var signal = Source(x, y, z);
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
    }
}
