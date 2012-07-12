#region Using

using System;

#endregion

namespace TerrainDemo.CDLOD
{
    public struct SelectedNode
    {
        public int X;

        public int Y;

        public int Size;

        // 使わないかも？
        public float MinHeight;

        // 使わないかも？
        public float MaxHeight;

        public int Level;
    }
}
