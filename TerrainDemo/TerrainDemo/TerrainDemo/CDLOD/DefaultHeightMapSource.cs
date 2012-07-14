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

        public int Width { get; private set; }

        public int Height { get; private set; }

        public DefaultHeightMapSource(NoiseMap noiseMap)
        {
            Width = noiseMap.Width;
            Height = noiseMap.Height;

            heights = new float[Width, Height];

            // All values must be [-1, 1].
            // Almost noise values are [-1, 1], so simply clamp them.
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    var h = noiseMap.Heights[x + y * Width];
                    heights[x, y] = MathHelper.Clamp(h, -1, 1);
                }
            }
        }

        public float GetHeight(int x, int y)
        {
            return heights[x, y];
        }

        public void GetAreaMinMaxHeight(int x, int y, int sizeX, int sizeY, out float minHeight, out float maxHeight)
        {
            minHeight = float.MaxValue;
            maxHeight = float.MinValue;
            for (int ry = 0; ry < sizeY; ry++)
            {
                for (int rx = 0; rx < sizeX; rx++)
                {
                    var h = heights[x + rx, y + ry];
                    minHeight = MathHelper.Min(minHeight, h);
                    maxHeight = MathHelper.Max(maxHeight, h);
                }
            }
        }
    }
}
