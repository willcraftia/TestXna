#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TiledTerrainDemo.CDLOD;
using TiledTerrainDemo.Noise;

#endregion

namespace TiledTerrainDemo.DemoLandscape
{
    public sealed class DemoHeightMapSource : IHeightMapSource, IDisposable
    {
        class NoiseMapImpl : INoiseMap
        {
            public float[] Values;

            public int Width { get; set; }

            public int Height { get; set; }

            public float this[int x, int y]
            {
                get { return Values[x + y * Width]; }
                set { Values[x + y * Width] = value; }
            }

            public NoiseMapImpl(int width, int height)
            {
                Width = width;
                Height = height;

                Values = new float[width * height];
            }
        }

        NoiseMapBuilder builder = new NoiseMapBuilder();

        NoiseMapImpl noiseMap;

        int width;

        int height;

        int overlapSize;

        Bounds bounds;

        Texture2D texture;

        bool textureDirty;

        public GraphicsDevice GraphicsDevice { get; set; }

        public int Width
        {
            get { return width; }
        }

        public int Height
        {
            get { return height; }
        }

        public int ActualWidth
        {
            get { return noiseMap.Width; }
        }

        public int ActualHeight
        {
            get { return noiseMap.Height; }
        }

        public NoiseDelegate NoiseSource
        {
            get { return builder.Source; }
            set { builder.Source = value; }
        }

        public Bounds Bounds
        {
            get { return bounds; }
            set { bounds = value; }
        }

        public float this[int x, int y]
        {
            get
            {
                x += overlapSize;
                y += overlapSize;
                return noiseMap[x, y];
            }
        }

        public Texture2D Texture
        {
            get
            {
                RefreshTexture();
                return texture;
            }
        }

        public DemoHeightMapSource(GraphicsDevice graphicsDevice, Settings settings)
        {
            GraphicsDevice = graphicsDevice;
            width = settings.HeightMapWidth;
            height = settings.HeightMapHeight;
            overlapSize = settings.HeightMapOverlapSize;

            noiseMap = new NoiseMapImpl(width + 2 * overlapSize, height + 2 * overlapSize);
            builder.Destination = noiseMap;

            texture = new Texture2D(graphicsDevice, noiseMap.Width, noiseMap.Height, false, SurfaceFormat.Single);
        }

        public void Build()
        {
            var dx = bounds.Width / (float) width;
            var dy = bounds.Height / (float) height;

            builder.Bounds = new Bounds
            {
                X = bounds.X - dx * overlapSize,
                Y = bounds.Y - dy * overlapSize,
                Width = bounds.Width + dx + (2 * dx * overlapSize),
                Height = bounds.Height + dy + (2 * dy * overlapSize)
            };

            builder.Build();

            textureDirty = true;
        }

        public void RefreshTexture()
        {
            if (!textureDirty) return;

            texture.SetData(noiseMap.Values);

            textureDirty = false;
        }

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

        // todo: a temporary implementation
        public void Erode(float talus, int iterations)
        {
            for (int i = 0; i < iterations; i++)
            {
                for (int y = 1 + overlapSize; y < noiseMap.Height - 2 - 2 * overlapSize; y++)
                //for (int y = 1; y < noiseMap.Height - 2; y++)
                {
                    for (int x = 1 + overlapSize; x < noiseMap.Width - 2 - 2 * overlapSize; x++)
                    //for (int x = 1; x < noiseMap.Width - 2; x++)
                    {
                        var h = noiseMap[x, y];
                        var h1 = noiseMap[x, y + 1];
                        var h2 = noiseMap[x - 1, y];
                        var h3 = noiseMap[x + 1, y];
                        var h4 = noiseMap[x, y - 1];

                        var d1 = h - h1;
                        var d2 = h - h2;
                        var d3 = h - h3;
                        var d4 = h - h4;

                        var a = 0;
                        var b = 0;

                        float maxD = 0;
                        if (maxD < d1)
                        {
                            maxD = d1;
                            b = 1;
                        }
                        if (maxD < d2)
                        {
                            maxD = d2;
                            a = -1;
                            b = 0;
                        }
                        if (maxD < d3)
                        {
                            maxD = d3;
                            a = 1;
                            b = 0;
                        }
                        if (maxD < d4)
                        {
                            maxD = d4;
                            a = 0;
                            b = -1;
                        }

                        if (talus < maxD) continue;

                        maxD *= 0.5f;
                        noiseMap[x, y] -= maxD;
                        noiseMap[x + a, y + b] += maxD;
                    }
                }
            }
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed;

        ~DemoHeightMapSource()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
                texture.Dispose();

            disposed = true;
        }

        #endregion
    }
}
