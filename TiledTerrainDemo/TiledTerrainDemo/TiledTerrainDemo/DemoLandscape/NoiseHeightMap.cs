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
        NoiseMapBuilder builder = new NoiseMapBuilder();

        Map<float> noiseMap;

        int width;

        int height;

        int overlapSize;

        Bounds bounds;

        //Texture2D texture;
        FlipTexture2D flipTexture;

        // TODO
        public bool textureDirty;

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
                x += overlapSize;
                y += overlapSize;

                // Clamp.
                if (x < 0) return 0;
                if (noiseMap.Width <= x) return 1;
                if (y < 0) return 0;
                if (noiseMap.Height <= y) return 1;

                return noiseMap[x, y];
            }
            set
            {
                x += overlapSize;
                y += overlapSize;

                if (x < 0 || noiseMap.Width <= x || y < 0 || noiseMap.Height <= y)
                    return;

                noiseMap[x, y] = value;
            }
        }

        public Texture2D Texture
        {
            get
            {
                RefreshTexture();
                //return texture;
                return flipTexture.Texture;
            }
        }

        public NoiseHeightMap(GraphicsDevice graphicsDevice, CDLODSettings settings)
        {
            GraphicsDevice = graphicsDevice;
            width = settings.HeightMapWidth;
            height = settings.HeightMapHeight;
            overlapSize = settings.HeightMapOverlapSize;

            noiseMap = new Map<float>(width + 2 * overlapSize, height + 2 * overlapSize);
            builder.Destination = noiseMap;

            //texture = new Texture2D(graphicsDevice, noiseMap.Width, noiseMap.Height, false, SurfaceFormat.Single);
            flipTexture = new FlipTexture2D(graphicsDevice, noiseMap.Width, noiseMap.Height, false, SurfaceFormat.Single);
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
            Erosion.ErodeThermal(this, 0.05f, 10);

            textureDirty = true;
        }

        public void RefreshTexture()
        {
            if (!textureDirty) return;

            flipTexture.Flip();
            //texture.SetData(noiseMap.Values);
            flipTexture.Texture.SetData(noiseMap.Values);

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
