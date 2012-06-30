#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace PerlinNoiseDemo
{
    public sealed class RidgedMultifractal : Fractal
    {
        float offset;

        float gain;

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

            float signal = Noise(x, y, z);
            if (signal < 0) signal = -signal;

            signal = offset - signal;
            signal *= signal;

            float value = signal;
            float weight = 1;

            for (int i = 1; i < octaveCount; i++)
            {
                x *= lacunarity;
                y *= lacunarity;
                z *= lacunarity;

                weight = MathHelper.Clamp(signal * gain , 0, 1);

                signal = Noise(x, y, z);
                if (signal < 0) signal = -signal;

                signal = offset - signal;
                signal *= signal;
                signal *= weight;

                value += signal * spectralWeights[i];
            }

            return value;
        }

        float SqrtWithSign(float value)
        {
            return value < 0 ? -(value * value) : value * value;
        }
    }
}
