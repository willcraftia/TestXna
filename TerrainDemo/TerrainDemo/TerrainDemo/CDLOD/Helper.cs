#region Using

using System;

#endregion

namespace TerrainDemo.CDLOD
{
    public static class Helper
    {
        public static bool IsPowOf2(int value)
        {
            if (value < 1) return false;
            return (value & (value - 1)) == 0;
        }
    }
}
