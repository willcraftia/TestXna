#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

#endregion

namespace TiledTerrainDemo.CDLOD
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

        public Node(int x, int y, int size, ref CDLODSettings settings)
        {
            this.x = x;
            this.y = y;
            this.size = size;

            if (settings.LeafNodeSize < size)
            {
                // if not a leaf node, then create child nodes.

                int childSize = size / 2;
                childTopLeft = new Node(x, y, childSize, ref settings);
                level = childTopLeft.level + 1;

                if (x + childSize < settings.HeightMapWidth - 1)
                {
                    childTopRight = new Node(x + childSize, y, childSize, ref settings);
                }

                if (y + childSize < settings.HeightMapHeight - 1)
                {
                    childBottomLeft = new Node(x, y + childSize, childSize, ref settings);
                }

                if (x + childSize < settings.HeightMapWidth - 1 &&
                    y + childSize < settings.HeightMapHeight - 1)
                {
                    childBottomRight = new Node(x + childSize, y + childSize, childSize, ref settings);
                }
            }
        }

        public void Build(ICDLODHeightMap heightMap)
        {
            if (level == 0)
            {
                // a leaf node.
                int limitX = Math.Min(heightMap.Width, x + size + 1);
                int limitY = Math.Min(heightMap.Height, y + size + 1);
                heightMap.GetAreaMinMaxHeight(x, y, limitX - x, limitY - y, out minHeight, out maxHeight);
            }
            else
            {
                childTopLeft.Build(heightMap);
                minHeight = childTopLeft.minHeight;
                maxHeight = childTopLeft.maxHeight;

                if (childTopRight != null)
                {
                    childTopRight.Build(heightMap);
                    minHeight = MathHelper.Min(minHeight, childTopRight.minHeight);
                    maxHeight = MathHelper.Max(maxHeight, childTopRight.maxHeight);
                }

                if (childBottomLeft != null)
                {
                    childBottomLeft.Build(heightMap);
                    minHeight = MathHelper.Min(minHeight, childBottomLeft.minHeight);
                    maxHeight = MathHelper.Max(maxHeight, childBottomLeft.maxHeight);
                }

                if (childBottomRight != null)
                {
                    childBottomRight.Build(heightMap);
                    minHeight = MathHelper.Min(minHeight, childBottomRight.minHeight);
                    maxHeight = MathHelper.Max(maxHeight, childBottomRight.maxHeight);
                }
            }
        }

        public bool Select(CDLODSelection selection, bool parentCompletelyInFrustum, bool ignoreVisibilityCheck)
        {
            BoundingBox boundingBox;
            GetBoundingBox(ref selection.TerrainOffset, selection.Settings.PatchScale, selection.Settings.HeightScale, out boundingBox);

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

        bool PreSelect(CDLODSelection selection, bool parentCompletelyInFrustum)
        {
            BoundingBox boundingBox;
            GetBoundingBox(ref selection.TerrainOffset, selection.Settings.PatchScale, selection.Settings.HeightScale, out boundingBox);

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
