#region Using

using System;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace TerrainDemo.LOD
{
    public sealed class BufferManager
    {
        GraphicsDevice graphicsDevice;

        IndexBuffer[] indexBuffers;

        int active;

        public VertexBuffer VertexBuffer { get; private set; }

        public IndexBuffer IndexBuffer
        {
            get { return indexBuffers[active]; }
        }

        public BufferManager(GraphicsDevice graphicsDevice, VertexPositionNormalTexture[] vertices)
        {
            this.graphicsDevice = graphicsDevice;

            VertexBuffer = new VertexBuffer(
                graphicsDevice, VertexPositionNormalTexture.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
            VertexBuffer.SetData(vertices);

            indexBuffers = new IndexBuffer[]
            {
                new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, 100000, BufferUsage.WriteOnly),
                new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, 100000, BufferUsage.WriteOnly)
            };
        }

        public void UpdateIndexBuffer(int[] indices, int indexCount)
        {
            var inactive = (active == 0) ? 1 : 0;
            indexBuffers[inactive].SetData(indices, 0, indexCount);
        }

        public void SwapIndexBuffer()
        {
            active = (active == 0) ? 1 : 0;
        }
    }
}
