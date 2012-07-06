#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace TerrainDemo.LOD
{
    public sealed class TreeVertexCollection
    {
        public VertexPositionNormalTexture[] Vertices;

        Vector3 position;

        int topSize;

        int halfSize;

        Vector3 scale;

        int heightMapSize;

        public TreeVertexCollection(Vector3 position, HeightMap heightMap, Vector3 scale)
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

                y = position.Y + heightMap.Heights[i] * scale.Y;
                var vertexPosition = new Vector3(x * scale.X, y, z * scale.Z);
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
            for (int i = 0; i < topSize; i++)
            {
                for (int j = 0; j < topSize; j++)
                {
                    var bottomLeft = j + i * heightMapSize;
                    var bottomRight = (j + 1) + i * heightMapSize;
                    var topLeft = j + (i + 1) * heightMapSize;
                    //var topRight = (j + 1) + (i + 1) * heightMapSize;

                    CalculateNormal(topLeft, bottomLeft, bottomRight);
                }
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
