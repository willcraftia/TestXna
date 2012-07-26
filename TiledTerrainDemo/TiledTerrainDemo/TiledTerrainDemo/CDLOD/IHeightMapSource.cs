#region Using

using System;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace TiledTerrainDemo.CDLOD
{
    public interface IHeightMapSource
    {
        int Width { get; }

        int Height { get; }

        float this[int x, int y] { get; }

        void GetAreaMinMaxHeight(int x, int y, int sizeX, int sizeY, out float minHeight, out float maxHeight);
    }
}
