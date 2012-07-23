#region Using

using System;

#endregion

namespace Willcraftia.Framework.Noise
{
    public static class NoiseHelper
    {
        public static float SCurve3(float x)
        {
            return x * x * (3 - 2 * x);
        }

        public static float SCurve5(float x)
        {
            return x * x * x * (x * (x * 6 - 15) + 10);
        }

        public static int Floor(float v)
        {
            // Faster than using (int) Math.Floor(x).
            return 0 < v ? (int) v : (int) v - 1;
        }
    }
}
