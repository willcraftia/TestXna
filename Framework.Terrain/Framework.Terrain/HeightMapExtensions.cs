#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Terrain
{
    public static class HeightMapExtensions
    {
        public static float Max(this IHeightMap heightMap)
        {
            float result = float.MinValue;

            for (int y = 0; y < heightMap.Height; y++)
                for (int x = 0; x < heightMap.Width; x++)
                    result = Math.Max(result, heightMap[x, y]);

            return result;
        }

        public static float Min(this IHeightMap heightMap)
        {
            float result = float.MaxValue;

            for (int y = 0; y < heightMap.Height; y++)
                for (int x = 0; x < heightMap.Width; x++)
                    result = Math.Min(result, heightMap[x, y]);

            return result;
        }

        public static void ForEach(this IWritableHeightMap heightMap, Action<float> action)
        {
            for (int y = 0; y < heightMap.Height; y++)
                for (int x = 0; x < heightMap.Width; x++)
                    action(heightMap[x, y]);
        }

        public static void Fill(this IWritableHeightMap heightMap, float value)
        {
            for (int y = 0; y < heightMap.Height; y++)
                for (int x = 0; x < heightMap.Width; x++)
                    heightMap[x, y] = value;
        }

        public static void Clear(this IWritableHeightMap heightMap)
        {
            Fill(heightMap, 0);
        }
    }
}
