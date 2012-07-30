#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework.Terrain.CDLOD
{
    public struct CDLODSettings
    {
        public const int MaxLevelCount = 15;

        public const int DefaultLeafNodeSize = 8;

        public const int DefaultLevelCount = 7;

        public const int DefaultPatchResolution = 2;

        public const float DefaultMapScale = 2;

        public const float DefaultHeightScale = 255;

        public const int DefaultHeightMapWidth = 512 + 1;

        public const int DefaultHeightMapHeight = 512 + 1;

        public const int DefaultHeightMapOverlapSize = 1;

        public int LeafNodeSize;

        public int LevelCount;

        public int PatchResolution;

        public float MapScale;

        public float HeightScale;

        public int HeightMapWidth;

        public int HeightMapHeight;

        public static CDLODSettings Default
        {
            get
            {
                return new CDLODSettings
                {
                    LeafNodeSize = DefaultLeafNodeSize,
                    LevelCount = DefaultLevelCount,
                    PatchResolution = DefaultPatchResolution,
                    MapScale = DefaultMapScale,
                    HeightScale = DefaultHeightScale,
                    HeightMapWidth = DefaultHeightMapWidth,
                    HeightMapHeight = DefaultHeightMapHeight
                };
            }
        }

        public Vector3 TerrainScale
        {
            get
            {
                return new Vector3
                {
                    X = (HeightMapWidth - 1) * MapScale,
                    Y = HeightScale,
                    Z = (HeightMapHeight - 1) * MapScale
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
