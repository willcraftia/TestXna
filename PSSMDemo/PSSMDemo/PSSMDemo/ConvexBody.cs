#region Using

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Xna.Framework;

#endregion

namespace PSSMDemo
{
    /// <summary>
    /// 凸体の構築とクリップを管理するクラスです。
    /// </summary>
    public sealed class ConvexBody
    {
        #region Polygon

        public sealed class Polygon
        {
            StructList<Vector3> vertices;

            public int VertexCount
            {
                get { return vertices.Count; }
            }

            internal Polygon()
            {
                vertices = new StructList<Vector3>(4);
            }

            public void GetVertex(int index, out Vector3 result)
            {
                vertices.GetItem(index, out result);
            }

            public void AddVertex(ref Vector3 vertex)
            {
                vertices.Add(ref vertex);
            }

            public void ClearVertices()
            {
                vertices.Clear();
            }
        }

        #endregion

        #region Edge

        struct Edge
        {
            public Vector3 Point0;

            public Vector3 Point1;

            public Edge(Vector3 point0, Vector3 point1)
            {
                Point0 = point0;
                Point1 = point1;
            }

            public override string ToString()
            {
                return "{Point0: " + Point0 + " Point1:" + Point1 + "}";
            }
        }

        #endregion

        #region PolygonCollection

        public sealed class PolygonCollection : Collection<Polygon>
        {
            ConvexBody convexBody;

            internal PolygonCollection(ConvexBody convexBody)
                : base(new List<Polygon>(6))
            {
                this.convexBody = convexBody;
            }

            protected override void RemoveItem(int index)
            {
                var item = Items[index];
                convexBody.ReleasePolygon(item);

                base.RemoveItem(index);
            }

            protected override void ClearItems()
            {
                foreach (var item in Items)
                {
                    convexBody.ReleasePolygon(item);
                }

                base.ClearItems();
            }

            internal void ClearWithoutReturnToPool()
            {
                base.ClearItems();
            }
        }

        #endregion

        // Ogre3d Vector3 と同じ値。
        const float EqualsPointTolerance = 1e-03f;

        // Ogre3d Vector3 と同じ値。
        // 即ち、Degree 1 の角度差ならば等しいベクトル方向であるとする。
        static readonly float DirectionEqualsTolerance = MathHelper.ToRadians(1);

        Vector3[] corners;

        List<bool> outsides;

        StructList<Edge> intersectEdges;

        Pool<Polygon> polygonPool;

        PolygonCollection workingPolygons;

        public PolygonCollection Polygons { get; private set; }

        public ConvexBody()
        {
            Polygons = new PolygonCollection(this);
            corners = new Vector3[8];
            outsides = new List<bool>(6);
            intersectEdges = new StructList<Edge>();
            polygonPool = new Pool<Polygon>(() => { return new Polygon(); });
            workingPolygons = new PolygonCollection(this);
        }

        /// <summary>
        /// 多角形プールからインスタンスを取得します。
        /// プールが空の場合には新たなインスタンスを生成します。
        /// </summary>
        /// <returns>
        /// ポリゴンが不要になった場合には、ReleasePolygon でプールへ戻す必要があります。
        /// </returns>
        public Polygon CreatePolygon()
        {
            var result = polygonPool.Borrow();

            Debug.Assert(result.VertexCount == 0, "A polygon may be shared unexpectedly.");

            return result;
        }

        /// <summary>
        /// 多角形をプールへ戻します。
        /// </summary>
        /// <param name="polygon">プールへ戻す多角形。</param>
        public void ReleasePolygon(Polygon polygon)
        {
            if (polygon == null) throw new ArgumentNullException("polygon");

            polygon.ClearVertices();
            polygonPool.Return(polygon);
        }

