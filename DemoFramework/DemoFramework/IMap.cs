#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework
{
    public interface IMap<T>
    {
        int Width { get; }

        int Height { get; }

        T this[int x, int y] { get; set; }
    }
}
