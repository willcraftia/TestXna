#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace TiledTerrainDemo.CDLOD
{
    public struct CDLODSettings
    {
        public const int MaxLevelCount = 15;

        public const int DefaultLeafNodeSize = 8;

        public const int DefaultLevelCount = 7;

        public const int DefaultPatchResolution = 2;

        public const float DefaultPatchScale = 2;

        public const float DefaultHeightScale = 255;

        public const int DefaultHeightMapWidth = 512 + 1;

        public const int DefaultHeightMapHeight = 512 + 1;

        public const int DefaultHeightMapOverlapSize = 1;

        public int LeafNodeSize;

        public int LevelCount;

        public int PatchResolution;

        public float PatchScale;

        public float HeightScale;

        public int HeightMapWidth;

        public int HeightMapHeight;

        public int HeightMapOverlapSize;

        public static CDLODSettings Default
        {
            get
            {
                return new CDLODSettings
                {
                    LeafNodeSize = DefaultLeafNodeSize,
                    LevelCount = DefaultLevelCount,
                    PatchResolution = DefaultPatchResolution,
                    PatchScale = DefaultPatchScale,
                    HeightScale = DefaultHeightScale,
                    HeightMapWidth = DefaultHeightMapWidth,
                    HeightMapHeight = DefaultHeightMapHeight,
                    HeightMapOverlapSize = DefaultHeightMapOverlapSize
                };
            }
        }

        public Vector3 TerrainScale
        {
            get
            {
                return new Vector3
                {
                    X = (HeightMapWidth - 1) * PatchScale,
                    Y = HeightScale,
                    Z = (HeightMapHeight - 1) * PatchScale
                };
            }
        }

        public int TopNodeSize
        {
            get
            {
                var topNodeSize = LeafNodeSize;
                for (int i = 1; i < LevelCount; i++) topNodeSize *= 2;
                return topNodeSize;
            }
        }

        public int PatchGridSize
        {
            get
            {
                return LeafNodeSize * PatchResolution;
            }
        }
    }
}
