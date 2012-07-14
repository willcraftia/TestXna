#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace TerrainDemo.CDLOD
{
    public interface IVisibleRanges
    {
        int Count { get; }

        float this[int level] { get; }

        // Muse invoke this method after constructing or setting properties.
        void Initialize();
    }
}
