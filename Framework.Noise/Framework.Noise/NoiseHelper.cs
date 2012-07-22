#region Using

using System;

#endregion

namespace Willcraftia.Framework.Noise
{
    public static class NoiseHelper
    {
        public static float SCurve(float value)
        {
            return value * value * (3 - 2 * value);
        }

        public static int Floor(float v)
        {
            // Faster than using (int) Math.Floor(x).
            return 0 < v ? (int) v : (int) v - 1;
        }
    }
}
