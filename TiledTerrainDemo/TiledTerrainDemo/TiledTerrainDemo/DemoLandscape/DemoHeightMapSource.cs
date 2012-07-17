#region Using

using System;
using Microsoft.Xna.Framework;
using TiledTerrainDemo.CDLOD;
using TiledTerrainDemo.Noise;

#endregion

namespace TiledTerrainDemo.DemoLandscape
{
    public sealed class DemoHeightMapSource : IHeightMapSource
    {
        public int Width { get; set; }

        public int Height { get; set; }

        public NoiseMap NoiseMap { get; set; }

        public float GetHeight(int x, int y)
        {
            return NoiseMap.Values[x + y * Width];
        }

        public void GetAreaMinMaxHeight(int x, int y, int sizeX, int sizeY, out float minHeight, out float maxHeight)
        {
            minHeight = float.MaxValue;
            maxHeight = float.MinValue;
            for (int ry = 0; ry < sizeY; ry++)
            {
                for (int rx = 0; rx < sizeX; rx++)
                {
                    var h = GetHeight(x + rx, y + ry);
                    minHeight = MathHelper.Min(minHeight, h);
                    maxHeight = MathHelper.Max(maxHeight, h);
                }
            }
        }
    }
}
