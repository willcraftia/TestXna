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

                // リーフ ノード (Level = 0)
                createDescription.HeightMap.GetAreaMinMaxHeight(x, y, limitX - x, limitY - y, out minHeight, out maxHeight);
            }
            else
            {
                int childSize = size / 2;

                childTopLeft = new Node(x, y, childSize, ref createDescription);
                minHeight = childTopLeft.minHeight;
                maxHeight = childTopLeft.maxHeight;

                childTopRight = new Node(x + childSize, y, childSize, ref createDescription);
                minHeight = MathHelper.Min(minHeight, childTopRight.minHeight);
                maxHeight = MathHelper.Max(maxHeight, childTopRight.maxHeight);

                childBottomLeft = new Node(x, y + childSize, childSize, ref createDescription);
                minHeight = MathHelper.Min(minHeight, childBottomLeft.minHeight);
                maxHeight = MathHelper.Max(maxHeight, childBottomLeft.maxHeight);

                childBottomRight = new Node(x + childSize, y + childSize, childSize, ref createDescription);
                minHeight = MathHelper.Min(minHeight, childBottomRight.minHeight);
                maxHeight = MathHelper.Max(maxHeight, childBottomRight.maxHeight);

                level = childTopLeft.level + 1;
            }
        }

        public bool Select(Selection selection, bool parentCompletelyInFrustum)
        {
            BoundingBox boundingBox;
            GetBoundingBox(ref selection.TerrainOffset, selection.PatchScale, selection.HeightScale, out boundingBox);

            ContainmentType containmentType = ContainmentType.Contains;
            if (!parentCompletelyInFrustum)
            {
                selection.Frustum.Contains(ref boundingBox, out containmentType);
            }

            var visibilityRange = selection.Morph.GetVisibilityRange(level);
            var sphere = new BoundingSphere(selection.EyePosition, visibilityRange);
            if (!boundingBox.Intersects(sphere))
                return false;

            if (level == 0)
            {
                // リーフ ノードに到達。
                if (containmentType != ContainmentType.Disjoint)
                    selection.AddSelectedNode(this);
                return true;
            }

            // 次に詳細な LOD の範囲を含まないならば、子を調べる必要はなく、自分を選択。
            var childVisibilityRange = selection.Morph.GetVisibilityRange(level - 1);
            if (!boundingBox.Intersects(new BoundingSphere(selection.EyePosition, childVisibilityRange)))
            {
                if (containmentType != ContainmentType.Disjoint)
                    selection.AddSelectedNode(this);
                return true;
            }

            bool weAreCompletelyInFrustum = (containmentType == ContainmentType.Contains);

            // 子ノードの選択を試行 (ここでは SelectedNode の追加を行わずに選択可能性のみを検査)。
            var allChildrenSelected = true;
            allChildrenSelected &= childTopLeft.PreSelect(selection, weAreCompletelyInFrustum);
            allChildrenSelected &= childTopRight.PreSelect(selection, weAreCompletelyInFrustum);
            allChildrenSelected &= childBottomLeft.PreSelect(selection, weAreCompletelyInFrustum);
            allChildrenSelected &= childBottomRight.PreSelect(selection, weAreCompletelyInFrustum);

            if (allChildrenSelected)
            {
                // 全ての子ノードが選択される場合にのみ、子ノードの LOD を採用する。
                childTopLeft.Select(selection, weAreCompletelyInFrustum);
                childTopRight.Select(selection, weAreCompletelyInFrustum);
                childBottomLeft.Select(selection, weAreCompletelyInFrustum);
                childBottomRight.Select(selection, weAreCompletelyInFrustum);
            }
            else
            {
                // それ以外の場合は、自ノードの LOD を採用する。
                // 一部の子ノードのみが選択可能となる状況は、LOD レベルの境界でのみ発生する。
                // LOD レベル境界における子ノードの詳細さは、
                // 自ノードと同程度までモーフィングされていると考えられる。
                // つまり、子ノードの選択を無視し、自ノードの選択としてしまっても、
                // 視覚上での違和感は発生しないと考えられる。
                // なお、オリジナル ソースでは、可能な限り子ノードの LOD を採用するために、
                // メッシュの描画時に子ノードで描画される範囲を描画しないように調整している。
                // ただし、その方法では、ノード毎にメッシュの形状が変化するため、
                // 各ノード毎に描画範囲を検査しながら描画する必要がある。
                // 一方、ここでの方法では、全ノードが同型のメッシュとなるため、
                // HW インスタンシングによる恩恵を受けることができる。
                if (containmentType != ContainmentType.Disjoint)
                    selection.AddSelectedNode(this);
            }

            return true;
        }

        bool PreSelect(Selection selection, bool parentCompletelyInFrustum)
        {
            BoundingBox boundingBox;
            GetBoundingBox(ref selection.TerrainOffset, selection.PatchScale, selection.HeightScale, out boundingBox);

            //ContainmentType containmentType = ContainmentType.Contains;
            //if (!parentCompletelyInFrustum)
            //{
            //    selection.Frustum.Contains(ref boundingBox, out containmentType);
            //    if (containmentType == ContainmentType.Disjoint) return false;
            //}

            var visibilityRange = selection.Morph.GetVisibilityRange(level);
            var sphere = new BoundingSphere(selection.EyePosition, visibilityRange);
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

        // TODO: できれば BoundingBox と BoundingFrustum の交差判定の前に球同士で判定したい。
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
