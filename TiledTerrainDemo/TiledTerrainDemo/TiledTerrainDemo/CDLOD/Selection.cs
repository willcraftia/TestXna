#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace TiledTerrainDemo.CDLOD
{
    public sealed class Selection
    {
        public const int MaxSelectedNodeCount = 500;

        public Vector3 TerrainOffset;

        public Matrix View;

        public Matrix Projection;

        public Vector3 EyePosition;

        public Texture2D HeightMapTexture;

        public Settings Settings;

        SelectedNode[] selectedNodes;

        public IVisibleRanges VisibleRanges { get; private set; }

        public BoundingFrustum Frustum { get; private set; }

        public int SelectedNodeCount { get; private set; }

        public Selection(Settings settings, IVisibleRanges visibleRanges)
        {
            Settings = settings;
            VisibleRanges = visibleRanges;

            Frustum = new BoundingFrustum(Matrix.Identity);
            selectedNodes = new SelectedNode[MaxSelectedNodeCount];
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

        public void GetSelectedNode(int index, out SelectedNode selectedNode)
        {
            selectedNode = selectedNodes[index];
        }

        public void GetVisibilitySphere(int level, out BoundingSphere sphere)
        {
            sphere = new BoundingSphere(EyePosition, VisibleRanges[level]);
        }

        public void AddSelectedNode(Node node)
        {
            if (MaxSelectedNodeCount <= SelectedNodeCount) return;

            selectedNodes[SelectedNodeCount++] = new SelectedNode
            {
                X = node.X,
                Y = node.Y,
                MinHeight = node.MinHeight,
                MaxHeight = node.MaxHeight,
                Size = node.Size,
                Level = node.Level
            };
        }

        public void GetPatchInstanceVertex(int index, out PatchInstanceVertex instance)
        {
            instance = new PatchInstanceVertex();
            instance.Offset.X = selectedNodes[index].X * Settings.PatchScale;
            instance.Offset.Y = selectedNodes[index].Y * Settings.PatchScale;
            instance.Scale = selectedNodes[index].Size * Settings.PatchScale;
            instance.Level = selectedNodes[index].Level;
        }
    }
}
