#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Terrain.CDLOD
{
    public sealed class CDLODSelection
    {
        public const int MaxSelectedNodeCount = 500;

        public Vector3 TerrainOffset;

        public Matrix View;

        public Matrix Projection;

        public Vector3 EyePosition;

        public Texture2D HeightMapTexture;

        public Texture2D NormalMapTexture;

        public CDLODSettings Settings;

        CDLODSelectedNode[] selectedNodes;

        public ICDLODVisibleRanges VisibleRanges { get; private set; }

        public BoundingFrustum Frustum { get; private set; }

        public int SelectedNodeCount { get; private set; }

        public CDLODSelection(CDLODSettings settings, ICDLODVisibleRanges visibleRanges)
        {
            Settings = settings;
            VisibleRanges = visibleRanges;

            Frustum = new BoundingFrustum(Matrix.Identity);
            selectedNodes = new CDLODSelectedNode[MaxSelectedNodeCount];
        }

        public void Prepare()
        {
            // Calculate the eye position from a view matrix.
            Matrix inverseView;
            Matrix.Invert(ref View, out inverseView);
            EyePosition = inverseView.Translation;

            // Update the view frustum.
            Frustum.Matrix = View * Projection;
        }

        public void ClearSelectedNodes()
        {
            // Reset the counter.
            SelectedNodeCount = 0;
        }

        public void GetSelectedNode(int index, out CDLODSelectedNode selectedNode)
        {
            selectedNode = selectedNodes[index];
        }

        public void GetVisibilitySphere(int level, out BoundingSphere sphere)
        {
            sphere = new BoundingSphere(EyePosition, VisibleRanges[level]);
        }

        internal void AddSelectedNode(Node node)
        {
            if (MaxSelectedNodeCount <= SelectedNodeCount) return;

            selectedNodes[SelectedNodeCount++] = new CDLODSelectedNode
            {
                X = node.X,
                Y = node.Y,
                MinHeight = node.MinHeight,
                MaxHeight = node.MaxHeight,
                Size = node.Size,
                Level = node.Level
            };
        }

        internal void GetPatchInstanceVertex(int index, out PatchInstanceVertex instance)
        {
            instance = new PatchInstanceVertex();
            instance.Offset.X = selectedNodes[index].X * Settings.PatchScale;
            instance.Offset.Y = selectedNodes[index].Y * Settings.PatchScale;
            instance.Scale = selectedNodes[index].Size * Settings.PatchScale;
            instance.Level = selectedNodes[index].Level;
        }
    }
}
