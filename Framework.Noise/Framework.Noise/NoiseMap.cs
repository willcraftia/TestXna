#region Using

using System;

#endregion

namespace Willcraftia.Framework.Noise
{
    /// <summary>
    /// The class manages 2-dimensional noise values.
    /// </summary>
    public sealed class NoiseMap
    {
        public const int DefaultWidth = 256 + 1;

        public const int DefaultHeight = 256 + 1;

        NoiseDelegate noise;

        int width = DefaultWidth;

        int height = DefaultHeight;

        Bounds bounds = Bounds.One;

        float[] values;

        public NoiseDelegate Noise
        {
            get { return noise; }
            set { noise = value; }
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
            get { return values; }
        }

        public float this[int x, int y]
        {
            get { return values[x + y * width]; }
            set { values[x + y * width] = value; }
        }

        public Bounds Bounds
        {
            get { return bounds; }
            set { bounds = value; }
        }

        /// <summary>
        /// Builds 2-dimensional noise values.
        /// </summary>
        public void Build()
        {
            var length = width * height;
            if (values == null || values.Length != length)
                values = new float[length];

            var deltaX = bounds.Width / (float) width;
            var deltaY = bounds.Height / (float) height;

            float y = bounds.Y;
            for (int i = 0; i < height; i++)
            {
                float x = bounds.X;
                for (int j = 0; j < width; j++)
                {
                    var index = j + i * width;
                    values[index] = Noise(x, 0, y);
                    x += deltaX;
                }
                y += deltaY;
            }
        }
    }
}
