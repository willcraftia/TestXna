#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

#endregion

namespace PerlinNoiseDemo
{
    public sealed class GradientColorCollection
    {
        struct GradientColor : IComparable<GradientColor>
        {
            public float Position;
            
            public Vector4 Color;

            public int CompareTo(GradientColor other)
            {
                return Position.CompareTo(other.Position);
            }
        }

        List<GradientColor> gradientColors = new List<GradientColor>();

        public void Add(float position, int r, int g, int b)
        {
            Add(position, r, g, b, 255);
        }

        public void Add(float position, int r, int g, int b, int a)
        {
            Add(position, new Color(r, g, b, a));
        }

        public void Add(float position, float r, float g, float b)
        {
            Add(position, r, g, b, 1);
        }

        public void Add(float position, float r, float g, float b, float a)
        {
            Add(position, new Vector4(r, g, b, a));
        }

        public void Add(float position, Color color)
        {
            Add(position, color.ToVector4());
        }

        public void Add(float position, Vector4 color)
        {
            var gradientColor = new GradientColor
            {
                Position = position,
                Color = color
            };
            gradientColors.Add(gradientColor);
            gradientColors.Sort();
        }

        public void Get(float position, out Color result)
        {
            Vector4 v;
            Get(position, out v);
            result = new Color(v.X, v.Y, v.Z, v.W);
        }

        public void Get(float position, out Vector4 result)
        {
            int index = -1;
            for (index = 0; index < gradientColors.Count; index++)
            {
                if (position < gradientColors[index].Position) break;
            }

            if (index < 0)
            {
                result = Vector4.Zero;
                return;
            }

            var index0 = Clamp(index - 1, 0, gradientColors.Count - 1);
            var index1 = Clamp(index, 0, gradientColors.Count - 1);

            if (index0 == index1)
            {
                result = gradientColors[index1].Color;
                return;
            }

            var p0 = gradientColors[index0].Position;
            var p1 = gradientColors[index1].Position;
            var amount = (position - p0) / (p1 - p0);

            var c0 = gradientColors[index0].Color;
            var c1 = gradientColors[index1].Color;

            var x = MathHelper.Lerp(c0.X, c1.X, amount);
            var y = MathHelper.Lerp(c0.Y, c1.Y, amount);
            var z = MathHelper.Lerp(c0.Z, c1.Z, amount);
            var w = MathHelper.Lerp(c0.W, c1.W, amount);

            result = new Vector4(x, y, z, w);
        }

        int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (max < value) return max;
            return value;
        }
    }
}
