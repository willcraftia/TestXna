#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework.Terrain.CDLOD
{
    public interface ICDLODVisibleRanges
    {
        int Count { get; }

        float this[int level] { get; }

        // Muse invoke this method after constructing or setting properties.
        void Initialize();
    }
}
