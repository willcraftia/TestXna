#region Using

using System;

#endregion

namespace TiledTerrainDemo.Noise
{
    public abstract class Fractal
    {
        public delegate float DelegateNoise1(float x);

        public delegate float DelegateNoise2(float x, float y);

        public delegate float DelegateNoise3(float x, float y, float z);

        protected const int maxOctaveCount = 30;

        protected float hurst = 0.9f;

        protected float frequency = 1;

        protected float lacunarity = 2;

        protected float[] spectralWeights = new float[maxOctaveCount];

        protected int octaveCount = 6;

        DelegateNoise1 noise1;

        DelegateNoise2 noise2;

        DelegateNoise3 noise3;

        bool initialized;

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
        /// Hurst を取得または設定します。
        /// </summary>
        public float Hurst
        {
            get { return hurst; }
            set
            {
                if (hurst == value) return;

                hurst = value;
                initialized = false;
            }
        }

        /// <summary>
        /// Frequency を取得または設定します。
        /// </summary>
        public float Frequency
        {
            get { return frequency; }
            set
            {
                if (frequency == value) return;

                frequency = value;
                initialized = false;
            }
        }

        /// <summary>
        /// 各 octave における frequency の増加の度合いを取得または設定します。
        /// 各 octave の frequency は、1 つ前の octave の frequency に lacunarity を掛けた値です。
        /// </summary>
        public float Lacunarity
        {
            get { return lacunarity; }
            set
            {
                if (lacunarity == value) return;

                lacunarity = value;
                initialized = false;
            }
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
            if (!initialized) Initialize();

            return GetValueOverride(x, y, z);
        }

        protected abstract float GetValueOverride(float x, float y, float z);

        protected virtual void Initialize()
        {
            InitializeSpectralWeights();
            initialized = true;
        }

        protected void InitializeSpectralWeights()
        {
            float f = frequency;
            for (int i = 0; i < maxOctaveCount; i++)
            {
                spectralWeights[i] = (float) Math.Pow(f, -hurst);
                f *= lacunarity;
            }
        }

        protected float Noise(float x)
        {
            return noise1(x);
        }

        protected float Noise(float x, float y)
        {
            return noise2(x, y);
        }

        protected float Noise(float x, float y, float z)
        {
            return noise3(x, y, z);
        }
    }
}
