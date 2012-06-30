#region Using

using System;

#endregion

namespace PerlinNoiseDemo
{
    public sealed class HeightMap
    {
        public delegate float DelegateGetValue2(float x, float y);

        const int defaultSize = 256 + 1;

        DelegateGetValue2 getValue2;

        int size;

        float inverseSize;

        int length;

        float[] heights;

        public DelegateGetValue2 GetValue2
        {
            get { return getValue2; }
            set { getValue2 = value; }
        }

        public int Size
        {
            get { return size; }
            set
            {
                if (size == value) return;

                size = value;
                inverseSize = 1 / (float) size;
                length = size * size;
            }
        }

        public float[] Heights
        {
            get { return heights; }
        }

        public HeightMap()
        {
            Size = defaultSize;
        }

        public void Build()
        {
            if (heights == null || heights.Length != length)
                heights = new float[length];

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    var index = i + j * size;
                    var x = i * inverseSize;
                    var y = j * inverseSize;
                    heights[index] = GetValue2(x, y);
                }
            }
        }
    }
}
