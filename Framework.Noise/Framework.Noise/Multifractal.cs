#region Using

using System;

#endregion

namespace Willcraftia.Framework.Noise
{
    public sealed class Multifractal : Musgrave
    {
        float offset = 1f;

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

            float value = 1;

            for (int i = 0; i < octaveCount; i++)
            {
                var signal = Noise(x, y, z) * spectralWeights[i] + offset;
                value *= signal;

                x *= lacunarity;
                y *= lacunarity;
                z *= lacunarity;
            }

            return value;
        }
    }
}
