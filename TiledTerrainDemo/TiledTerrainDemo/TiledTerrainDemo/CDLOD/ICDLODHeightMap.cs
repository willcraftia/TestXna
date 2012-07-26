#region Using

using System;
using Willcraftia.Xna.Framework.Terrain;

#endregion

namespace TiledTerrainDemo.CDLOD
{
    public interface ICDLODHeightMap : IHeightMap
    {
        void GetAreaMinMaxHeight(int x, int y, int sizeX, int sizeY, out float minHeight, out float maxHeight);
    }
}
