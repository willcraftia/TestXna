#region Using

using System;

#endregion

namespace TiledTerrainDemo.Noise
{
    public abstract class Fractal
    {
        public const int MaxOctaveCount = 30;

        public const float DefaultHurst = 0.9f;

        public const float DefaultFrequency = 1;

        public const float DefaultLacunarity = 2;

        public const int DefaultOctaveCount = 6;

        protected float hurst = DefaultHurst;

        protected float frequency = DefaultFrequency;

        protected float lacunarity = DefaultLacunarity;

        protected int octaveCount = DefaultOctaveCount;

        protected float[] spectralWeights = new float[MaxOctaveCount];

        bool initialized;

        public NoiseDelegate Noise { get; set; }

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
            for (int i = 0; i < MaxOctaveCount; i++)
            {
                spectralWeights[i] = (float) Math.Pow(f, -hurst);
                f *= lacunarity;
            }
        }
    }
}
