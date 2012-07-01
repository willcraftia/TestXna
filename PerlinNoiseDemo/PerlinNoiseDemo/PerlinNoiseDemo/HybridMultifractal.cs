#region Using

using System;

#endregion

namespace PerlinNoiseDemo
{
    public sealed class HybridMultifractal : Fractal
    {
        float offset = 0;

        float gain = 1;

        public float Offset
        {
            get { return offset; }
            set { offset = value; }
        }

        public float Gain
        {
            get { return gain; }
            set { gain = value; }
        }

        protected override float GetValueOverride(float x, float y, float z)
        {
            x *= frequency;
            y *= frequency;
            z *= frequency;

            float value = 0;
            float weight = 1;

            for (int i = 0; i < octaveCount; i++)
            {
                float signal = (Noise(x, y, z) + offset) * spectralWeights[i];
                signal *= weight;
                value += signal;

                weight *= gain * signal;
                if (1 < weight) weight = 1;

                x *= lacunarity;
                y *= lacunarity;
                z *= lacunarity;
            }

            return value;
        }
    }
}
