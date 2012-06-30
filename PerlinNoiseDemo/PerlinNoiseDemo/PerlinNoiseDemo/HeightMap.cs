#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace PerlinNoiseDemo
{
    /// <summary>
    /// Height map を管理するクラスです。
    /// </summary>
    public sealed class HeightMap
    {
        public delegate float DelegateGetValue2(float x, float y);

        const int defaultSize = 256 + 1;

        DelegateGetValue2 getValue2;

        int size;

        float inverseSize;

        int length;

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
            set
            {
                if (size == value) return;

                size = value;
                inverseSize = 1 / (float) size;
                length = size * size;
            }
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
        public HeightMap()
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
            if (heights == null || heights.Length != length)
                heights = new float[length];

            var deltaX = boundWidth / (float) size;
            var deltaY = boundHeight / (float) size;

            //for (int i = 0; i < size; i++)
            //{
            //    for (int j = 0; j < size; j++)
            //    {
            //        var index = i + j * size;
            //        var x = i * inverseSize;
            //        var y = j * inverseSize;
            //        heights[index] = GetValue2(x, y);
            //    }
            //}

            float y = boundY;
            for (int i = 0; i < size; i++)
            {
                float x = boundX;
                for (int j = 0; j < size; j++)
                {
                    var index = i + j * size;
                    heights[index] = GetValue2(x, y);
                    x += deltaX;
                }
                y += deltaY;
            }
        }
    }
}
