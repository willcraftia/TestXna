#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace TerrainDemo.CDLOD
{
    public sealed class Selection
    {
        public const int MaxSelectedNodeCount = 4000;

        public const float DefaultPatchScale = 2;

        public const float DefaultHeightScale = 50;

        public float PatchScale = DefaultPatchScale;

        public float HeightScale = DefaultHeightScale;

        public Vector3 TerrainOffset;

        public Matrix View;

        public Matrix Projection;

        public Vector3 EyePosition;

        public Texture2D HeightMapTexture;

        Settings settings;

        SelectedNode[] selectedNodes;

        public VisibilityRanges VisibilityRanges { get; private set; }

        public BoundingFrustum Frustum { get; private set; }

        public int SelectedNodeCount { get; private set; }

        public Vector3 TerrainScale
        {
            get
            {
                return new Vector3
                {
                    X = (HeightMapTexture.Width - 1) * PatchScale,
                    Y = HeightScale,
                    Z = (HeightMapTexture.Height - 1) * PatchScale
                };
            }
        }

        public Selection(Settings settings, VisibilityRanges visibilityRanges)
        {
            this.settings = settings;
            VisibilityRanges = visibilityRanges;

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
            sphere = new BoundingSphere(EyePosition, VisibilityRanges[level]);
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
            instance.Offset.X = selectedNodes[index].X * PatchScale + TerrainOffset.X;
            instance.Offset.Y = selectedNodes[index].Y * PatchScale + TerrainOffset.Z;
            instance.Scale = selectedNodes[index].Size * PatchScale;
            instance.Level = selectedNodes[index].Level;
        }
    }
}
