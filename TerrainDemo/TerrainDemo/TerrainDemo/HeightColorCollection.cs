#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

#endregion

namespace TerrainDemo
{
    /// <summary>
    /// 高度に対応する色のコレクションを管理するクラスです。
    /// </summary>
    public sealed class HeightColorCollection
    {
        /// <summary>
        /// 高度に対応する色を管理する構造体です。
        /// </summary>
        struct HeightColor : IComparable<HeightColor>
        {
            /// <summary>
            /// 高度。
            /// </summary>
            public float Height;

            /// <summary>
            /// 色。
            /// </summary>
            public Vector4 Color;

            /// <summary>
            /// Height プロパティで比較します。
            /// </summary>
            /// <param name="other"></param>
            /// <returns></returns>
            public int CompareTo(HeightColor other)
            {
                return Height.CompareTo(other.Height);
            }
        }

        List<HeightColor> heightColors = new List<HeightColor>();

        /// <summary>
        /// 高度と色の対応を追加します。
        /// </summary>
        /// <param name="height"></param>
        /// <param name="color"></param>
        public void AddColor(float height, Color color)
        {
            AddColor(height, color.ToVector4());
        }

        /// <summary>
        /// 高度と色の対応を追加します。
        /// </summary>
        /// <param name="height"></param>
        /// <param name="color"></param>
        public void AddColor(float height, Vector4 color)
        {
            var heightColor = new HeightColor
            {
                Height = height,
                Color = color
            };
            heightColors.Add(heightColor);
            heightColors.Sort();
        }

        /// <summary>
        /// 高度に対応する色を取得します。
        /// </summary>
        /// <param name="height"></param>
        /// <param name="result"></param>
        public void GetColor(float height, out Vector4 result)
        {
            int index = -1;
            for (index = 0; index < heightColors.Count; index++)
            {
                if (height < heightColors[index].Height) break;
            }

            if (index < 0)
            {
                result = Vector4.Zero;
                return;
            }

            var index0 = Clamp(index - 1, 0, heightColors.Count - 1);
            var index1 = Clamp(index, 0, heightColors.Count - 1);

            if (index0 == index1)
            {
                result = heightColors[index1].Color;
                return;
            }

            var lowerHeight = heightColors[index0].Height;
            var upperHeight = heightColors[index1].Height;
            var amount = (height - lowerHeight) / (upperHeight - lowerHeight);

            var c0 = heightColors[index0].Color;
            var c1 = heightColors[index1].Color;

            var x = Blend(c0.X, c1.X, amount);
            var y = Blend(c0.Y, c1.Y, amount);
            var z = Blend(c0.Z, c1.Z, amount);
            var w = Blend(c0.W, c1.W, amount);

            result = new Vector4(x, y, z, w);
        }

        /// <summary>
        /// Clamp。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="lower"></param>
        /// <param name="upper"></param>
        /// <returns></returns>
        int Clamp(int value, int lower, int upper)
        {
            if (value < lower) return lower;
            if (upper < value) return upper;
            return value;
        }

        /// <summary>
        /// Blend。
        /// </summary>
        /// <param name="value0"></param>
        /// <param name="value1"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        float Blend(float value0, float value1, float amount)
        {
            return value1 * amount + value0 * (1.0f - amount);
        }
    }
}
