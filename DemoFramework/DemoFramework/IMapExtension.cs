#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework
{
    public static class IMapExtension
    {
        public static float Min(this IMap map)
        {
            return Min(map, 0, 0, map.Width, map.Height);
        }

        public static float Min(this IMap map, int startX, int startY, int sizeX, int sizeY)
        {
            float min = float.MaxValue;

            for (int y = 0; y < sizeY; y++)
                for (int x = 0; x < sizeX; x++)
                    min = Math.Min(min, map[x + startX, y + startY]);

            return min;
        }

        public static float Max(this IMap map)
        {
            return Max(map, 0, 0, map.Width, map.Height);
        }

        public static float Max(this IMap map, int startX, int startY, int sizeX, int sizeY)
        {
            float max = float.MinValue;

            for (int y = 0; y < sizeY; y++)
                for (int x = 0; x < sizeX; x++)
                    max = Math.Max(max, map[x + startX, y + startY]);

            return max;
        }

        public static void MinMax(this IMap map, out float min, out float max)
        {
            MinMax(map, 0, 0, map.Width, map.Height, out min, out max);
        }

        public static void MinMax(this IMap map, int startX, int startY, int sizeX, int sizeY, out float min, out float max)
        {
            min = float.MaxValue;
            max = float.MinValue;

            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    var v = map[x + startX, y + startY];
                    min = Math.Min(min, v);
                    max = Math.Max(max, v);
                }
            }
        }

        public static IMap Fill(this IMap map, float value)
        {
            for (int y = 0; y < map.Height; y++)
                for (int x = 0; x < map.Width; x++)
                    map[x, y] = value;

            return map;
        }

        public static IMap Add(this IMap map, IMap other)
        {
            for (int y = 0; y < map.Height; y++)
                for (int x = 0; x < map.Width; x++)
                    map[x, y] += other[x, y];

            return map;
        }

        public static IMap Subtract(this IMap map, IMap other)
        {
            for (int y = 0; y < map.Height; y++)
                for (int x = 0; x < map.Width; x++)
                    map[x, y] -= other[x, y];

            return map;
        }

        public static IMap Multiply(this IMap map, IMap other)
        {
            for (int y = 0; y < map.Height; y++)
                for (int x = 0; x < map.Width; x++)
                    map[x, y] *= other[x, y];

            return map;
        }

        public static IMap Divide(this IMap map, IMap other)
        {
            for (int y = 0; y < map.Height; y++)
                for (int x = 0; x < map.Width; x++)
                    map[x, y] /= other[x, y];

            return map;
        }

        public static IMap Multiply(this IMap map, float factor)
        {
            if (factor == 1)
                return map;

            for (int y = 0; y < map.Height; y++)
                for (int x = 0; x < map.Width; x++)
                    map[x, y] *= factor;

            return map;
        }

        public static IMap Normalize(this IMap map)
        {
            float min;
            float max;
            MinMax(map, out min, out max);

            float length = max - min;
            if (length == 0 || length == 1)
                return map;

            float factor = 1 / length;
            return Multiply(map, factor);
        }

        public static IMap NormalizeSymmetric(this IMap map)
        {
            float min;
            float max;
            MinMax(map, out min, out max);

            if (1 - max < min)
                min = 1 - max;
            if (max < 1 - min)
                max = 1 - min;

            float length = max - min;
            if (length == 0)
                return map;

            float factor = 1 / length;
            for (int y = 0; y < map.Height; y++)
                for (int x = 0; x < map.Width; x++)
                    map[x, y] = (map[x, y] - min) * factor;

            return map;
        }

        public static IMap Clamp(this IMap map)
        {
            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    var v = map[x, y];
                    if (v < 0)
                    {
                        map[x, y] = 0;
                    }
                    else if (1 < v)
                    {
                        map[x, y] = 1;
                    }
                }
            }

            return map;
        }
    }
}
