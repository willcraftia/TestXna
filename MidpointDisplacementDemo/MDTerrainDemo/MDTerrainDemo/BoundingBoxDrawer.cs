#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace MDTerrainDemo
{
    public sealed class BoundingBoxDrawer
    {
        static readonly int[] indices;
        static readonly int primitiveCount;

        static BoundingBoxDrawer()
        {
            indices = new int[]
            {
                0, 1,
                1, 2,
                2, 3,
                3, 0,
                0, 4,
                1, 5,
                2, 6,
                3, 7,
                4, 5,
                5, 6,
                6, 7,
                7, 4,
            };
            primitiveCount = indices.Length / 2;
        }

        GraphicsDevice graphicsDevice;
        Vector3[] corners = new Vector3[8];
        VertexPositionColor[] vertices = new VertexPositionColor[8];

        public BoundingBoxDrawer(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
        }

        public void Draw(ref BoundingBox box, Effect effect, ref Color vertexColor)
        {
            box.GetCorners(corners);
            for (int i = 0; i < 8; i++)
            {
                vertices[i].Position = corners[i];
                vertices[i].Color = vertexColor;
            }

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(
                    PrimitiveType.LineList,
                    vertices,
                    0,
                    8,
                    indices,
                    0,
                    primitiveCount);
            }
        }
    }
}
