#region Using

using System;

#endregion

namespace TiledTerrainDemo.Noise
{
    /// <summary>
    /// The class manages 2-dimensional noise values as 1-dimensional array.
    /// </summary>
    public sealed class NoiseMap
    {
        public delegate float DelegateGetValue2(float x, float y);

        DelegateGetValue2 getValue2;

        int width = 256 + 1;

        int height = 256 + 1;

        float boundX;

        float boundY;

        float boundWidth;

        float boundHeight;

        float[] heights;

        /// <summary>
        /// Gets/sets a method generating 2-dimensional noise values.
        /// </summary>
        public DelegateGetValue2 GetValue2
        {
            get { return getValue2; }
            set { getValue2 = value; }
        }

        public int Width
        {
            get { return width; }
            set { width = value; }
        }

        public int Height
        {
            get { return height; }
            set { height = value; }
        }

        public float[] Values
        {
            get { return heights; }
        }

        public void SetBounds(float x, float y, float w, float h)
        {
            boundX = x;
            boundY = y;
            boundWidth = w;
            boundHeight = h;
        }

        /// <summary>
        /// Builds 2-dimensional noise values.
        /// </summary>
        public void Build()
        {
            var length = width * height;
            if (heights == null || heights.Length != length)
                heights = new float[length];

            var deltaX = boundWidth * (1 / (float) width);
            var deltaY = boundHeight * (1 / (float) height);

            float y = boundY;
            for (int i = 0; i < height; i++)
            {
                float x = boundX;
                for (int j = 0; j < width; j++)
                {
                    var index = j + i * width;
                    heights[index] = GetValue2(x, y);
                    x += deltaX;
                }
                y += deltaY;
            }
        }
    }
}
