#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace TiledTerrainDemo.DemoLandscape
{
    public struct HeightColor : IComparable<HeightColor>
    {
        public float Position;

        public Vector4 Color;

        public int CompareTo(HeightColor other)
        {
            return Position.CompareTo(other.Position);
        }
    }
}
