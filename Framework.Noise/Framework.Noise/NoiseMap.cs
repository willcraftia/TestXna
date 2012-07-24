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
        int width;

        int height;

        float[] values;

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

        public void Initialize()
        {
            var length = width * height;
            if (values == null || values.Length != length)
                values = new float[length];
        }

        public void Fill(float value)
        {
            Initialize();

            for (int i = 0; i < values.Length; i++)
                values[i] = value;
        }

        public void Clear()
        {
            if (values == null || width == 0 || height == 0)
                return;

            Initialize();

            Array.Clear(values, 0, values.Length);
        }

        public void ForEach(Action<float> action)
        {
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    action(values[x + y * width]);
        }
    }
}
