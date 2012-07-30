#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Graphics;

#endregion

namespace TiledTerrainDemo.DemoLandscape
{
    public sealed class NormalMap : Map<Rgba64>, IDisposable
    {
        FlipTexture2D flipTexture;

        // TODO
        public bool textureDirty;

        public Texture2D Texture
        {
            get
            {
                RefreshTexture();
                return flipTexture.Texture;
            }
        }

        public NormalMap(GraphicsDevice graphicsDevice, int width, int height)
            : base(width, height)
        {
            flipTexture = new FlipTexture2D(graphicsDevice, width, height, false, SurfaceFormat.Rgba64);
        }

        public void Build(IMap<float> heightMap, float amplitude)
        {
            int w = Math.Min(Width, heightMap.Width);
            int h = Math.Min(Height, heightMap.Height);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    var h0 = heightMap[x, y - 1] * amplitude;
                    var h1 = heightMap[x, y + 1] * amplitude;
                    var h2 = heightMap[x - 1, y] * amplitude;
                    var h3 = heightMap[x + 1, y] * amplitude;

                    var d0 = new Vector3(0, 1, h1 - h0);
                    var d1 = new Vector3(1, 0, h3 - h2);
                    d0.Normalize();
                    d1.Normalize();

                    Vector3 normal;
                    Vector3.Cross(ref d0, ref d1, out normal);
                    normal.Normalize();

                    // [-1, 1] -> [0, 1]
                    normal.X = normal.X * 0.5f + 0.5f;
                    normal.Y = normal.Y * 0.5f + 0.5f;
                    normal.Z = normal.Z * 0.5f + 0.5f;

                    Values[x + y * Width] = new Rgba64(normal.X, normal.Y, normal.Z, 0);
                }
            }

            textureDirty = true;
        }

        public void RefreshTexture()
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

        ~NormalMap()
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
