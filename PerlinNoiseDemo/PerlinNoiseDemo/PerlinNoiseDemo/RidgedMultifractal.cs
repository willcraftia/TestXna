﻿#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace PerlinNoiseDemo
{
    public sealed class RidgedMultifractal : Fractal
    {
        float offset = 1;

        float gain = 2;

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
                var signal = Noise(x, y, z);

                signal = Math.Abs(signal);
                signal = offset - signal;
                signal *= signal;

                signal *= weight;

                weight = signal * gain;
                weight = MathHelper.Clamp(weight, 0, 1);

                value += signal * spectralWeights[i];

                x *= lacunarity;
                y *= lacunarity;
                z *= lacunarity;
            }

            return value;
        }

        float SqrtWithSign(float value)
        {
            return value < 0 ? -(value * value) : value * value;
        }
    }
}
