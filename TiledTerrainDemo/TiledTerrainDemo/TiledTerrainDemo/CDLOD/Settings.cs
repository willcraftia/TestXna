#region Using

using System;

#endregion

namespace TiledTerrainDemo.CDLOD
{
    public struct Settings
    {
        public const int MaxLevelCount = 15;

        public const int DefaultLeafNodeSize = 8;

        public const int DefaultLevelCount = 7;

        public const float DefaultPatchScale = 2;

        public const float DefaultHeightScale = 255;

        public int LeafNodeSize;

        public int LevelCount;

        public float PatchScale;

        public float HeightScale;

        public static Settings Default
        {
            get
            {
                return new Settings
                {
                    LeafNodeSize = DefaultLeafNodeSize,
                    LevelCount = DefaultLevelCount,
                    PatchScale = DefaultPatchScale,
                    HeightScale = DefaultHeightScale
                };
            }
        }
    }
}
