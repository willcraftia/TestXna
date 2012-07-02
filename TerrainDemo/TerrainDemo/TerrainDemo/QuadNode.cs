#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace TerrainDemo
{
    public sealed class QuadNode
    {
        QuadNode parent;

        QuadTree parentTree;

        int positionIndex;

        int nodeDepth;

        int nodeSize;

        bool isActive;

        bool isSplit;

        public QuadNodeVertex VertexTopLeft;
        public QuadNodeVertex VertexTop;
        public QuadNodeVertex VertexTopRight;
        public QuadNodeVertex VertexLeft;
        public QuadNodeVertex VertexCenter;
        public QuadNodeVertex VertexRight;
        public QuadNodeVertex VertexBottomLeft;
        public QuadNodeVertex VertexBottom;
        public QuadNodeVertex VertexBottomRight;

        public QuadNode ChildTopLeft;
        public QuadNode ChildTopRight;
        public QuadNode ChildBottomLeft;
        public QuadNode ChildBottomRight;

        public QuadNode NeighborTop;
        public QuadNode NeighborBottom;
        public QuadNode NeighborLeft;
        public QuadNode NeighborRight;

        public BoundingBox Bounds;

        public QuadNodeType NodeType { get; private set; }

        public bool HasChildren { get; private set; }

        public QuadNode(QuadNodeType nodeType, int nodeSize, int nodeDepth, QuadNode parent, QuadTree parentTree, int positionIndex)
        {
            NodeType = nodeType;
            this.nodeSize = nodeSize;
            this.nodeDepth = nodeDepth;
            this.parent = parent;
            this.parentTree = parentTree;
            this.positionIndex = positionIndex;

            AddVertices();

            Bounds = new BoundingBox
            {
                Min = parentTree.TreeVertices.Vertices[VertexTopLeft.Index].Position,
                Max = parentTree.TreeVertices.Vertices[VertexBottomRight.Index].Position
            };
            Bounds.Min.Y = -950;
            Bounds.Max.Y = 950;

            if (4 <= nodeSize) AddChildren();

            if (nodeDepth == 1)
            {
                AddNeighbors();

                VertexTopLeft.Activated = true;
                VertexTopRight.Activated = true;
                VertexCenter.Activated = true;
                VertexBottomLeft.Activated = true;
                VertexBottomRight.Activated = true;
            }
        }

        public void SetActiveVertices()
        {
            if (isSplit && HasChildren)
            {
                ChildTopLeft.SetActiveVertices();
                ChildTopRight.SetActiveVertices();
                ChildBottomLeft.SetActiveVertices();
                ChildBottomRight.SetActiveVertices();
                return;
            }

            parentTree.UpdateBuffer(VertexCenter.Index);
            parentTree.UpdateBuffer(VertexTopLeft.Index);

            if (VertexTop.Activated)
            {
                parentTree.UpdateBuffer(VertexTop.Index);
                parentTree.UpdateBuffer(VertexCenter.Index);
                parentTree.UpdateBuffer(VertexTop.Index);
            }
            parentTree.UpdateBuffer(VertexTopRight.Index);

            parentTree.UpdateBuffer(VertexCenter.Index);
            parentTree.UpdateBuffer(VertexTopRight.Index);

            if (VertexRight.Activated)
            {
                parentTree.UpdateBuffer(VertexRight.Index);
                parentTree.UpdateBuffer(VertexCenter.Index);
                parentTree.UpdateBuffer(VertexRight.Index);
            }
            parentTree.UpdateBuffer(VertexBottomRight.Index);

            parentTree.UpdateBuffer(VertexCenter.Index);
            parentTree.UpdateBuffer(VertexBottomRight.Index);

            if (VertexBottom.Activated)
            {
                parentTree.UpdateBuffer(VertexBottom.Index);
                parentTree.UpdateBuffer(VertexCenter.Index);
                parentTree.UpdateBuffer(VertexBottom.Index);
            }
            parentTree.UpdateBuffer(VertexBottomLeft.Index);

            parentTree.UpdateBuffer(VertexCenter.Index);
            parentTree.UpdateBuffer(VertexBottomLeft.Index);

            if (VertexLeft.Activated)
            {
                parentTree.UpdateBuffer(VertexLeft.Index);
                parentTree.UpdateBuffer(VertexCenter.Index);
                parentTree.UpdateBuffer(VertexLeft.Index);
            }
            parentTree.UpdateBuffer(VertexTopLeft.Index);
        }

        public void Activate()
        {
            VertexTopLeft.Activated = true;
            VertexTopRight.Activated = true;
            VertexCenter.Activated = true;
            VertexBottomLeft.Activated = true;
            VertexBottomRight.Activated = true;

            isActive = true;
        }

        public void EnforceMinimumDepth()
        {
            if (nodeDepth < parentTree.MinimumDepth)
            {
                if (HasChildren)
                {
                    isActive = false;
                    isSplit = true;

                    ChildTopLeft.EnforceMinimumDepth();
                    ChildTopRight.EnforceMinimumDepth();
                    ChildBottomLeft.EnforceMinimumDepth();
                    ChildBottomRight.EnforceMinimumDepth();
                }
                else
                {
                    Activate();
                    isSplit = false;
                }

                return;
            }

            if (nodeDepth == parentTree.MinimumDepth || (nodeDepth < parentTree.MinimumDepth && !HasChildren))
            {
                Activate();
                isSplit = false;
            }
        }

        void AddVertices()
        {
            switch (NodeType)
            {
                case QuadNodeType.TopLeft:
                    VertexTopLeft = parent.VertexTopLeft;
                    VertexTopRight = parent.VertexTop;
                    VertexBottomLeft = parent.VertexLeft;
                    VertexBottomRight = parent.VertexCenter;
                    break;
                case QuadNodeType.TopRight:
                    VertexTopLeft = parent.VertexTop;
                    VertexTopRight = parent.VertexTopRight;
                    VertexBottomLeft = parent.VertexCenter;
                    VertexBottomRight = parent.VertexRight;
                    break;
                case QuadNodeType.BottomLeft:
                    VertexTopLeft = parent.VertexLeft;
                    VertexTopRight = parent.VertexCenter;
                    VertexBottomLeft = parent.VertexBottomLeft;
                    VertexBottomRight = parent.VertexBottom;
                    break;
                case QuadNodeType.BottomRight:
                    VertexTopLeft = parent.VertexCenter;
                    VertexTopRight = parent.VertexRight;
                    VertexBottomLeft = parent.VertexBottom;
                    VertexBottomRight = parent.VertexBottomRight;
                    break;
                default:
                    VertexTopLeft = new QuadNodeVertex
                    {
                        Activated = true,
                        Index = 0
                    };
                    VertexTopRight = new QuadNodeVertex
                    {
                        Activated = true,
                        Index = VertexTopLeft.Index + nodeSize
                    };
                    VertexBottomLeft = new QuadNodeVertex
                    {
                        Activated = true,
                        Index = (parentTree.TopNodeSize + 1) * parentTree.TopNodeSize
                    };
                    VertexBottomRight = new QuadNodeVertex
                    {
                        Activated = true,
                        Index = VertexBottomLeft.Index + nodeSize
                    };
                    break;
            }

            VertexTop = new QuadNodeVertex
            {
                Activated = false,
                Index = VertexTopLeft.Index + (nodeSize / 2)
            };
            VertexLeft = new QuadNodeVertex
            {
                Activated = false,
                Index = VertexTopLeft.Index + (parentTree.TopNodeSize + 1) * (nodeSize / 2)
            };
            VertexCenter = new QuadNodeVertex
            {
                Activated = false,
                Index = VertexLeft.Index + (nodeSize / 2)
            };
            VertexRight = new QuadNodeVertex
            {
                Activated = false,
                Index = VertexLeft.Index + nodeSize
            };
            VertexBottom = new QuadNodeVertex
            {
                Activated = false,
                Index = VertexBottomLeft.Index + (nodeSize / 2)
            };
        }

        void AddChildren()
        {
            ChildTopLeft = new QuadNode(QuadNodeType.TopLeft, nodeSize / 2, nodeDepth + 1, this, parentTree, VertexTopLeft.Index);
            ChildTopRight = new QuadNode(QuadNodeType.TopRight, nodeSize / 2, nodeDepth + 1, this, parentTree, VertexTop.Index);
            ChildBottomLeft = new QuadNode(QuadNodeType.BottomLeft, nodeSize / 2, nodeDepth + 1, this, parentTree, VertexLeft.Index);
            ChildBottomRight = new QuadNode(QuadNodeType.BottomRight, nodeSize / 2, nodeDepth + 1, this, parentTree, VertexCenter.Index);

            HasChildren = true;
        }

        void AddNeighbors()
        {
            switch (NodeType)
            {
                case QuadNodeType.TopLeft:
                    if (parent.NeighborTop != null) NeighborTop = parent.NeighborTop.ChildBottomLeft;
                    NeighborRight = parent.ChildTopRight;
                    NeighborBottom = parent.ChildBottomLeft;
                    if (parent.NeighborLeft != null) NeighborLeft = parent.NeighborLeft.ChildTopRight;
                    break;
                case QuadNodeType.TopRight:
                    if (parent.NeighborTop != null) NeighborTop = parent.NeighborTop.ChildBottomRight;
                    if (parent.NeighborRight != null) NeighborRight = parent.NeighborRight.ChildTopLeft;
                    NeighborBottom = parent.ChildBottomRight;
                    NeighborLeft = parent.ChildTopLeft;
                    break;
                case QuadNodeType.BottomLeft:
                    NeighborTop = parent.ChildTopLeft;
                    NeighborRight = parent.ChildBottomRight;
                    if (parent.NeighborBottom != null) NeighborBottom = parent.NeighborBottom.ChildTopLeft;
                    if (parent.NeighborLeft != null) NeighborLeft = parent.NeighborLeft.ChildBottomRight;
                    break;
                case QuadNodeType.BottomRight:
                    NeighborTop = parent.ChildTopRight;
                    if (parent.NeighborRight != null) NeighborRight = parent.NeighborRight.ChildBottomLeft;
                    if (parent.NeighborBottom != null) NeighborBottom = parent.NeighborBottom.ChildTopRight;
                    NeighborLeft = parent.ChildBottomLeft;
                    break;
            }

            if (HasChildren)
            {
                ChildTopLeft.AddNeighbors();
                ChildTopRight.AddNeighbors();
                ChildBottomLeft.AddNeighbors();
                ChildBottomRight.AddNeighbors();
            }
        }
    }
}