        /// <summary>
        /// 視錐台の頂点で凸体を構築します。
        /// </summary>
        /// <param name="frustum">視錐台。</param>
        public void Define(BoundingFrustum frustum)
        {
            Polygons.Clear();
            frustum.GetCorners(corners);

            // LiSPSM/Ogre (CCW) に合わせる。
            // 各々、配列インデックスと頂点の対応が異なる点に注意。

            // BoundingFrustum
            // 0: near-top-left
            // 1: near-top-right
            // 2: near-bottom-right
            // 3: near-bottom-left
            // 4: far-top-left
            // 5: far-top-right
            // 6: far-bottom-right
            // 7: far-bottom-left

            // LiSPSM : BoundingFrustum
            // 0: 3: near-bottom-left
            // 1: 2: near-bottom-right
            // 2: 1: near-top-right
            // 3: 0: near-top-left
            // 4: 7: far-bottom-left
            // 5: 6: far-bottom-right
            // 6: 5: far-top-right
            // 7: 4: far-top-left

            // Ogre : BoundingFrustum
            // 0: 1: near-top-right
            // 1: 0: near-top-left
            // 2: 3: near-bottom-left
            // 3: 2: near-bottom-right
            // 4: 5: far-top-right
            // 5: 4: far-top-left
            // 6: 7: far-bottom-left
            // 7: 6: far-bottom-right

            var near = CreatePolygon();
            near.AddVertex(ref corners[1]);
            near.AddVertex(ref corners[0]);
            near.AddVertex(ref corners[3]);
            near.AddVertex(ref corners[2]);
            Polygons.Add(near);

            var far = CreatePolygon();
            far.AddVertex(ref corners[4]);
            far.AddVertex(ref corners[5]);
            far.AddVertex(ref corners[6]);
            far.AddVertex(ref corners[7]);
            Polygons.Add(far);

            var left = CreatePolygon();
            left.AddVertex(ref corners[4]);
            left.AddVertex(ref corners[7]);
            left.AddVertex(ref corners[3]);
            left.AddVertex(ref corners[0]);
            Polygons.Add(left);

            var right = CreatePolygon();
            right.AddVertex(ref corners[5]);
            right.AddVertex(ref corners[1]);
            right.AddVertex(ref corners[2]);
            right.AddVertex(ref corners[6]);
            Polygons.Add(right);

            var bottom = CreatePolygon();
            bottom.AddVertex(ref corners[7]);
            bottom.AddVertex(ref corners[6]);
            bottom.AddVertex(ref corners[2]);
            bottom.AddVertex(ref corners[3]);
            Polygons.Add(bottom);

            var top = CreatePolygon();
            top.AddVertex(ref corners[5]);
            top.AddVertex(ref corners[4]);
            top.AddVertex(ref corners[0]);
            top.AddVertex(ref corners[1]);
            Polygons.Add(top);
        }

        /// <summary>
        /// 境界ボックスで凸体をクリップします。
        /// </summary>
        /// <param name="box">境界ボックス。</param>
        public void Clip(BoundingBox box)
        {
            // near
            Clip(PlaneHelper.CreatePlane(new Vector3(0, 0, 1), box.Max));
            // far
            Clip(PlaneHelper.CreatePlane(new Vector3(0, 0, -1), box.Min));
            // left
            Clip(PlaneHelper.CreatePlane(new Vector3(-1, 0, 0), box.Min));
            // right
            Clip(PlaneHelper.CreatePlane(new Vector3(1, 0, 0), box.Max));
            // bottom
            Clip(PlaneHelper.CreatePlane(new Vector3(0, -1, 0), box.Min));
            // top
            Clip(PlaneHelper.CreatePlane(new Vector3(0, 1, 0), box.Max));
        }

