#region Using

using System;

#endregion

namespace Willcraftia.Framework.Terrain
{
    public interface IHeightMap
    {
        int Width { get; }

        int Height { get; }

        float this[int x, int y] { get; }
    }
}
