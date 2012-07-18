#region Using

using System;

#endregion

namespace TiledTerrainDemo.Noise
{
    public static class NoiseHelper
    {
        public static float CubicSCurve(float value)
        {
            return value * value * (3 - 2 * value);
        }
    }
}
