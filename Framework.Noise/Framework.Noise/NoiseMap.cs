#region Using

using System;
using Microsoft.Xna.Framework;

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

        bool seamlessEnabled;

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

        public Bounds Bounds
        {
            get { return bounds; }
            set { bounds = value; }
        }

        public bool SeamlessEnabled
        {
            get { return seamlessEnabled; }
            set { seamlessEnabled = value; }
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

                    if (!seamlessEnabled)
                    {
                        values[index] = Noise(x, 0, y);
                    }
                    else
                    {
                        float sw = Noise(x, 0, y);
                        float se = Noise(x + bounds.Width, 0, y);
                        float nw = Noise(x, 0, y + bounds.Height);
                        float ne = Noise(x + bounds.Width, 0, y + bounds.Height);
                        float xa = 1 - ((x - bounds.X) / bounds.Width);
                        float ya = 1 - ((y - bounds.Y) / bounds.Height);
                        float y0 = MathHelper.Lerp(sw, se, xa);
                        float y1 = MathHelper.Lerp(nw, ne, xa);
                        values[index] = MathHelper.Lerp(y0, y1, ya);
                    }

                    x += deltaX;
                }
                y += deltaY;
            }
        }
    }
}
