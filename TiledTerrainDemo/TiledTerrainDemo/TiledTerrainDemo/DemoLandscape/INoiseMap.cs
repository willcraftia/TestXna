#region Using

using System;

#endregion

namespace TiledTerrainDemo.DemoLandscape
{
    public interface INoiseMap
    {
        int Width { get; }

        int Height { get; }

        float this[int x, int y] { get; set; }
    }
}
