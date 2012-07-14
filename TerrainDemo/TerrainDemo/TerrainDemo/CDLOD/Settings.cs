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

        public const float DefaultVisibilityDistance = 20000;

        public const float DefaultDetailBalance = 2;

        public const float DefaultPatchScale = 2;

        public const float DefaultHeightScale = 255;

        public int LeafNodeSize;

        public int LevelCount;

        public float VisibilityDistance;

        public float DetailBalance;

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
                    VisibilityDistance = DefaultVisibilityDistance,
                    DetailBalance = DefaultDetailBalance,
                    PatchScale = DefaultPatchScale,
                    HeightScale = DefaultHeightScale
                };
            }
        }
    }
}
