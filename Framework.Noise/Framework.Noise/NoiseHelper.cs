#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Noise
{
    public static class NoiseHelper
    {
        public static int Floor(float v)
        {
            // Faster than using (int) Math.Floor(x).
            return 0 < v ? (int) v : (int) v - 1;
        }

        public static float PassThrough(float x)
        {
            return x;
        }

        public static float SCurve3(float x)
        {
            return x * x * (3 - 2 * x);
        }

        public static float SCurve5(float x)
        {
            return x * x * x * (x * (x * 6 - 15) + 10);
        }

        public static float CalculateGradient(int hash, float x, float y, float z)
        {
            // Convert low 4 bits of hash code into 12 simple
            int h = hash & 15;
            // gradient directions, and compute dot product.
            float u = h < 8 ? x : y;
            // Fix repeats at h = 12 to 15
            float v = h < 4 ? y : h == 12 || h == 14 ? x : z;
            return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
        }
    }
}
