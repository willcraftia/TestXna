#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Terrain.CDLOD
{
    public sealed class CDLODTerrainRenderer : IDisposable
    {
        CDLODSettings settings;

        PatchInstanceVertex[] instances = new PatchInstanceVertex[CDLODSelection.MaxSelectedNodeCount];

        /// <summary>
        /// The vertex buffer to populate instances.
        /// </summary>
        WritableVertexBuffer<PatchInstanceVertex> instanceVertexBuffer;

        /// <summary>
        /// Vertex bindings.
        /// </summary>
        VertexBufferBinding[] vertexBufferBindings = new VertexBufferBinding[2];

        /// <summary>
        /// A patch mesh.
        /// </summary>
        PatchMesh patchMesh;

        public GraphicsDevice GraphicsDevice { get; private set; }

        public CDLODSettings Settings
        {
            get { return settings; }
        }

        public int PatchGridSize
        {
            get { return patchMesh.GridSize; }
        }

        public CDLODTerrainRenderer(GraphicsDevice graphicsDevice, CDLODSettings settings)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");

            GraphicsDevice = graphicsDevice;
            this.settings = settings;

            instanceVertexBuffer = new WritableVertexBuffer<PatchInstanceVertex>(GraphicsDevice, CDLODSelection.MaxSelectedNodeCount * 2);

            // TODO: I want to change a patch resolution at runtime.
            // patchGridSize = leafNodeSize * patchResolution;
            patchMesh = new PatchMesh(GraphicsDevice, settings.PatchGridSize);
        }

        public void Draw(GameTime gameTime, Effect effect, CDLODSelection selection)
        {
            if (selection.SelectedNodeCount == 0) return;

            // create instances
            for (int i = 0; i < selection.SelectedNodeCount; i++)
                selection.GetPatchInstanceVertex(i, out instances[i]);

            var offset = instanceVertexBuffer.SetData(instances, 0, selection.SelectedNodeCount);

            vertexBufferBindings[0] = new VertexBufferBinding(patchMesh.VertexBuffer, 0);
            vertexBufferBindings[1] = new VertexBufferBinding(instanceVertexBuffer.VertexBuffer, offset, 1);

            GraphicsDevice.SetVertexBuffers(vertexBufferBindings);
            GraphicsDevice.Indices = patchMesh.IndexBuffer;

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawInstancedPrimitives(
                    PrimitiveType.TriangleList, 0, 0,
                    patchMesh.NumVertices, 0, patchMesh.PrimitiveCount, selection.SelectedNodeCount);
            }
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed;

        ~CDLODTerrainRenderer()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                patchMesh.Dispose();
            }

            disposed = true;
        }

        #endregion
    }
}
