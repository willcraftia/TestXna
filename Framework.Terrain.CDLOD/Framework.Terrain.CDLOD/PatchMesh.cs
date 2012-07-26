#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Terrain.CDLOD
{
    internal sealed class PatchMesh : IDisposable
    {
        public int GridSize { get; private set; }

        public int NumVertices { get; private set; }

        public int PrimitiveCount { get; private set; }

        public VertexBuffer VertexBuffer { get; private set; }

        public IndexBuffer IndexBuffer { get; private set; }

        public int TopLeftEndIndex { get; private set; }

        public int TopRightEndIndex { get; private set; }

        public int BottomLeftEndIndex { get; private set; }

        public int BottomRightEndIndex { get; private set; }

        public PatchMesh(GraphicsDevice graphicsDevice, int gridSize)
        {
            // GridSize must be leafNodeSize * n, and n must be power of 2.
            GridSize = gridSize;

            int vertexSize = gridSize + 1;

            NumVertices = vertexSize * vertexSize;
            VertexBuffer = new VertexBuffer(graphicsDevice, VertexPosition.VertexDeclaration, NumVertices, BufferUsage.WriteOnly);

            var vertices = new VertexPosition[NumVertices];
            for (int z = 0; z < vertexSize; z++)
                for (int x = 0; x < vertexSize; x++)
                    vertices[vertexSize * z + x] = new VertexPosition(new Vector3(x / (float) gridSize, 0, z / (float) gridSize));

            VertexBuffer.SetData(vertices);

            PrimitiveCount = gridSize * gridSize * 2;
            var indexCount = PrimitiveCount * 3;
            IndexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, indexCount, BufferUsage.WriteOnly);

            var indices = new ushort[indexCount];
            int index = 0;
            int halfSize = gridSize / 2;

            // Top left.
            for (int z = 0; z < halfSize; z++)
            {
                for (int x = 0; x < halfSize; x++)
                {
                    indices[index++] = (ushort) ((x + 0) + (z + 0) * vertexSize);
                    indices[index++] = (ushort) ((x + 1) + (z + 0) * vertexSize);
                    indices[index++] = (ushort) ((x + 0) + (z + 1) * vertexSize);

                    indices[index++] = (ushort) ((x + 1) + (z + 0) * vertexSize);
                    indices[index++] = (ushort) ((x + 1) + (z + 1) * vertexSize);
                    indices[index++] = (ushort) ((x + 0) + (z + 1) * vertexSize);
                }
            }
            TopLeftEndIndex = index;

            // Top right.
            for (int z = 0; z < halfSize; z++)
            {
                for (int x = halfSize; x < gridSize; x++)
                {
                    indices[index++] = (ushort) ((x + 0) + (z + 0) * vertexSize);
                    indices[index++] = (ushort) ((x + 1) + (z + 0) * vertexSize);
                    indices[index++] = (ushort) ((x + 0) + (z + 1) * vertexSize);

                    indices[index++] = (ushort) ((x + 1) + (z + 0) * vertexSize);
                    indices[index++] = (ushort) ((x + 1) + (z + 1) * vertexSize);
                    indices[index++] = (ushort) ((x + 0) + (z + 1) * vertexSize);
                }
            }
            TopRightEndIndex = index;

            // Bottom left.
            for (int z = halfSize; z < gridSize; z++)
            {
                for (int x = 0; x < halfSize; x++)
                {
                    indices[index++] = (ushort) ((x + 0) + (z + 0) * vertexSize);
                    indices[index++] = (ushort) ((x + 1) + (z + 0) * vertexSize);
                    indices[index++] = (ushort) ((x + 0) + (z + 1) * vertexSize);

                    indices[index++] = (ushort) ((x + 1) + (z + 0) * vertexSize);
                    indices[index++] = (ushort) ((x + 1) + (z + 1) * vertexSize);
                    indices[index++] = (ushort) ((x + 0) + (z + 1) * vertexSize);
                }
            }
            BottomLeftEndIndex = index;

            // Bottom right.
            for (int z = halfSize; z < gridSize; z++)
            {
                for (int x = halfSize; x < gridSize; x++)
                {
                    indices[index++] = (ushort) ((x + 0) + (z + 0) * vertexSize);
                    indices[index++] = (ushort) ((x + 1) + (z + 0) * vertexSize);
                    indices[index++] = (ushort) ((x + 0) + (z + 1) * vertexSize);

                    indices[index++] = (ushort) ((x + 1) + (z + 0) * vertexSize);
                    indices[index++] = (ushort) ((x + 1) + (z + 1) * vertexSize);
                    indices[index++] = (ushort) ((x + 0) + (z + 1) * vertexSize);
                }
            }
            BottomRightEndIndex = index;

            IndexBuffer.SetData(indices);
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed;

        ~PatchMesh()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                VertexBuffer.Dispose();
                IndexBuffer.Dispose();
            }

            disposed = true;
        }

        #endregion
    }
}
