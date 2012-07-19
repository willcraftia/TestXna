#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace TiledTerrainDemo.CDLOD
{
    public sealed class Selection
    {
        public const int MaxSelectedNodeCount = 4000;

        public Vector3 TerrainOffset;

        public Matrix View;

        public Matrix Projection;

        public Vector3 EyePosition;

        public Texture2D HeightMapTexture;

        Settings settings;

        SelectedNode[] selectedNodes;

        public IVisibleRanges VisibleRanges { get; private set; }

        public BoundingFrustum Frustum { get; private set; }

        public int SelectedNodeCount { get; private set; }

        public float PatchScale
        {
            get { return settings.PatchScale; }
        }

        public float HeightScale
        {
            get { return settings.HeightScale; }
        }

        public Vector3 TerrainScale
        {
            get
            {
                return new Vector3
                {
                    X = (settings.HeightMapWidth - 1) * settings.PatchScale,
                    Y = settings.HeightScale,
                    Z = (settings.HeightMapHeight - 1) * settings.PatchScale
                };
            }
        }

        public Selection(Settings settings, IVisibleRanges visibleRanges)
        {
            this.settings = settings;
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
            instance.Offset.X = selectedNodes[index].X * settings.PatchScale + TerrainOffset.X;
            instance.Offset.Y = selectedNodes[index].Y * settings.PatchScale + TerrainOffset.Z;
            instance.Scale = selectedNodes[index].Size * settings.PatchScale;
            instance.Level = selectedNodes[index].Level;
        }
    }
}
