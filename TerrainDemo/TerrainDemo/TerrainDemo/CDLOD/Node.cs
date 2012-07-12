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

        Node parent;

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

        public Node(Node parent, int x, int y, int size, IHeightMapSource heightMap)
        {
            this.parent = parent;
            this.x = x;
            this.y = y;
            this.size = size;

            if (4 <= size)
            {
                int childSize = size / 2;

                childTopLeft = new Node(this, x, y, childSize, heightMap);
                minHeight = childTopLeft.minHeight;
                maxHeight = childTopLeft.maxHeight;

                childTopRight = new Node(this, x + childSize, y, childSize, heightMap);
                minHeight = MathHelper.Min(minHeight, childTopRight.minHeight);
                maxHeight = MathHelper.Max(maxHeight, childTopRight.maxHeight);

                childBottomLeft = new Node(this, x, y + childSize, childSize, heightMap);
                minHeight = MathHelper.Min(minHeight, childBottomLeft.minHeight);
                maxHeight = MathHelper.Max(maxHeight, childBottomLeft.maxHeight);

                childBottomRight = new Node(this, x + childSize, y + childSize, childSize, heightMap);
                minHeight = MathHelper.Min(minHeight, childBottomRight.minHeight);
                maxHeight = MathHelper.Max(maxHeight, childBottomRight.maxHeight);

                level = childTopLeft.level + 1;
            }
            else
            {
                // リーフ ノード (Level = 0)
                heightMap.GetAreaMinMaxHeight(x, y, size, out minHeight, out maxHeight);
            }
        }

        public bool Select(Selection selection)
        {
            BoundingBox boundingBox;
            GetBoundingBox(ref selection.TerrainOffset, selection.PatchScale, selection.HeightScale, out boundingBox);

            var visibilityRange = selection.Morph.GetVisibilityRange(level);
            var sphere = new BoundingSphere(selection.EyePosition, visibilityRange);
            if (!boundingBox.Intersects(sphere))
                return false;

            if (level == 0)
            {
                // リーフ ノードに到達。
                selection.AddSelectedNode(this);
                return true;
            }

            // 次に詳細な LOD の範囲を含まないならば、子を調べる必要はなく、自分を選択。
            var childVisibilityRange = selection.Morph.GetVisibilityRange(level - 1);
            if (!boundingBox.Intersects(new BoundingSphere(selection.EyePosition, childVisibilityRange)))
            {
                selection.AddSelectedNode(this);
                return true;
            }

            // 子ノードの選択を試行 (ここでは SelectedNode の追加を行わずに選択可能性のみを検査)。
            var allChildrenSelected = true;
            allChildrenSelected &= childTopLeft.PreSelect(selection);
            allChildrenSelected &= childTopRight.PreSelect(selection);
            allChildrenSelected &= childBottomLeft.PreSelect(selection);
            allChildrenSelected &= childBottomRight.PreSelect(selection);

            if (allChildrenSelected)
            {
                // 全ての子ノードが選択される場合にのみ、子ノードの LOD を採用する。
                childTopLeft.Select(selection);
                childTopRight.Select(selection);
                childBottomLeft.Select(selection);
                childBottomRight.Select(selection);
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
                selection.AddSelectedNode(this);
            }

            return true;
        }

        bool PreSelect(Selection selection)
        {
            BoundingBox boundingBox;
            GetBoundingBox(ref selection.TerrainOffset, selection.PatchScale, selection.HeightScale, out boundingBox);

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
    }
}
