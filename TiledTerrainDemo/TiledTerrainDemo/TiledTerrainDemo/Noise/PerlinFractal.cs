#region Using

using System;

#endregion

namespace TiledTerrainDemo.Noise
{
    /// <summary>
    /// ノイズを調波させるクラスです。
    /// </summary>
    public sealed class PerlinFractal
    {
        NoiseDelegate noise;

        float frequency = 1;

        float lacunarity = 2;

        float persistence = 0.5f;

        int octaveCount = 6;

        public NoiseDelegate Noise
        {
            get { return noise; }
            set { noise = value; }
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
