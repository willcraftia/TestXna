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

        IMap<float> heightMap;

        float amplitude;

        bool textureDirty;

        public Texture2D Texture
        {
            get
            {
                RefreshTexture();
                return flipTexture.Texture;
            }
        }

        public IMap<float> HeightMap
        {
            get { return heightMap; }
            set { heightMap = value; }
        }

        public float Amplitude
        {
            get { return amplitude; }
            set { amplitude = value; }
        }

        public NormalMap(GraphicsDevice graphicsDevice, int width, int height)
            : base(width, height)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");

            flipTexture = new FlipTexture2D(graphicsDevice, width, height, false, SurfaceFormat.Rgba64);
        }

        public void Build()
        {
            int w = Math.Min(Width, heightMap.Width);
            int h = Math.Min(Height, heightMap.Height);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    var h0 = heightMap[x, y - 1];
                    var h1 = heightMap[x, y + 1];
                    var h2 = heightMap[x - 1, y];
                    var h3 = heightMap[x + 1, y];

                    Vector3 normal;
                    CalculateNormal(h0, h1, h2, h3, out normal);

                    Rgba64 value;
                    NormalToRgba64(ref normal, out value);

                    Values[x + y * Width] = value;
                }
            }

            textureDirty = true;
        }

        public void MergeLeftNeighbor(NormalMap neighbor)
        {
            var neighborHeightMap = neighbor.heightMap;
            var rightEdge = Width - 1;

            for (int y = 0; y < Height; y++)
            {
                var h0 = heightMap[0, y - 1];
                var h1 = heightMap[0, y + 1];
                var h2 = neighborHeightMap[rightEdge - 1, y];
                var h3 = heightMap[0 + 1, y];

                Vector3 normal;
                CalculateNormal(h0, h1, h2, h3, out normal);

                Rgba64 value;
                NormalToRgba64(ref normal, out value);

                this[0, y] = value;
                neighbor[rightEdge, y] = value;
            }

            textureDirty = true;
        }

        public void MergeRightNeighbor(NormalMap neighbor)
        {
            var neighborHeightMap = neighbor.heightMap;
            var rightEdge = Width - 1;

            for (int y = 0; y < Height; y++)
            {
                var h0 = heightMap[rightEdge, y - 1];
                var h1 = heightMap[rightEdge, y + 1];
                var h2 = heightMap[rightEdge - 1, y];
                var h3 = neighborHeightMap[0 + 1, y];

                Vector3 normal;
                CalculateNormal(h0, h1, h2, h3, out normal);

                Rgba64 value;
                NormalToRgba64(ref normal, out value);

                this[rightEdge, y] = value;
                neighbor[0, y] = value;
            }

            textureDirty = true;
        }

        public void MergeTopNeighbor(NormalMap neighbor)
        {
            var neighborHeightMap = neighbor.heightMap;
            var bottomEdge = Height - 1;

            for (int x = 0; x < Width; x++)
            {
                var h0 = neighborHeightMap[x, bottomEdge - 1];
                var h1 = heightMap[x, 0 + 1];
                var h2 = heightMap[x - 1, 0];
                var h3 = heightMap[x + 1, 0];

                Vector3 normal;
                CalculateNormal(h0, h1, h2, h3, out normal);

                Rgba64 value;
                NormalToRgba64(ref normal, out value);

                this[x, 0] = value;
                neighbor[x, bottomEdge] = value;
            }

            textureDirty = true;
        }

        public void MergeBottomNeighbor(NormalMap neighbor)
        {
            var neighborHeightMap = neighbor.heightMap;
            var bottomEdge = Height - 1;

            for (int x = 0; x < Width; x++)
            {
                var h0 = heightMap[x, bottomEdge - 1];
                var h1 = neighborHeightMap[x, 0 + 1];
                var h2 = heightMap[x - 1, bottomEdge];
                var h3 = heightMap[x + 1, bottomEdge];

                Vector3 normal;
                CalculateNormal(h0, h1, h2, h3, out normal);

                Rgba64 value;
                NormalToRgba64(ref normal, out value);

                this[x, bottomEdge] = value;
                neighbor[x, 0] = value;
            }

            textureDirty = true;
        }

        void CalculateNormal(float h0, float h1, float h2, float h3, out Vector3 normal)
        {
            var d0 = new Vector3(0, 1, (h1 - h0) * amplitude);
            var d1 = new Vector3(1, 0, (h3 - h2) * amplitude);
            d0.Normalize();
            d1.Normalize();

            Vector3.Cross(ref d0, ref d1, out normal);
            normal.Normalize();
        }

        void NormalToRgba64(ref Vector3 normal, out Rgba64 result)
        {
            normal.X = normal.X * 0.5f + 0.5f;
            normal.Y = normal.Y * 0.5f + 0.5f;
            normal.Z = normal.Z * 0.5f + 0.5f;

            result = new Rgba64(normal.X, normal.Y, normal.Z, 0);
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
