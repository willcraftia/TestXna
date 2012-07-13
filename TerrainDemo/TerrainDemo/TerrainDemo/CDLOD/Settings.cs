#region Using

using System;

#endregion

namespace TerrainDemo.CDLOD
{
    public struct Settings
    {
        public const int MaxLevelCount = 15;

        public const int DefaultLeafNodeSize = 8;

        public const int DefaultLevelCount = 7;

        public int LeafNodeSize;

        public int LevelCount;

        public static Settings Default
        {
            get
            {
                return new Settings
                {
                    LeafNodeSize = DefaultLeafNodeSize,
                    LevelCount = DefaultLevelCount
                };
            }
        }
    }
}
