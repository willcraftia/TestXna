#region Using

using System;

#endregion

namespace Willcraftia.Framework.Noise
{
    public sealed class PerlinFractal : INoiseModule
    {
        public const float DefaultFrequency = 1;

        public const float DefaultLacunarity = 2;

        public const float DefaultPersistence = 0.5f;

        public const int DefaultOctave = 6;

        NoiseDelegate noise;

        float frequency = DefaultFrequency;

        float lacunarity = DefaultLacunarity;

        float persistence = DefaultPersistence;

        int octaveCount = DefaultOctave;

        public NoiseDelegate Noise
        {
            get { return noise; }
            set { noise = value; }
        }

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
                var signal = noise(x, y, z);
                value += signal * amplitude;

                x *= lacunarity;
                y *= lacunarity;
                z *= lacunarity;

                amplitude *= persistence;
            }

            return value;
        }
    }
}
