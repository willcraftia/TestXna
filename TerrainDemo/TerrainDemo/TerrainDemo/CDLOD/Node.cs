#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

#endregion

namespace TerrainDemo.CDLOD
{
    public sealed class Node
    {
        int x;

        int y;

        float minHeight;

        float maxHeight;

        int size;

        int level;

        Node childTopLeft;

        Node childTopRight;

        Node childBottomLeft;

        Node childBottomRight;

        public int X
        {
            get { return x; }
        }

        public int Y
        {
            get { return y; }
        }

        public float MinHeight
        {
            get { return minHeight; }
        }

        public float MaxHeight
        {
            get { return maxHeight; }
        }

        public int Size
        {
            get { return size; }
        }

        public int Level
        {
            get { return level; }
        }

        public Node(int x, int y, int size, ref CreateDescription createDescription)
        {
            this.x = x;
            this.y = y;
            this.size = size;

            if (size == createDescription.LeafNodeSize)
            {
                int limitX = Math.Min(createDescription.HeightMap.Width, x + size + 1);
                int limitY = Math.Min(createDescription.HeightMap.Height, y + size + 1);

                // this is a leaf node.
                createDescription.HeightMap.GetAreaMinMaxHeight(x, y, limitX - x, limitY - y, out minHeight, out maxHeight);
            }
            else
            {
                int childSize = size / 2;

                childTopLeft = new Node(x, y, childSize, ref createDescription);
                minHeight = childTopLeft.minHeight;
                maxHeight = childTopLeft.maxHeight;

                if (x + childSize < createDescription.HeightMap.Width - 1)
                {
                    childTopRight = new Node(x + childSize, y, childSize, ref createDescription);
                    minHeight = MathHelper.Min(minHeight, childTopRight.minHeight);
                    maxHeight = MathHelper.Max(maxHeight, childTopRight.maxHeight);
                }

                if (y + childSize < createDescription.HeightMap.Height - 1)
                {
                    childBottomLeft = new Node(x, y + childSize, childSize, ref createDescription);
                    minHeight = MathHelper.Min(minHeight, childBottomLeft.minHeight);
                    maxHeight = MathHelper.Max(maxHeight, childBottomLeft.maxHeight);
                }

                if (x + childSize < createDescription.HeightMap.Width - 1 &&
                    y + childSize < createDescription.HeightMap.Height - 1)
                {
                    childBottomRight = new Node(x + childSize, y + childSize, childSize, ref createDescription);
                    minHeight = MathHelper.Min(minHeight, childBottomRight.minHeight);
                    maxHeight = MathHelper.Max(maxHeight, childBottomRight.maxHeight);
                }

                level = childTopLeft.level + 1;
            }
        }

        public bool Select(Selection selection, bool parentCompletelyInFrustum, bool ignoreVisibilityCheck)
        {
            BoundingBox boundingBox;
            GetBoundingBox(ref selection.TerrainOffset, selection.PatchScale, selection.HeightScale, out boundingBox);

            ContainmentType containmentType = ContainmentType.Contains;
            if (!parentCompletelyInFrustum)
            {
                selection.Frustum.Contains(ref boundingBox, out containmentType);
            }

            BoundingSphere sphere;
            bool intersected = true;

            if (!ignoreVisibilityCheck)
            {
                selection.GetVisibilitySphere(level, out sphere);
                boundingBox.Intersects(ref sphere, out intersected);
                if (!intersected)
                    return false;
            }

            if (level == 0)
            {
                // we reach a leaf node.
                if (containmentType != ContainmentType.Disjoint)
                    selection.AddSelectedNode(this);
                return true;
            }

            // If this node is out of the next visibility, we do not need to check children.
            selection.GetVisibilitySphere(level - 1, out sphere);
            boundingBox.Intersects(ref sphere, out intersected);
            if (!intersected)
            {
                if (containmentType != ContainmentType.Disjoint)
                    selection.AddSelectedNode(this);
                return true;
            }

            bool weAreCompletelyInFrustum = (containmentType == ContainmentType.Contains);

            // Check a child node's visibility on ahead.
            var someChildrenSelected = false;
            someChildrenSelected |= childTopLeft.PreSelect(selection, weAreCompletelyInFrustum);
            if (childTopRight != null)
                someChildrenSelected |= childTopRight.PreSelect(selection, weAreCompletelyInFrustum);
            if (childBottomLeft != null)
                someChildrenSelected |= childBottomLeft.PreSelect(selection, weAreCompletelyInFrustum);
            if (childBottomRight != null)
                someChildrenSelected |= childBottomRight.PreSelect(selection, weAreCompletelyInFrustum);

            if (someChildrenSelected)
            {
                // Select all children to avoid T-junctions by ignoring a visibiliy range check
                // if can select at least one.
                // The original code tries to select finer nodes as far as possible,
                // and hides parts of a coaser node overlapped by them at render time.
                // But using HW instancing, we must use a same mesh, so can not use such a overlap.
                childTopLeft.Select(selection, weAreCompletelyInFrustum, true);
                if (childTopRight != null)
                    childTopRight.Select(selection, weAreCompletelyInFrustum, true);
                if (childBottomLeft != null)
                    childBottomLeft.Select(selection, weAreCompletelyInFrustum, true);
                if (childBottomRight != null)
                    childBottomRight.Select(selection, weAreCompletelyInFrustum, true);
            }
            else
            {
                if (containmentType != ContainmentType.Disjoint)
                    selection.AddSelectedNode(this);
            }

            return true;
        }

        bool PreSelect(Selection selection, bool parentCompletelyInFrustum)
        {
            BoundingBox boundingBox;
            GetBoundingBox(ref selection.TerrainOffset, selection.PatchScale, selection.HeightScale, out boundingBox);

            // do not check the intersection between AABB and the view frustum.

            BoundingSphere sphere;
            selection.GetVisibilitySphere(level, out sphere);
            if (!boundingBox.Intersects(sphere))
                return false;

            return true;
        }

        void GetBoundingBox(ref Vector3 terrainOffset, float patchScale, float heightScale, out BoundingBox boundingBox)
        {
            boundingBox = new BoundingBox();

            boundingBox.Min.X = x * patchScale + terrainOffset.X;
            boundingBox.Min.Y = minHeight * heightScale + terrainOffset.Y;
            boundingBox.Min.Z = y * patchScale + terrainOffset.Z;
            
            boundingBox.Max.X = (x + size) * patchScale + terrainOffset.X;
            boundingBox.Max.Y = maxHeight * heightScale + terrainOffset.Y;
            boundingBox.Max.Z = (y + size) * patchScale + terrainOffset.Z;
        }

        // TODO: I want to use the intersection between an AABB and a frustum.
        void GetBoundingSphere(ref Vector3 terrainOffset, float patchScale, float heightScale, out BoundingSphere sphere)
        {
            var min = new Vector3
            {
                X = x * patchScale + terrainOffset.X,
                Y = minHeight * heightScale + terrainOffset.Y,
                Z = y * patchScale + terrainOffset.Z
            };
            var max = new Vector3
            {
                X = (x + size) * patchScale + terrainOffset.X,
                Y = maxHeight * heightScale + terrainOffset.Y,
                Z = (y + size) * patchScale + terrainOffset.Z
            };
            var center = max - min;
            var radius = center.Length() * 0.5f;
            sphere = new BoundingSphere(center, radius);
        }
    }
}
