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

        //Vector3 cameraPosition;

        //Vector3 lastCameraPosition = new Vector3(float.NaN, float.NaN, float.NaN);

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

        //public Vector3 CameraPosition
        //{
        //    get { return cameraPosition; }
        //    set { cameraPosition = value; }
        //}

        public BoundingFrustum ViewFrustum { get; set; }

        public int IndexCount { get; set; }

        public BasicEffect Effect;

        public int MinimumDepth = 6;

        public QuadTree(GraphicsDevice graphicsDevice, Vector3 position, HeightMap heightMap, Matrix view, Matrix projection, int scale)
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
            //Effect.Texture = new Texture2D(graphicsDevice, 100, 100);
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
            //if (cameraPosition == lastCameraPosition) return;
            if (View == lastView) return;

            Effect.View = View;
            Effect.Projection = Projection;

            //lastCameraPosition = cameraPosition;
            lastView = View;
            IndexCount = 0;

            rootNode.EnforceMinimumDepth();
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
