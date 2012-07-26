#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Terrain.CDLOD
{
    public static class CDLODHelper
    {
        public static bool IsPowOf2(int value)
        {
            if (value < 1) return false;
            return (value & (value - 1)) == 0;
        }
    }
}
