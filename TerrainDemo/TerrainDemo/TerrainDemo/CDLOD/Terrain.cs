#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TerrainDemo.Framework.Graphics;

#endregion

namespace TerrainDemo.CDLOD
{
    public sealed class Terrain : IDisposable
    {
        Settings settings;

        QuadTree quadTree = new QuadTree();

        Texture2D heightMapTexture;

        public GraphicsDevice GraphicsDevice { get; private set; }

        public Terrain(GraphicsDevice graphicsDevice, Settings settings)
        {
            GraphicsDevice = graphicsDevice;
            this.settings = settings;
        }

        public void Initialize(IHeightMapSource heightMap)
        {
            // Build the quadtree.
            var createDescription = new CreateDescription
            {
                LeafNodeSize = settings.LeafNodeSize,
                LevelCount = settings.LevelCount,
                HeightMap = heightMap
            };
            quadTree.Build(ref createDescription);

            // Initialize a height map texture.
            InitializeHeightMapTexture(heightMap);
        }

        /// <summary>
        /// Initialize a height map texture.
        /// The height map texture is created with SurfaceFormat.Single.
        /// </summary>
        /// <param name="heightMap"></param>
        void InitializeHeightMapTexture(IHeightMapSource heightMap)
        {
            heightMapTexture = new Texture2D(GraphicsDevice, heightMap.Width, heightMap.Height, false, SurfaceFormat.Single);

            var heights = new float[heightMap.Width * heightMap.Height];
            for (int y = 0; y < heightMap.Height; y++)
                for (int x = 0; x < heightMap.Width; x++)
                    heights[x + y * heightMap.Width] = heightMap.GetHeight(x, y);

            heightMapTexture.SetData(heights);
        }

        public void Select(Selection selection)
        {
            // Prepare selection's state per an update.
            selection.ClearSelectedNodes();

            // Select visible nodes.
            quadTree.Select(selection);

            // Set the height map texture to render.
            selection.HeightMapTexture = heightMapTexture;
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed;

        ~Terrain()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                if (heightMapTexture != null) heightMapTexture.Dispose();
            }

            disposed = true;
        }

        #endregion
    }
}
