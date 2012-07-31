#region Using

using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework
{
    public class Map<T> : IMap<T>
    {
        public readonly T[] Values;

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

        public T this[int x, int y]
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

            Values = new T[width * height];
        }

        public void Clear()
        {
            Array.Clear(Values, 0, Values.Length);
        }
    }
}
