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

        public Texture2D Texture { get; private set; }

        public GraphicsDevice GraphicsDevice { get; private set; }

        public TiledNoiseMap TiledNoiseMap { get; set; }

        public DemoHeightMapSource(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");

            GraphicsDevice = graphicsDevice;
        }

        public void Build()
        {
            if (TiledNoiseMap == null)
                throw new InvalidOperationException("TiledNoiseMap is null.");

            int w = TiledNoiseMap.ActualWidth;
            int h = TiledNoiseMap.ActualHeight;

            if (Texture == null || Texture.Width != w || Texture.Height != h)
            {
                if (Texture != null) Texture.Dispose();

                Texture = new Texture2D(GraphicsDevice, w, h, false, SurfaceFormat.Single);
            }

            Texture.SetData(TiledNoiseMap.Values);
        }

        public float GetHeight(int x, int y)
        {
            return TiledNoiseMap[x, y];
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
            {
                if (Texture != null) Texture.Dispose();
            }

            disposed = true;
        }

        #endregion
    }
}
