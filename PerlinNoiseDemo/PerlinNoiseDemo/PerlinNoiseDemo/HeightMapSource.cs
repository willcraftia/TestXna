#region Using

using System;
using Microsoft.Xna.Framework;
using Willcraftia.Framework.Noise;

#endregion

namespace PerlinNoiseDemo
{
    /// <summary>
    /// The class manages a height map.
    /// </summary>
    public sealed class HeightMapSource : INoiseMap
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

        public HeightMapSource(int width, int height)
        {
            this.width = width;
            this.height = height;

            values = new float[width * height];
        }

        public void Fill(float value)
        {
            for (int i = 0; i < values.Length; i++)
                values[i] = value;
        }

        public void Clear()
        {
            if (values.Length == 0)
                return;

            Array.Clear(values, 0, values.Length);
        }

        public void ForEach(Action<float> action)
        {
            for (int i = 0; i < values.Length; i++)
                action(values[i]);
        }

        public float Max()
        {
            float result = float.MinValue;

            for (int i = 0; i < values.Length; i++)
                result = Math.Max(result, values[i]);

            return result;
        }

        public float Min()
        {
            float result = float.MaxValue;

            for (int i = 0; i < values.Length; i++)
                result = Math.Min(result, values[i]);

            return result;
        }
    }
}
