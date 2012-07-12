#region Using

using System;

#endregion

namespace TerrainDemo.CDLOD
{
    public struct CreateDescription
    {
        public int LeafNodeSize;

        public int LevelCount;

        public IHeightMapSource HeightMap;
    }
}
