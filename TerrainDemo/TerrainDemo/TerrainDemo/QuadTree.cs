#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace TerrainDemo
{
    public sealed class QuadTree
    {
        GraphicsDevice graphicsDevice;

        QuadNode rootNode;

        TreeVertexCollection treeVertices;

        BufferManager buffers;

        Vector3 position;

        int topNodeSize;

        int[] indices;

        public Matrix View;
        public Matrix Projection;

        Matrix lastView;

        public int TopNodeSize
        {
            get { return topNodeSize; }
        }

        public QuadNode RootNode
        {
            get { return rootNode; }
        }

        public TreeVertexCollection TreeVertices
        {
            get { return treeVertices; }
        }

        public BoundingFrustum ViewFrustum { get; private set; }

        public int IndexCount { get; private set; }

        public BasicEffect Effect;

        public int MinimumDepth;

        QuadNode activeNode;

        public bool Cull { get; set; }

        public ViewClipShape ClipShape;
        internal Vector3[] VFCorners = new Vector3[8];

        public QuadTree(GraphicsDevice graphicsDevice, Vector3 position, HeightMap heightMap, Matrix view, Matrix projection, Vector3 scale)
        {
            this.graphicsDevice = graphicsDevice;
            this.position = position;
            this.topNodeSize = heightMap.Size - 1;

            treeVertices = new TreeVertexCollection(position, heightMap, scale);
            buffers = new BufferManager(graphicsDevice, treeVertices.Vertices);
            rootNode = new QuadNode(QuadNodeType.FullNode, topNodeSize, 1, null, this, 0);
            View = view;
            Projection = projection;
            
            ViewFrustum = new BoundingFrustum(view * projection);

            indices = new int[(heightMap.Size + 1) * (heightMap.Size + 1) * 3];

            Effect = new BasicEffect(graphicsDevice);
            Effect.EnableDefaultLighting();
            Effect.FogEnabled = true;
            Effect.FogStart = 300;
            Effect.FogEnd = 1000;
            Effect.FogColor = Color.Black.ToVector3();
            Effect.TextureEnabled = true;
            Effect.World = Matrix.Identity;
            Effect.View = view;
            Effect.Projection = projection;
        }

        public void UpdateBuffer(int index)
        {
            indices[IndexCount] = index;
            IndexCount++;
        }

        public void Update(GameTime gameTime)
        {
            if (View == lastView) return;

            ViewFrustum.Matrix = View * Projection;

            Effect.View = View;
            Effect.Projection = Projection;

            Matrix inverseView;
            Matrix.Invert(ref View, out inverseView);
            var cameraPosition = inverseView.Translation;

            ViewFrustum.GetCorners(VFCorners);
            var clip = ClippingFrustrum.FromFrustrumCorners(VFCorners, cameraPosition);
            ClipShape = clip.ProjectToTargetY(position.Y);

            lastView = View;
            IndexCount = 0;

            rootNode.Merge();
            rootNode.EnforceMinimumDepth();

            activeNode = rootNode.GetDeepestNodeWithPoint(ClipShape.ViewPoint);
            if (activeNode != null) activeNode.Split();

            rootNode.SetActiveVertices();

            buffers.UpdateIndexBuffer(indices, IndexCount);
            buffers.SwapIndexBuffer();
        }

        public void Draw(GameTime gameTime)
        {
            graphicsDevice.SetVertexBuffer(buffers.VertexBuffer);
            graphicsDevice.Indices = buffers.IndexBuffer;

            foreach (var pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList, 0, 0, treeVertices.Vertices.Length, 0, IndexCount / 3);
            }
        }
    }
}
