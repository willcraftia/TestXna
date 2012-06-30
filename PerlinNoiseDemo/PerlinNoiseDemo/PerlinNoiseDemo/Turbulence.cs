#region Using

using System;

#endregion

namespace PerlinNoiseDemo
{
    public sealed class Turbulence
    {
        public delegate float DelegateNoise1(float x);

        public delegate float DelegateNoise2(float x, float y);

        public delegate float DelegateNoise3(float x, float y, float z);

        DelegateNoise1 noise1;

        DelegateNoise2 noise2;
        
        DelegateNoise3 noise3;

        float frequency = 1;

        float lacunarity = 2;

        float persistence = 0.5f;

        int octaveCount = 6;

        public DelegateNoise1 Noise1
        {
            get { return noise1; }
            set { noise1 = value; }
        }

        public DelegateNoise2 Noise2
        {
            get { return noise2; }
            set { noise2 = value; }
        }

        public DelegateNoise3 Noise3
        {
            get { return noise3; }
            set { noise3 = value; }
        }

        /// <summary>
        /// Frequency を取得または設定します。
        /// </summary>
        public float Frequency
        {
            get { return frequency; }
            set { frequency = value; }
        }

        /// <summary>
        /// 各 octave における frequency の増加の度合いを取得または設定します。
        /// 各 octave の frequency は、1 つ前の octave の frequency に lacunarity を掛けた値です。
        /// </summary>
        public float Lacunarity
        {
            get { return lacunarity; }
            set { lacunarity = value; }
        }

        /// <summary>
        /// 各 octave における amplitude の増加の度合いを取得または設定します。
        /// 各 octave の amplitude は、1 つ前の octave の amplitude に persistece を掛けた値です。
        /// </summary>
        public float Persistence
        {
            get { return persistence; }
            set { persistence = value; }
        }

        /// <summary>
        /// Octave 数を取得または設定します。
        /// </summary>
        public int OctaveCount
        {
            get { return octaveCount; }
            set { octaveCount = value; }
        }

        public float GetValue1(float x)
        {
            if (noise1 == null) throw new InvalidOperationException("Noise1 delegate is null.");

            float result = 0;
            float amplitude = 1;

            x *= frequency;

            for (int i = 0; i < octaveCount; i++)
            {
                result += noise1(x) * amplitude;

                x *= lacunarity;

                amplitude *= persistence;
            }

            return result;
        }

        public float GetValue2(float x, float y)
        {
            if (noise2 == null) throw new InvalidOperationException("Noise2 delegate is null.");

            float result = 0;
            float amplitude = 1;

            x *= frequency;
            y *= frequency;

            for (int i = 0; i < octaveCount; i++)
            {
                result += noise2(x, y) * amplitude;

                x *= lacunarity;
                y *= lacunarity;

                amplitude *= persistence;
            }

            return result;
        }

        public float GetValue3(float x, float y, float z)
        {
            if (noise3 == null) throw new InvalidOperationException("Noise3 delegate is null.");

            float result = 0;
            float amplitude = 1;

            x *= frequency;
            y *= frequency;
            z *= frequency;

            for (int i = 0; i < octaveCount; i++)
            {
                result += noise3(x, y, z) * amplitude;

                x *= lacunarity;
                y *= lacunarity;
                z *= lacunarity;

                amplitude *= persistence;
            }

            return result;
        }
    }
}
