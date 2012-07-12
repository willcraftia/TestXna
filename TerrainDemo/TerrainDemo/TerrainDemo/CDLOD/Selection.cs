#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

#endregion

namespace TerrainDemo.CDLOD
{
    public sealed class Selection
    {
        public Vector3 EyePosition;

        public Vector3 TerrainOffset;

        public float PatchScale;

        public float HeightScale;

        public Morph Morph;

        // 処理効率のために配列を公開。
        public SelectedNode[] SelectedNodes;

        public int SelectedNodeCount;

        public int MaxSelectedNodeCount;

        public void AddSelectedNode(Node node)
        {
            SelectedNodes[SelectedNodeCount++] = new SelectedNode
            {
                X = node.X,
                Y = node.Y,
                MinHeight = node.MinHeight,
                MaxHeight = node.MaxHeight,
                Size = node.Size,
                Level = node.Level
            };
        }
    }
}
