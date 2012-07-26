#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace MidpointDisplacementDemo
{
    public sealed class QuadMesh : IDisposable
    {
        public VertexBuffer VertexBuffer { get; private set; }

        public QuadMesh(GraphicsDevice graphicsDevice, float size)
        {
            var half = size * 0.5f;

            VertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionTexture), 4, BufferUsage.WriteOnly);
            var vertices = new VertexPositionTexture[4];
            vertices[0] = new VertexPositionTexture(new Vector3(0, size, 0.0f), new Vector2(0, 0));
            vertices[1] = new VertexPositionTexture(new Vector3(size, size, 0.0f), new Vector2(1, 0));
            vertices[2] = new VertexPositionTexture(new Vector3(0, 0, 0.0f), new Vector2(0, 1));
            vertices[3] = new VertexPositionTexture(new Vector3(size, 0, 0.0f), new Vector2(1, 1));
            VertexBuffer.SetData(vertices);
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed;

        ~QuadMesh()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                VertexBuffer.Dispose();
            }

            disposed = true;
        }

        #endregion
    }
}
