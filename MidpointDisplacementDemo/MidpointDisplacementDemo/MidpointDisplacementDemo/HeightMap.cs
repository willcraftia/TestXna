#region Using

using System;
using Willcraftia.Xna.Framework.Terrain;

#endregion

namespace MidpointDisplacementDemo
{
    public sealed class HeightMap : IWritableHeightMap
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

        public float this[int x, int y]
        {
            get { return values[x + y * width]; }
            set { values[x + y * width] = value; }
        }

        public float[] Values
        {
            get { return values; }
        }

        public HeightMap(int width, int height)
        {
            this.width = width;
            this.height = height;

            values = new float[width * height];
        }
    }
}
