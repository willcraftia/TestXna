#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace TerrainDemo
{
    public sealed class TreeVertexCollection
    {
        public VertexPositionNormalTexture[] Vertices;

        Vector3 position;

        int topSize;

        int halfSize;

        int scale;

        int heightMapSize;

        public TreeVertexCollection(Vector3 position, HeightMap heightMap, int scale)
        {
            this.position = position;
            this.scale = scale;

            heightMapSize = heightMap.Size;
            topSize = heightMapSize - 1;
            halfSize = topSize / 2;

            var vertexCount = heightMapSize * heightMapSize;
            Vertices = new VertexPositionNormalTexture[vertexCount];

            BuildVertices(heightMap);
            CalculateNormals();
        }

        void BuildVertices(HeightMap heightMap)
        {
            var x = position.X;
            var y = position.Y;
            var z = position.Z;
            var maxX = x + topSize;

            for (int i = 0; i < Vertices.Length; i++)
            {
                if (maxX < x)
                {
                    x = position.X;
                    z++;
                }

                y = position.Y + heightMap.Heights[i] * 100;
                var vertexPosition = new Vector3(x * scale, y * scale, z * scale);
                var vertexNormal = Vector3.Zero;
                var vertexTexcood = new Vector2
                {
                    X = (vertexPosition.X - position.X) / topSize,
                    Y = (vertexPosition.Z - position.Z) / topSize
                };
                Vertices[i] = new VertexPositionNormalTexture(vertexPosition, vertexNormal, vertexTexcood);
                x++;
            }
        }

        void CalculateNormals()
        {
            var length = topSize * topSize;
            for (int i = 0; i < length; i++)
            {
                var lowerLeft = i;
                var lowerRight = i + 1;
                var topLeft = i + heightMapSize;
                var topRight = i + heightMapSize + 1;

                CalculateNormal(topLeft, lowerRight, lowerLeft);
                CalculateNormal(topLeft, topRight, lowerRight);
            }

            for (int i = 0; i < Vertices.Length; i++)
                Vertices[i].Normal.Normalize();
        }

        void CalculateNormal(int index0, int index1, int index2)
        {
            var side0 = Vertices[index0].Position - Vertices[index2].Position;
            var side1 = Vertices[index0].Position - Vertices[index1].Position;
            var normal = Vector3.Cross(side0, side1);

            Vertices[index0].Normal += normal;
            Vertices[index1].Normal += normal;
            Vertices[index2].Normal += normal;
        }
    }
}
