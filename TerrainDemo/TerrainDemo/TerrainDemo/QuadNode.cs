#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace TerrainDemo
{
    public sealed class QuadNode
    {
        public QuadNode Parent { get; private set; }

        QuadTree parentTree;

        int positionIndex;

        int nodeDepth;

        int nodeSize;

        public bool IsActive { get; set; }

        public bool IsSplit { get; private set; }

        public bool CanSplit
        {
            get { return 2 <= nodeSize; }
        }

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

        public BoundingRect Bounds { get; private set; }

        public QuadNodeType NodeType { get; private set; }

        public bool HasChildren { get; private set; }

        public bool IsInView
        {
            get { return Bounds.Intersects(parentTree.ClipShape); }
        }

        public QuadNode(QuadNodeType nodeType, int nodeSize, int nodeDepth, QuadNode parent, QuadTree parentTree, int positionIndex)
        {
            NodeType = nodeType;
            this.nodeSize = nodeSize;
            this.nodeDepth = nodeDepth;
            Parent = parent;
            this.parentTree = parentTree;
            this.positionIndex = positionIndex;

            AddVertices();

            var topLeft = new Vector2
            {
                X = parentTree.TreeVertices.Vertices[VertexTopLeft.Index].Position.X,
                Y = parentTree.TreeVertices.Vertices[VertexTopLeft.Index].Position.Z
            };
            var bottomRight = new Vector2
            {
                X = parentTree.TreeVertices.Vertices[VertexBottomRight.Index].Position.X,
                Y = parentTree.TreeVertices.Vertices[VertexBottomRight.Index].Position.Z
            };
            Bounds = BoundingRect.FromPoints(topLeft, bottomRight);

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
            if (parentTree.Cull && !IsInView) return;

            if (IsSplit && HasChildren)
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

            IsActive = true;
        }

        public void EnforceMinimumDepth()
        {
            if (nodeDepth < parentTree.MinimumDepth)
            {
                if (HasChildren)
                {
                    IsActive = false;
                    IsSplit = true;

                    ChildTopLeft.EnforceMinimumDepth();
                    ChildTopRight.EnforceMinimumDepth();
                    ChildBottomLeft.EnforceMinimumDepth();
                    ChildBottomRight.EnforceMinimumDepth();
                }
                else
                {
                    Activate();
                    IsSplit = false;
                }

                return;
            }

            if (nodeDepth == parentTree.MinimumDepth || (nodeDepth < parentTree.MinimumDepth && !HasChildren))
            {
                Activate();
                IsSplit = false;
            }
        }

        public bool Contains(Vector2 point)
        {
            return point.X >= Bounds.Point1.X && point.X <= Bounds.Point3.X &&
                point.Y >= Bounds.Point1.Y && point.Y <= Bounds.Point3.Y;
        }

        public QuadNode GetDeepestNodeWithPoint(Vector2 point)
        {
            if (!Contains(point)) return null;

            if (!HasChildren) return this;

            if (ChildTopLeft.Contains(point)) return ChildTopLeft.GetDeepestNodeWithPoint(point);
            if (ChildTopRight.Contains(point)) return ChildTopRight.GetDeepestNodeWithPoint(point);
            if (ChildBottomLeft.Contains(point)) return ChildBottomLeft.GetDeepestNodeWithPoint(point);
            return ChildBottomRight.GetDeepestNodeWithPoint(point);
        }

        public void Split()
        {
            if (parentTree.Cull && !IsInView) return;

            if (Parent != null && !Parent.IsSplit) Parent.Split();

            if (CanSplit)
            {
                if (HasChildren)
                {
                    ChildTopLeft.Activate();
                    ChildTopRight.Activate();
                    ChildBottomLeft.Activate();
                    ChildBottomRight.Activate();

                    IsActive = false;
                }
                else
                {
                    IsActive = true;
                }

                IsSplit = true;

                VertexTop.Activated = true;
                VertexLeft.Activated = true;
                VertexRight.Activated = true;
                VertexBottom.Activated = true;
            }

            EnsureNeighborParentSplit(NeighborTop);
            EnsureNeighborParentSplit(NeighborRight);
            EnsureNeighborParentSplit(NeighborBottom);
            EnsureNeighborParentSplit(NeighborLeft);

            if (NeighborTop != null) NeighborTop.VertexBottom.Activated = true;
            if (NeighborRight != null) NeighborRight.VertexLeft.Activated = true;
            if (NeighborBottom != null) NeighborBottom.VertexTop.Activated = true;
            if (NeighborLeft != null) NeighborLeft.VertexRight.Activated = true;
        }

        public void Merge()
        {
            VertexTop.Activated = false;
            VertexLeft.Activated = false;
            VertexRight.Activated = false;
            VertexBottom.Activated = false;

            if (NodeType != QuadNodeType.FullNode)
            {
                VertexTopLeft.Activated = false;
                VertexTopRight.Activated = false;
                VertexBottomLeft.Activated = false;
                VertexBottomRight.Activated = false;
            }

            IsActive = true;
            IsSplit = false;

            if (HasChildren)
            {
                if (ChildTopLeft.IsSplit)
                {
                    ChildTopLeft.Merge();
                    ChildTopLeft.IsActive = false;
                }
                else
                {
                    ChildTopLeft.VertexTop.Activated = false;
                    ChildTopLeft.VertexLeft.Activated = false;
                    ChildTopLeft.VertexRight.Activated = false;
                    ChildTopLeft.VertexBottom.Activated = false;
                }

                if (ChildTopRight.IsSplit)
                {
                    ChildTopRight.Merge();
                    ChildTopRight.IsActive = false;
                }
                else
                {
                    ChildTopRight.VertexTop.Activated = false;
                    ChildTopRight.VertexLeft.Activated = false;
                    ChildTopRight.VertexRight.Activated = false;
                    ChildTopRight.VertexBottom.Activated = false;
                }

                if (ChildBottomLeft.IsSplit)
                {
                    ChildBottomLeft.Merge();
                    ChildBottomLeft.IsActive = false;
                }
                else
                {
                    ChildBottomLeft.VertexTop.Activated = false;
                    ChildBottomLeft.VertexLeft.Activated = false;
                    ChildBottomLeft.VertexRight.Activated = false;
                    ChildBottomLeft.VertexBottom.Activated = false;
                }

                if (ChildBottomRight.IsSplit)
                {
                    ChildBottomRight.Merge();
                    ChildBottomRight.IsActive = false;
                }
                else
                {
                    ChildBottomRight.VertexTop.Activated = false;
                    ChildBottomRight.VertexLeft.Activated = false;
                    ChildBottomRight.VertexRight.Activated = false;
                    ChildBottomRight.VertexBottom.Activated = false;
                }
            }
        }

        void EnsureNeighborParentSplit(QuadNode neighbor)
        {
            if (neighbor != null && neighbor.Parent != null && !neighbor.Parent.IsSplit)
                neighbor.Parent.Split();
        }

        void AddVertices()
        {
            switch (NodeType)
            {
                case QuadNodeType.TopLeft:
                    VertexTopLeft = Parent.VertexTopLeft;
                    VertexTopRight = Parent.VertexTop;
                    VertexBottomLeft = Parent.VertexLeft;
                    VertexBottomRight = Parent.VertexCenter;
                    break;
                case QuadNodeType.TopRight:
                    VertexTopLeft = Parent.VertexTop;
                    VertexTopRight = Parent.VertexTopRight;
                    VertexBottomLeft = Parent.VertexCenter;
                    VertexBottomRight = Parent.VertexRight;
                    break;
                case QuadNodeType.BottomLeft:
                    VertexTopLeft = Parent.VertexLeft;
                    VertexTopRight = Parent.VertexCenter;
                    VertexBottomLeft = Parent.VertexBottomLeft;
                    VertexBottomRight = Parent.VertexBottom;
                    break;
                case QuadNodeType.BottomRight:
                    VertexTopLeft = Parent.VertexCenter;
                    VertexTopRight = Parent.VertexRight;
                    VertexBottomLeft = Parent.VertexBottom;
                    VertexBottomRight = Parent.VertexBottomRight;
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
                    if (Parent.NeighborTop != null) NeighborTop = Parent.NeighborTop.ChildBottomLeft;
                    NeighborRight = Parent.ChildTopRight;
                    NeighborBottom = Parent.ChildBottomLeft;
                    if (Parent.NeighborLeft != null) NeighborLeft = Parent.NeighborLeft.ChildTopRight;
                    break;
                case QuadNodeType.TopRight:
                    if (Parent.NeighborTop != null) NeighborTop = Parent.NeighborTop.ChildBottomRight;
                    if (Parent.NeighborRight != null) NeighborRight = Parent.NeighborRight.ChildTopLeft;
                    NeighborBottom = Parent.ChildBottomRight;
                    NeighborLeft = Parent.ChildTopLeft;
                    break;
                case QuadNodeType.BottomLeft:
                    NeighborTop = Parent.ChildTopLeft;
                    NeighborRight = Parent.ChildBottomRight;
                    if (Parent.NeighborBottom != null) NeighborBottom = Parent.NeighborBottom.ChildTopLeft;
                    if (Parent.NeighborLeft != null) NeighborLeft = Parent.NeighborLeft.ChildBottomRight;
                    break;
                case QuadNodeType.BottomRight:
                    NeighborTop = Parent.ChildTopRight;
                    if (Parent.NeighborRight != null) NeighborRight = Parent.NeighborRight.ChildBottomLeft;
                    if (Parent.NeighborBottom != null) NeighborBottom = Parent.NeighborBottom.ChildTopRight;
                    NeighborLeft = Parent.ChildBottomLeft;
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
