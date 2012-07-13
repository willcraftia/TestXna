#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace TerrainDemo.CDLOD
{
    public struct SelectedNode
    {
        public int X;

        public int Y;

        public int Size;

        public float MinHeight;

        public float MaxHeight;

        public int Level;

        public void GetBoundingBox(ref Vector3 terrainOffset, float patchScale, float heightScale, out BoundingBox boundingBox)
        {
            boundingBox = new BoundingBox();

            boundingBox.Min.X = X * patchScale + terrainOffset.X;
            boundingBox.Min.Y = MinHeight * heightScale + terrainOffset.Y;
            boundingBox.Min.Z = Y * patchScale + terrainOffset.Z;

            boundingBox.Max.X = (X + Size) * patchScale + terrainOffset.X;
            boundingBox.Max.Y = MaxHeight * heightScale + terrainOffset.Y;
            boundingBox.Max.Z = (Y + Size) * patchScale + terrainOffset.Z;
        }
    }
}
