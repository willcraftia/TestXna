#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Noise
{
    public sealed class Heterofractal : Musgrave
    {
        float offset = 0.7f;

        public float Offset
        {
            get { return offset; }
            set { offset = value; }
        }

        protected override float GetValueOverride(float x, float y, float z)
        {
            x *= frequency;
            y *= frequency;
            z *= frequency;

            float signal = Source(x, y, z) + offset;
            float value = signal;

            x *= lacunarity;
            y *= lacunarity;
            z *= lacunarity;

            for (int i = 1; i < octaveCount; i++)
            {
                signal = Source(x, y, z) + offset;
                signal *= spectralWeights[i];
                signal *= value;
                value += signal;

                x *= lacunarity;
                y *= lacunarity;
                z *= lacunarity;
            }

            return value;
        }
    }
}
