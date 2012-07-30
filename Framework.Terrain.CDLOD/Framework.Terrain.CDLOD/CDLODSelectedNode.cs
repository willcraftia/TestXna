#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework.Terrain.CDLOD
{
    public struct CDLODSelectedNode
    {
        public int X;

        public int Y;

        public int Size;

        public float MinHeight;

        public float MaxHeight;

        public int Level;

        public void GetBoundingBox(ref Vector3 terrainOffset, float mapScale, float heightScale, out BoundingBox boundingBox)
        {
            boundingBox = new BoundingBox();

            boundingBox.Min.X = X * mapScale + terrainOffset.X;
            boundingBox.Min.Y = MinHeight * heightScale + terrainOffset.Y;
            boundingBox.Min.Z = Y * mapScale + terrainOffset.Z;

            boundingBox.Max.X = (X + Size) * mapScale + terrainOffset.X;
            boundingBox.Max.Y = MaxHeight * heightScale + terrainOffset.Y;
            boundingBox.Max.Z = (Y + Size) * mapScale + terrainOffset.Z;
        }
    }
}
