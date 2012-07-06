#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace TerrainDemo.NoLOD
{
    public sealed class NoLODTerrain : IDisposable
    {
        GraphicsDevice graphicsDevice;

        float scale = 1;

        float bumpiness = 64;

        HeightMap heightMap;

        HeightColorCollection heightColors = new HeightColorCollection();

        VertexPositionColor[] vertices;

        int[] indices;

        public HeightMap HeightMap
        {
            get { return heightMap; }
            set { heightMap = value; }
        }

        public HeightColorCollection HeightColors
        {
            get { return heightColors; }
            set { heightColors = value; }
        }

        public VertexBuffer VertexBuffer { get; set; }

        public IndexBuffer IndexBuffer { get; set; }

        public NoLODTerrain(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
        }

        public void Build()
        {
            CreateVertices();
            CreateIndices();
        }

        void CreateVertices()
        {
            var size = heightMap.Size;
            var heights = heightMap.Heights;
            var vertexCount = size * size;

            if (vertices == null || vertices.Length != vertexCount)
                vertices = new VertexPositionColor[vertexCount];

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    var index = i * size + j;
                    vertices[index].Position = new Vector3
                    {
                        X = j * scale,
                        Y = heights[index] * bumpiness,
                        Z = i * scale
                    };

                    Vector4 c;
                    heightColors.GetColor(heights[index], out c);
                    vertices[index].Color = new Color(c.X, c.Y, c.Z, c.W);
                }
            }

            if (VertexBuffer != null && VertexBuffer.VertexCount != vertexCount)
            {
                VertexBuffer.Dispose();
                VertexBuffer = null;
            }

            if (VertexBuffer == null)
                VertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionColor), vertexCount, BufferUsage.WriteOnly);
            
            VertexBuffer.SetData(vertices);
        }

        void CreateIndices()
        {
            var size = heightMap.Size;
            var heights = heightMap.Heights;
            var indexCount = (size - 1) * (size - 1) * 3 * 2;

            if (indices == null || indices.Length != indexCount)
                indices = new int[indexCount];

            int counter = 0;
            for (int i = 0; i < size - 1; i++)
            {
                for (int j = 0; j < size - 1; j++)
                {
                    var leftDown = (short) (i * size + j);
                    var rightDown = (short) (i * size + (j + 1));
                    var leftUp = (short) ((i + 1) * size + j);
                    var rightUp = (short) ((i + 1) * size + (j + 1));

                    indices[counter++] = leftUp;
                    indices[counter++] = rightDown;
                    indices[counter++] = leftDown;

                    indices[counter++] = leftUp;
                    indices[counter++] = rightUp;
                    indices[counter++] = rightDown;
                }
            }

            if (IndexBuffer != null && IndexBuffer.IndexCount != indexCount)
            {
                IndexBuffer.Dispose();
                IndexBuffer = null;
            }

            if (IndexBuffer == null)
                IndexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, indexCount, BufferUsage.WriteOnly);

            IndexBuffer.SetData(indices);
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed;

        ~NoLODTerrain()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                if (VertexBuffer != null) VertexBuffer.Dispose();
                if (IndexBuffer != null) IndexBuffer.Dispose();
            }

            disposed = true;
        }

        #endregion
    }
}
