#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework
{
    public interface IMap
    {
        int Width { get; }

        int Height { get; }

        float this[int x, int y] { get; set; }
    }
}
