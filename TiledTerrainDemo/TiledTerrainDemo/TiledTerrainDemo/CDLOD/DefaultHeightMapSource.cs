#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TiledTerrainDemo.Noise;

#endregion

namespace TiledTerrainDemo.CDLOD
{
    public sealed class DefaultHeightMapSource : IHeightMapSource
    {
        float[,] heights;

        public int Width { get; private set; }

        public int Height { get; private set; }

        public Texture2D Texture { get; private set; }

        public GraphicsDevice GraphicsDevice { get; private set; }

        public DefaultHeightMapSource(GraphicsDevice graphicsDevice, float[] sourceHeights, int width, int height)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");

            GraphicsDevice = graphicsDevice;
            Width = width;
            Height = height;

            heights = new float[Width, Height];

            // All values must be [-1, 1].
            // Almost noise values are [-1, 1], so simply clamp them.
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    var h = sourceHeights[x + y * Width];
                    heights[x, y] = MathHelper.Clamp(h, -1, 1);
                }
            }
        }

        public void Build()
        {
            Texture = new Texture2D(GraphicsDevice, Width, Height, false, SurfaceFormat.Single);

            var data = new float[Width * Height];
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    data[x + y * Width] = heights[x, y];

            Texture.SetData(data);
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
