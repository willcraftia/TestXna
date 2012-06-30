#region Using

using System;

#endregion

namespace PerlinNoiseDemo
{
    /// <summary>
    /// ノイズを調波させるクラスです。
    /// </summary>
    public sealed class PerlinFractal
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

        /// <summary>
        /// 1 次元ノイズ関数を取得または設定します。
        /// </summary>
        public DelegateNoise1 Noise1
        {
            get { return noise1; }
            set { noise1 = value; }
        }

        /// <summary>
        /// 2 次元ノイズ関数を取得または設定します。
        /// </summary>
        public DelegateNoise2 Noise2
        {
            get { return noise2; }
            set { noise2 = value; }
        }

        /// <summary>
        /// 3 次元ノイズ関数を取得または設定します。
        /// </summary>
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

        /// <summary>
        /// 1 次元ノイズ値を取得します。
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public float GetValue(float x)
        {
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

        /// <summary>
        /// 2 次元ノイズ値を取得します。
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public float GetValue(float x, float y)
        {
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

        /// <summary>
        /// 3 次元ノイズ値を取得します。
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public float GetValue(float x, float y, float z)
        {
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
