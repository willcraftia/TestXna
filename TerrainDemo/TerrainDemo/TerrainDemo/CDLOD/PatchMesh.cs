#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace TerrainDemo.CDLOD
{
    public sealed class PatchMesh : IDisposable
    {
        public int NumVertices { get; private set; }

        public int PrimitiveCount { get; private set; }

        public VertexBuffer VertexBuffer { get; private set; }

        public IndexBuffer IndexBuffer { get; private set; }

        public int TopLeftEndIndex { get; private set; }

        public int TopRightEndIndex { get; private set; }

        public int BottomLeftEndIndex { get; private set; }

        public int BottomRightEndIndex { get; private set; }

        public PatchMesh(GraphicsDevice graphicsDevice)
        {
            int size = 2;
            int vertexSize = size + 1;

            // 4 つの正方形 (8 つの三角形) の頂点数。
            NumVertices = vertexSize * vertexSize;
            VertexBuffer = new VertexBuffer(graphicsDevice, VertexPosition.VertexDeclaration, NumVertices, BufferUsage.WriteOnly);

            var vertices = new VertexPosition[NumVertices];
            for (int z = 0; z < vertexSize; z++)
                for (int x = 0; x < vertexSize; x++)
                    vertices[vertexSize * z + x] = new VertexPosition(new Vector3(x / (float) size, 0, z / (float) size));

            VertexBuffer.SetData(vertices);

            // 8 つの三角形で構築。
            PrimitiveCount = size * size * 2;
            // 8 つの三角形のためのインデックス数。
            var indexCount = PrimitiveCount * 3;
            IndexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, indexCount, BufferUsage.WriteOnly);

            var indices = new ushort[indexCount];
            int index = 0;
            int halfSize = size / 2;

            // Top left の面。
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

            // Top right の面。
            for (int z = 0; z < halfSize; z++)
            {
                for (int x = halfSize; x < size; x++)
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

            // Bottom left の面。
            for (int z = halfSize; z < size; z++)
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

            // Bottom right の面。
            for (int z = halfSize; z < size; z++)
            {
                for (int x = halfSize; x < size; x++)
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
