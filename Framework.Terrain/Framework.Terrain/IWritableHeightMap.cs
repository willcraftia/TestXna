#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Terrain
{
    public interface IWritableHeightMap : IHeightMap
    {
        new float this[int x, int y] { get; set; }
    }
}
