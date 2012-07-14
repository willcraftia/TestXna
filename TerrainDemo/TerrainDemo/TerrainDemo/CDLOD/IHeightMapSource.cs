#region Using

using System;

#endregion

namespace TerrainDemo.CDLOD
{
    public interface IHeightMapSource
    {
        int Width { get; }

        int Height { get; }

        // A value is [-1, 1].
        float GetHeight(int x, int y);

        void GetAreaMinMaxHeight(int x, int y, int sizeX, int sizeY, out float minHeight, out float maxHeight);
    }
}
