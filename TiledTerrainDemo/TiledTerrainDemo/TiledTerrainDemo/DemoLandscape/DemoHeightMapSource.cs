﻿#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TiledTerrainDemo.CDLOD;
using TiledTerrainDemo.Noise;

#endregion

namespace TiledTerrainDemo.DemoLandscape
{
    public sealed class DemoHeightMapSource : IHeightMapSource
    {
        public int Width
        {
            get
            {
                return (TiledNoiseMap != null) ? TiledNoiseMap.Width : 0;
            }
        }

        public int Height
        {
            get
            {
                return (TiledNoiseMap != null) ? TiledNoiseMap.Height : 0;
            }
        }

        public float this[int x, int y]
        {
            get { return TiledNoiseMap[x, y]; }
        }

        public TiledNoiseMap TiledNoiseMap { get; set; }

        public void GetAreaMinMaxHeight(int x, int y, int sizeX, int sizeY, out float minHeight, out float maxHeight)
        {
            minHeight = float.MaxValue;
            maxHeight = float.MinValue;
            for (int ry = 0; ry < sizeY; ry++)
            {
                for (int rx = 0; rx < sizeX; rx++)
                {
                    var h = this[x + rx, y + ry];
                    minHeight = MathHelper.Min(minHeight, h);
                    maxHeight = MathHelper.Max(maxHeight, h);
                }
            }
        }
    }
}
