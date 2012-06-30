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

            float result = (Noise(x, y, z) + offset) * spectralWeights[0];
            float weight = gain * result;

            x *= lacunarity;
            y *= lacunarity;
            z *= lacunarity;

            for (int i = 1; i < octaveCount; i++)
            {
                if (1 < weight) weight = 1;

                float signal = (Noise(x, y, z) + offset) * spectralWeights[i];
                signal *= weight;
                result += signal;
                weight *= gain * signal;

                x *= lacunarity;
                y *= lacunarity;
                z *= lacunarity;
            }

            return result;
        }
    }
}
