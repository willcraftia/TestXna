#region Using

using System;
using Microsoft.Xna.Framework;
using Willcraftia.Framework.Noise;
using Willcraftia.Framework.Terrain;

#endregion

namespace PerlinNoiseDemo
{
    /// <summary>
    /// The class manages a height map.
    /// </summary>
    public sealed class HeightMap : INoiseMap, IHeightMap
    {
        int width;

        int height;

        float[] values;

        public int Width
        {
            get { return width; }
        }

        public int Height
        {
            get { return height; }
        }

        public float[] Values
        {
            get { return values; }
        }

        public float this[int x, int y]
        {
            get { return values[x + y * width]; }
            set { values[x + y * width] = value; }
        }

        public HeightMap(int width, int height)
        {
            this.width = width;
            this.height = height;

            values = new float[width * height];
        }
    }
}
