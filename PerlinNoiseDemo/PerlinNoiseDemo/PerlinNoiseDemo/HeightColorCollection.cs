#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

#endregion

namespace PerlinNoiseDemo
{
    public sealed class HeightColorCollection
    {
        struct HeightColor : IComparable<HeightColor>
        {
            public float Height;
            
            public Vector4 Color;

            public int CompareTo(HeightColor other)
            {
                return Height.CompareTo(other.Height);
            }
        }

        List<HeightColor> heightColors = new List<HeightColor>();

        public void AddColor(float height, Color color)
        {
            AddColor(height, color.ToVector4());
        }

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

        int Clamp(int value, int lower, int upper)
        {
            if (value < lower) return lower;
            if (upper < value) return upper;
            return value;
        }

        float Blend(float value0, float value1, float amount)
        {
            return value1 * amount + value0 * (1.0f - amount);
        }
    }
}
