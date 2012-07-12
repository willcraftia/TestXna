#region Using

using System;
using Microsoft.Xna.Framework;
using TerrainDemo.Noise;

#endregion

namespace TerrainDemo.CDLOD
{
    public sealed class DefaultHeightMapSource : IHeightMapSource
    {
        float[,] heights;

        public int Size { get; private set; }

        public DefaultHeightMapSource(NoiseMap noiseMap)
        {
            Size = noiseMap.Size;

            heights = new float[Size, Size];

            // 単純に [-1, 1] で clamp して設定。
            // ほとんどの height は [-1, 1] に収まるため。
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    var h = noiseMap.Heights[x + y * Size];
                    heights[x, y] = MathHelper.Clamp(h, -1, 1);
                }
            }
        }

        public float GetHeight(int x, int y)
        {
            return heights[x, y];
        }

        public void GetAreaMinMaxHeight(int x, int y, int size, out float minHeight, out float maxHeight)
        {
            minHeight = float.MaxValue;
            maxHeight = float.MinValue;
            for (int ry = 0; ry < size; ry++)
            {
                for (int rx = 0; rx < size; rx++)
                {
                    var h = heights[x + rx, y + ry];
                    minHeight = MathHelper.Min(minHeight, h);
                    maxHeight = MathHelper.Max(maxHeight, h);
                }
            }
        }
    }
}
