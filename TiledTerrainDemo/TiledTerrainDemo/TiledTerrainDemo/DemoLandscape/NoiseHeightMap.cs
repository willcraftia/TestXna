#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Noise;
using Willcraftia.Xna.Framework.Terrain;
using Willcraftia.Xna.Framework.Terrain.CDLOD;

#endregion

namespace TiledTerrainDemo.DemoLandscape
{
    public sealed class NoiseHeightMap : IMap<float>, IDisposable
    {
        public readonly float[] Values;

        NoiseMapBuilder builder = new NoiseMapBuilder();

        int width;

        int height;

        Bounds bounds;

        FlipTexture2D flipTexture;

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

        public SampleSourceDelegate NoiseSource
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
                // Clamp.
                if (x < 0) return 0;
                if (width <= x) return 1;
                if (y < 0) return 0;
                if (height <= y) return 1;

                return Values[x + y * width];
            }
            set
            {
                // Clamp.
                if (x < 0 || width <= x ||
                    y < 0 || height <= y)
                    return;

                Values[x + y * width] = value;
            }
        }

        public Texture2D Texture
        {
            get
            {
                RefreshTexture();
                return flipTexture.Texture;
            }
        }

        public NoiseHeightMap(GraphicsDevice graphicsDevice, int width, int height)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");
            if (width < 0) throw new ArgumentOutOfRangeException("width");
            if (height < 0) throw new ArgumentOutOfRangeException("height");

            GraphicsDevice = graphicsDevice;
            this.width = width;
            this.height = height;

            Values = new float[width * height];
            builder.Destination = this;

            flipTexture = new FlipTexture2D(graphicsDevice, width, height, false, SurfaceFormat.Single);
        }

        public void Build()
        {
            var dx = bounds.Width / (float) width;
            var dy = bounds.Height / (float) height;

            builder.Bounds = new Bounds
            {
                X = bounds.X,
                Y = bounds.Y,
                Width = bounds.Width + dx,
                Height = bounds.Height + dy
            };

            builder.Build();
            Erosion.ErodeThermal(this, 0.05f, 10);

            textureDirty = true;
        }

        public void MergeLeftNeighbor(IMap<float> neighbor)
        {
            var rightEdge = width - 1;
            
            for (int y = 0; y < height; y++)
                MergeHeight(this, 0, y, neighbor, rightEdge, y);

            textureDirty = true;
        }

        public void MergeRightNeighbor(IMap<float> neighbor)
        {
            var rightEdge = width - 1;

            for (int y = 0; y < height; y++)
                MergeHeight(this, rightEdge, y, neighbor, 0, y);

            textureDirty = true;
        }

        public void MergeTopNeighbor(IMap<float> neighbor)
        {
            var bottomEdge = height - 1;

            for (int x = 0; x < width; x++)
                MergeHeight(this, x, 0, neighbor, x, bottomEdge);

            textureDirty = true;
        }

        public void MergeBottomNeighbor(IMap<float> neighbor)
        {
            var bottomEdge = height - 1;

            for (int x = 0; x < width; x++)
                MergeHeight(this, x, bottomEdge, neighbor, x, 0);

            textureDirty = true;
        }

        void MergeHeight(IMap<float> map0, int x0, int y0, IMap<float> map1, int x1, int y1)
        {
            var h0 = map0[x0, y0];
            var h1 = map1[x1, y1];
            var h = (h0 + h1) * 0.5f;
            map0[x0, y0] = h;
            map1[x1, y1] = h;
        }

        void RefreshTexture()
        {
            if (!textureDirty) return;

            flipTexture.Flip();
            flipTexture.Texture.SetData(Values);

            textureDirty = false;
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed;

        ~NoiseHeightMap()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
                flipTexture.Dispose();

            disposed = true;
        }

        #endregion
    }
}
