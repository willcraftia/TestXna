#region Using

using System;

#endregion

namespace TerrainDemo.Noise
{
    /// <summary>
    /// Height map を管理するクラスです。
    /// </summary>
    public sealed class NoiseMap
    {
        public delegate float DelegateGetValue2(float x, float y);

        const int defaultSize = 256 + 1;

        DelegateGetValue2 getValue2;

        int size;

        float boundX;

        float boundY;

        float boundWidth;

        float boundHeight;

        float[] heights;

        /// <summary>
        /// 各 height 値を生成するためのメソッドを取得または設定します。
        /// </summary>
        public DelegateGetValue2 GetValue2
        {
            get { return getValue2; }
            set { getValue2 = value; }
        }

        /// <summary>
        /// サイズを取得または設定します。
        /// Heights プロパティの長さは Size * Size となります。
        /// </summary>
        public int Size
        {
            get { return size; }
            set { size = value; }
        }

        /// <summary>
        /// Height 値の配列を取得します。
        /// </summary>
        public float[] Heights
        {
            get { return heights; }
        }

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        public NoiseMap()
        {
            Size = defaultSize;
        }

        public void SetBounds(float x, float y, float width, float height)
        {
            boundX = x;
            boundY = y;
            boundWidth = width;
            boundHeight = height;
        }

        /// <summary>
        /// Height map を生成します。
        /// </summary>
        public void Build()
        {
            var length = size * size;
            if (heights == null || heights.Length != length)
                heights = new float[length];

            var inverseSize = 1 / (float) size;
            var deltaX = boundWidth * inverseSize;
            var deltaY = boundHeight * inverseSize;

            float y = boundY;
            for (int i = 0; i < size; i++)
            {
                float x = boundX;
                for (int j = 0; j < size; j++)
                {
                    var index = j + i * size;
                    heights[index] = GetValue2(x, y);
                    x += deltaX;
                }
                y += deltaY;
            }
        }
    }
}
