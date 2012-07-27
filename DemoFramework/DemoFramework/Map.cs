#region Using

using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework
{
    public sealed class Map : IMap
    {
        public readonly float[] Values;

        int width;

        int height;

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
            get { return Values[x + y * width]; }
            set { Values[x + y * width] = value; }
        }

        public Map(int width, int height)
        {
            if (width < 0) throw new ArgumentOutOfRangeException("width");
            if (height < 0) throw new ArgumentOutOfRangeException("height");

            this.width = width;
            this.height = height;

            Values = new float[width * height];
        }
    }
}