        /// <summary>
        /// 平面で凸体をクリップします。
        /// </summary>
        /// <param name="plane">平面。</param>
        public void Clip(Plane plane)
        {
            // 複製。
            for (int i = 0; i < Polygons.Count; i++)
                workingPolygons.Add(Polygons[i]);

            // 元を削除。
            Polygons.ClearWithoutReturnToPool();

            // オリジナル コードでは辺を Polygon クラスで扱っているが、
            // 見通しを良くするため Edge 構造体で管理。
            // ただし、途中のクリップ判定では、複数の交点を検出する可能性があるため、
            // 一度 Polygon クラスで頂点を集めた後、Edge へ変換している。
            intersectEdges.Clear();

            for (int ip = 0; ip < workingPolygons.Count; ip++)
            {
                var originalPolygon = workingPolygons[ip];
                if (originalPolygon.VertexCount < 3)
                    continue;

                var newPolygon = CreatePolygon();
                var intersectPolygon = CreatePolygon();

                Clip(ref plane, originalPolygon, newPolygon, intersectPolygon);

                if (3 <= newPolygon.VertexCount)
                {
                    // 面がある場合。

                    Polygons.Add(newPolygon);
                }
                else
                {
                    // 追加しなかった Polygon オブジェクトはリリース。
                    ReleasePolygon(newPolygon);
                }

                // 交差した辺を記憶。
                if (intersectPolygon.VertexCount == 2)
                {
                    Vector3 v0;
                    Vector3 v1;
                    intersectPolygon.GetVertex(0, out v0);
                    intersectPolygon.GetVertex(1, out v1);

                    var edge = new Edge(v0, v1);

                    intersectEdges.Add(ref edge);
                }

                // 交差する辺についての Polygon オブジェクトをリリース。
                ReleasePolygon(intersectPolygon);
            }

            // 新たな多角形の構築には、少なくとも 3 つの辺が必要。
            if (3 <= intersectEdges.Count)
            {
                Edge lastEdge;
                intersectEdges.GetLastItem(out lastEdge);
                intersectEdges.RemoveLast();

                Vector3 first = lastEdge.Point0;
                Vector3 second = lastEdge.Point1;

                Vector3 next;

                if (FindPointAndRemoveEdge(ref second, intersectEdges, out next))
                {
                    var closingPolygon = CreatePolygon();
                    Polygons.Add(closingPolygon);

                    // 交差する二つの辺から多角形の法線を算出。
                    Vector3 edge0;
                    Vector3 edge1;
                    Vector3.Subtract(ref first, ref second, out edge0);
                    Vector3.Subtract(ref next, ref second, out edge1);
                    Vector3 polygonNormal;
                    Vector3.Cross(ref edge0, ref edge1, out polygonNormal);

                    bool frontside;
                    DirectionEquals(ref plane.Normal, ref polygonNormal, out frontside);

                    Vector3 firstVertex;
                    Vector3 currentVertex;

                    if (frontside)
                    {
                        // 
                        closingPolygon.AddVertex(ref next);
                        closingPolygon.AddVertex(ref second);
                        closingPolygon.AddVertex(ref first);
                        firstVertex = next;
                        currentVertex = first;
                    }
                    else
                    {
                        closingPolygon.AddVertex(ref first);
                        closingPolygon.AddVertex(ref second);
                        closingPolygon.AddVertex(ref next);
                        firstVertex = first;
                        currentVertex = next;
                    }

                    while (0 < intersectEdges.Count)
                    {
                        if (FindPointAndRemoveEdge(ref currentVertex, intersectEdges, out next))
                        {
                            if (intersectEdges.Count != 0)
                            {
                                currentVertex = next;
                                closingPolygon.AddVertex(ref next);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            workingPolygons.Clear();
        }

        void DirectionEquals(ref Vector3 v0, ref Vector3 v1, out bool result)
        {
            float dot;
            Vector3.Dot(ref v0, ref v1, out dot);

            float angle = (float) Math.Acos(dot);

            result = (angle <= DirectionEqualsTolerance);
        }

        bool FindPointAndRemoveEdge(ref Vector3 point, StructList<Edge> edges, out Vector3 another)
        {
            another = default(Vector3);
            int index = -1;

            for (int i = 0; i < edges.Count; i++)
            {
                Edge edge;
                edges.GetItem(i, out edge);

                if (EqualsPoints(ref edge.Point0, ref point))
                {
                    another = edge.Point1;
                    index = i;
                    break;
                }
                else if (EqualsPoints(ref edge.Point1, ref point))
                {
                    another = edge.Point0;
                    index = i;
                    break;
                }
            }

            // リスト内部における部分的な配列複製を回避するため、
            // 対象となった要素を末尾と入れ替えた後、末尾を対象に削除。
            if (0 <= index)
            {
                edges.SwapWithLast(index);
                edges.RemoveLast();
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool EqualsPoints(ref Vector3 left, ref Vector3 right)
        {
            return ((float) Math.Abs(right.X - left.X) < EqualsPointTolerance &&
                (float) Math.Abs(right.Y - left.Y) < EqualsPointTolerance &&
                (float) Math.Abs(right.Z - left.Z) < EqualsPointTolerance);
        }

        void Clip(ref Plane plane, Polygon originalPolygon, Polygon newPolygon, Polygon intersectPolygon)
        {
            // 各頂点が面 plane の裏側にあるか否か。
            outsides.Clear();
            for (int iv = 0; iv < originalPolygon.VertexCount; iv++)
            {
                Vector3 v;
                originalPolygon.GetVertex(iv, out v);

                // 面 plane から頂点 v の距離。
                float distance;
                plane.DotCoordinate(ref v, out distance);

                // 頂点 v が面 plane の外側 (表側) にあるならば true、
                // さもなくば false。
                outsides.Add(0.0f < distance);
            }

            for (int iv0 = 0; iv0 < originalPolygon.VertexCount; iv0++)
            {
                // 二つの頂点は多角形の辺を表す。

                // 次の頂点のインデックス (末尾の次は先頭)。
                int iv1 = (iv0 + 1) % originalPolygon.VertexCount;

                if (outsides[iv0] && outsides[iv1])
                {
                    // 辺が面 plane の外側にあるならばスキップ。
                    continue;
                }

                if (outsides[iv0])
                {
                    // 面 plane の外側から内側へ向かう辺の場合。

                    Vector3 v0;
                    Vector3 v1;
                    originalPolygon.GetVertex(iv0, out v0);
                    originalPolygon.GetVertex(iv1, out v1);

                    Vector3? intersect;
                    IntersectEdgeAndPlane(ref v0, ref v1, ref plane, out intersect);

                    if (intersect != null)
                    {
                        Vector3 intersectV = intersect.Value;
                        newPolygon.AddVertex(ref intersectV);
                        intersectPolygon.AddVertex(ref intersectV);
                    }

                    newPolygon.AddVertex(ref v1);
                }
                else if (outsides[iv1])
                {
                    // 面 plane の内側から外側へ向かう辺の場合。

                    Vector3 v0;
                    Vector3 v1;
                    originalPolygon.GetVertex(iv0, out v0);
                    originalPolygon.GetVertex(iv1, out v1);

                    Vector3? intersect;
                    IntersectEdgeAndPlane(ref v0, ref v1, ref plane, out intersect);

                    if (intersect != null)
                    {
                        Vector3 intersectV = intersect.Value;
                        newPolygon.AddVertex(ref intersectV);
                        intersectPolygon.AddVertex(ref intersectV);
                    }
                }
                else
                {
                    // 辺が面の内側にある場合。

                    Vector3 v1;
                    originalPolygon.GetVertex(iv1, out v1);

                    newPolygon.AddVertex(ref v1);
                }
            }
        }

        /// <summary>
        /// 辺と平面の交差を判定します。
        /// </summary>
        /// <param name="point0">point1 と対をなす辺の点。</param>
        /// <param name="point1">point0 と対をなす辺の点。</param>
        /// <param name="plane">平面。</param>
        /// <param name="result">
        /// 交点 (辺と平面が交差する場合)、null (それ以外の場合)。
        /// </param>
        void IntersectEdgeAndPlane(ref Vector3 point0, ref Vector3 point1, ref Plane plane, out Vector3? result)
        {
            // 辺の方向。
            var direction = point0 - point1;
            direction.Normalize();

            // 辺と面 p との交差を判定。
            var ray = new Ray(point1, direction);

            float? intersect;
            ray.Intersects(ref plane, out intersect);

            if (intersect != null)
            {
                // 交点。
                result = RayHelper.GetPoint(ref ray, intersect.Value);
            }
            else
            {
                result = null;
            }
        }
    }
}
