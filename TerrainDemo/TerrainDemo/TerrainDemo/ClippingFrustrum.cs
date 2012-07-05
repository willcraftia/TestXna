#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace TerrainDemo
{
    public struct ClippingFrustrum
    {
        public ClipPlane FarPlane { get; private set; }
        public ClipPlane NearPlane { get; private set; }
        public Vector3 CameraPosition { get; private set; }

        private ClippingFrustrum(Vector3[] frustrumCorners, Vector3 cameraPosition)
            : this()
        {
            FarPlane = ClipPlane.FromPoints(frustrumCorners[4], frustrumCorners[5], frustrumCorners[6], frustrumCorners[7]);
            NearPlane = ClipPlane.FromPoints(frustrumCorners[0], frustrumCorners[1], frustrumCorners[2], frustrumCorners[3]);

            CameraPosition = cameraPosition;
        }

        public static ClippingFrustrum FromFrustrumCorners(Vector3[] corners, Vector3 cameraPosition)
        {
            if (corners.Length != 8)
                throw new ArgumentException("Corners must be of length 8");

            return new ClippingFrustrum(corners, cameraPosition);
        }

        /// <summary>
        /// Project the clipping frustrum onto the target Y position
        /// </summary>
        /// <param name="targetY">Y position of the target plane</param>
        /// <returns>Four Sided Polygon with corners representing the X and Z positions</returns>
        public ViewClipShape ProjectToTargetY(float targetY)
        {
            var nearIntersectType = NearPlane.PlaneInsersection(targetY);
            var farIntersectType = FarPlane.PlaneInsersection(targetY);

            ViewClipShape shape;

            //Target plane not in view frustrum.  Setting a single point at the near plane center.
            if (nearIntersectType == farIntersectType
                && nearIntersectType != PlaneIntersectType.Intersects
                && farIntersectType != PlaneIntersectType.Intersects)
            {
                var singlePoint = new Vector2(CameraPosition.X, CameraPosition.Z);
                shape = ViewClipShape.FromPoints(singlePoint, singlePoint, singlePoint);
                shape.ViewPoint = singlePoint;
                return shape;
            }

            //Target plane slices between clipping planes with no intersection.
            if (nearIntersectType != farIntersectType
                && nearIntersectType != PlaneIntersectType.Intersects
                && farIntersectType != PlaneIntersectType.Intersects)
            {
                shape = FromNoIntersection(NearPlane, FarPlane, targetY);
            }
            else if (farIntersectType == PlaneIntersectType.Intersects)
            {
                //Far clipping plane intersects target plane
                shape = FromKnownIntersection(FarPlane, NearPlane, targetY);
            }
            else
            {
                //Near clipping plane intersects target plane
                shape = FromKnownIntersection(NearPlane, FarPlane, targetY);
            }

            //Now we need to determine whether we need to insert the camera position
            var cam = new Vector2(CameraPosition.X, CameraPosition.Z);
            if (!shape.ContainsPoint(cam))
            {
                AddCameraPosition(ref shape);
            }

            shape.ViewPoint = cam;

            return shape;

        }

        private void AddCameraPosition(ref ViewClipShape inputShape)
        {
            var cam = new Vector2(CameraPosition.X, CameraPosition.Z);
            var closest = 0;
            var closeDist = Vector2.Distance(cam, inputShape.Point1);

            //find closest point
            for (var i = 1; i < inputShape.Count; i++)
            {
                var dist = Vector2.Distance(cam, inputShape[i]);

                if (dist <= closeDist)
                    closest = i;
            }

            //Find the adjacent points
            int adj1, adj2;
            if (closest == 0)
            {
                adj1 = inputShape.Count - 1;
                adj2 = closest + 1;
            }
            else if (closest == inputShape.Count - 1)
            {
                adj1 = closest - 1;
                adj2 = 0;
            }
            else
            {
                adj1 = closest - 1;
                adj2 = closest + 1;
            }

            var angle1 = CullingUtils.LineAngle(inputShape[closest] - cam, inputShape[adj1] - cam);
            var angle2 = CullingUtils.LineAngle(inputShape[closest] - cam, inputShape[adj2] - cam);

            if (angle1 < 90 && angle2 < 90)
            {
                //point will be inside the shape, replace it with camera position
                inputShape.ReplacePoint(closest, cam);
            }
            else
            {
                //use the angles to determine what points it is between
                if (angle1 >= 90)
                {
                    inputShape.InsertPointAt(closest, cam);
                }
                else
                {
                    inputShape.InsertPointAt(adj2, cam);
                }
            }
        }

        /// <summary>
        /// Ground does not intersect either plane, but lies between them.
        /// </summary>
        /// <param name="plane1"></param>
        /// <param name="plane2"></param>
        /// <param name="targetY"></param>
        /// <returns></returns>
        private static ViewClipShape FromNoIntersection(ClipPlane plane1, ClipPlane plane2, float targetY)
        {
            var intersect1 = CullingUtils.LinePointFromKnownY(plane1.Point1, plane2.Point1, targetY);
            var intersect2 = CullingUtils.LinePointFromKnownY(plane1.Point2, plane2.Point2, targetY);
            var intersect3 = CullingUtils.LinePointFromKnownY(plane1.Point3, plane2.Point3, targetY);
            var intersect4 = CullingUtils.LinePointFromKnownY(plane1.Point4, plane2.Point4, targetY);

            var point1 = new Vector2(intersect1.X, intersect1.Z);
            var point2 = new Vector2(intersect2.X, intersect2.Z);
            var point3 = new Vector2(intersect3.X, intersect3.Z);
            var point4 = new Vector2(intersect4.X, intersect4.Z);

            return ViewClipShape.FromPoints(point1, point2, point3, point4);
        }

        /// <summary>
        /// Use when one plane is known to be intersecting
        /// </summary>
        /// <param name="plane1">The plane known to be intersecting</param>
        /// <param name="plane2">The second plane</param>
        /// <param name="targetY">The target Y position of the ground plane</param>
        /// <returns></returns>
        private static ViewClipShape FromKnownIntersection(ClipPlane plane1, ClipPlane plane2, float targetY)
        {
            Vector3 intersect1, intersect2, intersect3, intersect4 = Vector3.Zero;
            Vector3 intersect5 = Vector3.Zero, intersect6 = Vector3.Zero;
            int pointCount;

            if (plane1.Point1.Y > targetY) //point 1 is above the plane
            {
                if (plane1.Point2.Y > targetY) //point 2 is above the plane
                {
                    if (plane1.Point3.Y > targetY) //point 3 is above the plane, point 4 has to be below - points 1, 2 & 3 above
                    {
                        intersect1 = CullingUtils.LinePointFromKnownY(plane1.Point1, plane1.Point4, targetY);
                        intersect2 = CullingUtils.LinePointFromKnownY(plane1.Point3, plane1.Point4, targetY);
                        pointCount = 3;

                        #region PLANE 2 PROJECTION
                        /* PLANE 2 PROJECTION */
                        if (plane2.Point4.Y > targetY) //plane 2 is completely above the line
                        {
                            intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point4, plane1.Point4, targetY);
                        }
                        else //point 4 of plane 2 is below the line
                        {
                            if (plane2.Point2.Y < targetY) //point 2 of plane 2 is below the line - Down 1, 2, 3 & 4
                            {
                                intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point3, plane1.Point3, targetY);
                                intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point2, plane1.Point1, targetY);
                                intersect5 = CullingUtils.LinePointFromKnownY(plane2.Point1, plane1.Point1, targetY);
                                pointCount = 5;

                            }
                            else //point 2 of plane 2 is above the line
                            {
                                if (plane2.Point3.Y > targetY) //plane 2 point 3 is above the line
                                {
                                    intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point3, plane2.Point4, targetY);
                                }
                                else //plane 2 point 3 is below the line
                                {
                                    intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point3, plane1.Point3, targetY);
                                    intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point2, plane2.Point3, targetY);
                                    pointCount = 4;
                                }

                                if (plane2.Point1.Y > targetY) //plane 2 point 1 is above the line
                                {
                                    if (pointCount == 4)
                                        intersect5 = CullingUtils.LinePointFromKnownY(plane2.Point1, plane2.Point4, targetY);
                                    else
                                        intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point1, plane2.Point4, targetY);

                                    pointCount++;
                                }
                                else //plane 2 point 1 is below the line
                                {
                                    if (pointCount == 4)
                                    {
                                        intersect5 = CullingUtils.LinePointFromKnownY(plane2.Point2, plane2.Point1, targetY);
                                        intersect6 = CullingUtils.LinePointFromKnownY(plane2.Point1, plane1.Point1, targetY);
                                    }
                                    else
                                    {
                                        intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point2, plane2.Point1, targetY);
                                        intersect5 = CullingUtils.LinePointFromKnownY(plane2.Point1, plane1.Point1, targetY);
                                    }
                                    pointCount += 2;
                                }
                            }
                        }
                        /* END PLANE 2 PROJECTION */
                        #endregion

                    }
                    else //point 3 is below the target plane
                    {
                        if (plane1.Point4.Y > targetY) //point 4 is above the target plane - points 1, 2 & 4 above
                        {
                            intersect1 = CullingUtils.LinePointFromKnownY(plane1.Point4, plane1.Point3, targetY);
                            intersect2 = CullingUtils.LinePointFromKnownY(plane1.Point2, plane1.Point3, targetY);
                            pointCount = 3;

                            #region PLANE 2 PROJECTION
                            /* PLANE 2 PROJECTION */
                            if (plane2.Point3.Y > targetY) //plane 2 is completely above the line
                            {
                                intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point3, plane1.Point3, targetY);
                            }
                            else //point 4 of plane 2 is below the line
                            {
                                if (plane2.Point1.Y < targetY) //point 2 of plane 2 is below the line - Down 1, 2, 3 & 4
                                {
                                    intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point2, plane1.Point2, targetY);
                                    intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point1, plane1.Point4, targetY);
                                    intersect5 = CullingUtils.LinePointFromKnownY(plane2.Point4, plane1.Point4, targetY);
                                    pointCount = 5;

                                }
                                else //point 2 of plane 2 is above the line
                                {
                                    if (plane2.Point2.Y > targetY) //plane 2 point 3 is above the line
                                    {
                                        intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point2, plane2.Point3, targetY);
                                    }
                                    else //plane 2 point 3 is below the line
                                    {
                                        intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point2, plane1.Point2, targetY);
                                        intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point1, plane2.Point2, targetY);
                                        pointCount = 4;
                                    }

                                    if (plane2.Point4.Y > targetY) //plane 2 point 1 is above the line
                                    {
                                        if (pointCount == 4)
                                            intersect5 = CullingUtils.LinePointFromKnownY(plane2.Point4, plane2.Point3, targetY);
                                        else
                                            intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point4, plane2.Point3, targetY);

                                        pointCount++;
                                    }
                                    else //plane 2 point 1 is below the line
                                    {
                                        if (pointCount == 4)
                                        {
                                            intersect5 = CullingUtils.LinePointFromKnownY(plane2.Point1, plane2.Point4, targetY);
                                            intersect6 = CullingUtils.LinePointFromKnownY(plane2.Point4, plane1.Point4, targetY);
                                        }
                                        else
                                        {
                                            intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point1, plane2.Point4, targetY);
                                            intersect5 = CullingUtils.LinePointFromKnownY(plane2.Point4, plane1.Point4, targetY);
                                        }
                                        pointCount += 2;
                                    }
                                }
                            }
                            /* END PLANE 2 PROJECTION */
                            #endregion

                        }
                        else //points 3 and 4 are both below the target plane - points 1 & 2 are above
                        {
                            intersect1 = CullingUtils.LinePointFromKnownY(plane1.Point1, plane1.Point4, targetY);
                            intersect2 = CullingUtils.LinePointFromKnownY(plane1.Point2, plane1.Point3, targetY);

                            #region PLANE 2 PROJECTION
                            if (plane2.Point2.Y > targetY) //point2 is above
                            {
                                if (plane2.Point3.Y > targetY) //point3 is above
                                {
                                    if (plane2.Point4.Y > targetY) //All four points are above
                                    {
                                        intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point3, plane1.Point3, targetY);
                                        intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point4, plane1.Point4, targetY);
                                        pointCount = 4;
                                    }
                                    else //point4 is below - point1, point2 and point3 are above
                                    {
                                        intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point3, plane1.Point3, targetY);
                                        intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point3, plane2.Point4, targetY);
                                        intersect5 = CullingUtils.LinePointFromKnownY(plane2.Point1, plane2.Point4, targetY);
                                        pointCount = 5;
                                    }
                                }
                                else //point3 is below
                                {
                                    if (plane2.Point4.Y > targetY) //point1, point2, and point4 are above
                                    {
                                        intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point2, plane2.Point3, targetY);
                                        intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point4, plane2.Point3, targetY);
                                        intersect5 = CullingUtils.LinePointFromKnownY(plane2.Point4, plane1.Point4, targetY);
                                        pointCount = 5;
                                    }
                                    else //point4 is below
                                    {
                                        if (plane2.Point1.Y > targetY) //point1 and point2 are above
                                        {
                                            intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point2, plane2.Point3, targetY);
                                            intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point1, plane2.Point4, targetY);
                                            pointCount = 4;
                                        }
                                        else //only point2 is above
                                        {
                                            intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point2, plane2.Point3, targetY);
                                            intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point2, plane2.Point1, targetY);
                                            intersect5 = CullingUtils.LinePointFromKnownY(plane2.Point1, plane1.Point1, targetY);
                                            pointCount = 5;
                                        }
                                    }
                                }
                            }
                            else //Point2 is below 
                            {
                                if (plane2.Point1.Y > targetY) //only point1 is above
                                {
                                    intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point2, plane1.Point2, targetY);
                                    intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point1, plane2.Point2, targetY);
                                    intersect5 = CullingUtils.LinePointFromKnownY(plane2.Point1, plane2.Point4, targetY);
                                    pointCount = 5;
                                }
                                else //all points are below
                                {
                                    intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point2, plane1.Point2, targetY);
                                    intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point1, plane1.Point1, targetY);
                                    pointCount = 4;
                                }
                            }

                            #endregion
                        }
                    }
                }
                else //point 2 is below the target plane
                {
                    if (plane1.Point4.Y < targetY) //point 4 is also below the target plane, point 3 must be also - point 1 is above
                    {
                        intersect1 = CullingUtils.LinePointFromKnownY(plane1.Point1, plane1.Point4, targetY);
                        intersect2 = CullingUtils.LinePointFromKnownY(plane1.Point1, plane1.Point2, targetY);
                        pointCount = 3;

                        #region PLANE 2 PROJECTION

                        if (plane2.Point1.Y < targetY) //All of plane 2 is below
                        {
                            intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point1, plane1.Point1, targetY);
                        }
                        else //point1 is above
                        {
                            if (plane2.Point3.Y > targetY) //All of plane 2 is above
                            {
                                intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point2, plane1.Point2, targetY);
                                intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point3, plane1.Point3, targetY);
                                intersect5 = CullingUtils.LinePointFromKnownY(plane2.Point4, plane1.Point4, targetY);
                                pointCount = 5;
                            }
                            else //point3 is below
                            {
                                if (plane2.Point2.Y > targetY) //point2 is above
                                {
                                    intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point2, plane1.Point2, targetY);
                                    intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point2, plane2.Point3, targetY);
                                    pointCount = 4;
                                }
                                else //point2 is below
                                {
                                    intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point1, plane2.Point2, targetY);
                                    pointCount = 3;
                                }

                                if (plane2.Point4.Y > targetY) //point4 is above
                                {
                                    if (pointCount == 4)
                                    {
                                        intersect5 = CullingUtils.LinePointFromKnownY(plane2.Point4, plane2.Point3, targetY);
                                        intersect6 = CullingUtils.LinePointFromKnownY(plane2.Point4, plane1.Point4, targetY);
                                        pointCount = 6;
                                    }
                                    else //point2 was below
                                    {
                                        intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point4, plane2.Point3, targetY);
                                        intersect5 = CullingUtils.LinePointFromKnownY(plane2.Point4, plane1.Point4, targetY);
                                        pointCount = 5;
                                    }
                                }
                                else //point4 is below
                                {
                                    if (pointCount == 4)
                                    {
                                        intersect5 = CullingUtils.LinePointFromKnownY(plane2.Point1, plane2.Point4, targetY);
                                        pointCount = 5;
                                    }
                                    else //point2 was below, only point1 is above
                                    {
                                        intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point1, plane2.Point4, targetY);
                                        pointCount = 4;
                                    }
                                }
                            }
                        }

                        #endregion

                    }
                    else //point 4 is above the target plane
                    {
                        if (plane1.Point3.Y < targetY) //point 3 is below the target plane - 1 & 4 are above
                        {
                            intersect1 = CullingUtils.LinePointFromKnownY(plane1.Point4, plane1.Point3, targetY);
                            intersect2 = CullingUtils.LinePointFromKnownY(plane1.Point1, plane1.Point2, targetY);

                            #region PLANE 2 PROJECTION
                            if (plane2.Point1.Y > targetY) //Point1 is above
                            {
                                if (plane2.Point2.Y > targetY) //Point2 is above
                                {
                                    if (plane2.Point3.Y > targetY) //All four points are above
                                    {
                                        intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point2, plane1.Point2, targetY);
                                        intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point3, plane1.Point3, targetY);
                                        pointCount = 4;
                                    }
                                    else //Point3 is below - Point4, Point1 and Point2 are above
                                    {
                                        intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point2, plane1.Point2, targetY);
                                        intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point2, plane2.Point3, targetY);
                                        intersect5 = CullingUtils.LinePointFromKnownY(plane2.Point4, plane2.Point3, targetY);
                                        pointCount = 5;
                                    }
                                }
                                else //Point2 is below
                                {
                                    if (plane2.Point3.Y > targetY) //Point4, Point1, and Point3 are above
                                    {
                                        intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point1, plane2.Point2, targetY);
                                        intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point3, plane2.Point2, targetY);
                                        intersect5 = CullingUtils.LinePointFromKnownY(plane2.Point3, plane1.Point3, targetY);
                                        pointCount = 5;
                                    }
                                    else //Point3 is below
                                    {
                                        if (plane2.Point4.Y > targetY) //Point4 and Point1 are above
                                        {
                                            intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point1, plane2.Point2, targetY);
                                            intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point4, plane2.Point3, targetY);
                                            pointCount = 4;
                                        }
                                        else //only Point1 is above
                                        {
                                            intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point1, plane2.Point2, targetY);
                                            intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point1, plane2.Point4, targetY);
                                            intersect5 = CullingUtils.LinePointFromKnownY(plane2.Point4, plane1.Point4, targetY);
                                            pointCount = 5;
                                        }
                                    }
                                }
                            }
                            else //Point1 is below 
                            {
                                if (plane2.Point4.Y > targetY) //only Point4 is above
                                {
                                    intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point1, plane1.Point1, targetY);
                                    intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point4, plane2.Point1, targetY);
                                    intersect5 = CullingUtils.LinePointFromKnownY(plane2.Point4, plane2.Point3, targetY);
                                    pointCount = 5;
                                }
                                else //all points are below
                                {
                                    intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point1, plane1.Point1, targetY);
                                    intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point4, plane1.Point4, targetY);
                                    pointCount = 4;
                                }
                            }

                            #endregion

                        }
                        else //points 1, 4 & 3 are above
                        {
                            intersect1 = CullingUtils.LinePointFromKnownY(plane1.Point3, plane1.Point2, targetY);
                            intersect2 = CullingUtils.LinePointFromKnownY(plane1.Point1, plane1.Point2, targetY);
                            pointCount = 3;

                            #region PLANE 2 PROJECTION
                            /* PLANE 2 PROJECTION */
                            if (plane2.Point2.Y > targetY) //plane 2 is completely above the line
                            {
                                intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point2, plane1.Point2, targetY);
                            }
                            else //point 4 of plane 2 is below the line
                            {
                                if (plane2.Point4.Y < targetY) //point 2 of plane 2 is below the line - Down 1, 2, 3 & 4
                                {
                                    intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point1, plane1.Point1, targetY);
                                    intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point4, plane1.Point3, targetY);
                                    intersect5 = CullingUtils.LinePointFromKnownY(plane2.Point3, plane1.Point3, targetY);
                                    pointCount = 5;

                                }
                                else //point 2 of plane 2 is above the line
                                {
                                    if (plane2.Point1.Y > targetY) //plane 2 point 3 is above the line
                                    {
                                        intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point1, plane2.Point2, targetY);
                                    }
                                    else //plane 2 point 3 is below the line
                                    {
                                        intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point1, plane1.Point1, targetY);
                                        intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point4, plane2.Point1, targetY);
                                        pointCount = 4;
                                    }

                                    if (plane2.Point3.Y > targetY) //plane 2 point 1 is above the line
                                    {
                                        if (pointCount == 4)
                                            intersect5 = CullingUtils.LinePointFromKnownY(plane2.Point3, plane2.Point2, targetY);
                                        else
                                            intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point3, plane2.Point2, targetY);

                                        pointCount++;
                                    }
                                    else //plane 2 point 1 is below the line
                                    {
                                        if (pointCount == 4)
                                        {
                                            intersect5 = CullingUtils.LinePointFromKnownY(plane2.Point4, plane2.Point3, targetY);
                                            intersect6 = CullingUtils.LinePointFromKnownY(plane2.Point3, plane1.Point3, targetY);
                                        }
                                        else
                                        {
                                            intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point4, plane2.Point3, targetY);
                                            intersect5 = CullingUtils.LinePointFromKnownY(plane2.Point3, plane1.Point3, targetY);
                                        }
                                        pointCount += 2;
                                    }
                                }
                            }
                            /* END PLANE 2 PROJECTION */
                            #endregion
                        }
                    }
                }
            }
            else //point 1 is below the target plane
            {
                if (plane1.Point2.Y > targetY) //point 2 is above the plane
                {
                    if (plane1.Point4.Y > targetY) //points 2 and 4 are above the plane so point 3 must be as well Up 2, 3 & 4
                    {
                        intersect1 = CullingUtils.LinePointFromKnownY(plane1.Point2, plane1.Point1, targetY);
                        intersect2 = CullingUtils.LinePointFromKnownY(plane1.Point4, plane1.Point1, targetY);
                        pointCount = 3;

                        #region PLANE 2 PROJECTION
                        /* PLANE 2 PROJECTION */
                        if (plane2.Point1.Y > targetY) //plane 2 is completely above the line
                        {
                            intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point1, plane1.Point1, targetY);
                        }
                        else //point 4 of plane 2 is below the line
                        {
                            if (plane2.Point3.Y < targetY) //point 2 of plane 2 is below the line - Down 1, 2, 3 & 4
                            {
                                intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point4, plane1.Point4, targetY);
                                intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point3, plane1.Point2, targetY);
                                intersect5 = CullingUtils.LinePointFromKnownY(plane2.Point2, plane1.Point2, targetY);
                                pointCount = 5;

                            }
                            else //point 2 of plane 2 is above the line
                            {
                                if (plane2.Point4.Y > targetY) //plane 2 point 3 is above the line
                                {
                                    intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point4, plane2.Point1, targetY);
                                }
                                else //plane 2 point 3 is below the line
                                {
                                    intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point4, plane1.Point4, targetY);
                                    intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point3, plane2.Point4, targetY);
                                    pointCount = 4;
                                }

                                if (plane2.Point2.Y > targetY) //plane 2 point 1 is above the line
                                {
                                    if (pointCount == 4)
                                        intersect5 = CullingUtils.LinePointFromKnownY(plane2.Point2, plane2.Point1, targetY);
                                    else
                                        intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point2, plane2.Point1, targetY);

                                    pointCount++;
                                }
                                else //plane 2 point 1 is below the line
                                {
                                    if (pointCount == 4)
                                    {
                                        intersect5 = CullingUtils.LinePointFromKnownY(plane2.Point3, plane2.Point2, targetY);
                                        intersect6 = CullingUtils.LinePointFromKnownY(plane2.Point2, plane1.Point2, targetY);
                                    }
                                    else
                                    {
                                        intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point3, plane2.Point2, targetY);
                                        intersect5 = CullingUtils.LinePointFromKnownY(plane2.Point2, plane1.Point2, targetY);
                                    }
                                    pointCount += 2;
                                }
                            }
                        }
                        /* END PLANE 2 PROJECTION */
                        #endregion
                    }
                    else //point 4 is below the plane
                    {
                        if (plane1.Point3.Y > targetY) //point 3 is above the plane - Up 2 & 3
                        {
                            intersect1 = CullingUtils.LinePointFromKnownY(plane1.Point2, plane1.Point1, targetY);
                            intersect2 = CullingUtils.LinePointFromKnownY(plane1.Point3, plane1.Point4, targetY);


                            #region PLANE 2 PROJECTION
                            if (plane2.Point3.Y > targetY) //Point3 is above
                            {
                                if (plane2.Point4.Y > targetY) //Point4 is above
                                {
                                    if (plane2.Point1.Y > targetY) //All four points are above
                                    {
                                        intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point4, plane1.Point4, targetY);
                                        intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point1, plane1.Point1, targetY);
                                        pointCount = 4;
                                    }
                                    else //Point1 is below - Point2, Point3 and Point4 are above
                                    {
                                        intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point4, plane1.Point4, targetY);
                                        intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point4, plane2.Point1, targetY);
                                        intersect5 = CullingUtils.LinePointFromKnownY(plane2.Point2, plane2.Point1, targetY);
                                        pointCount = 5;
                                    }
                                }
                                else //Point4 is below
                                {
                                    if (plane2.Point1.Y > targetY) //Point2, Point3, and Point1 are above
                                    {
                                        intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point3, plane2.Point4, targetY);
                                        intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point1, plane2.Point4, targetY);
                                        intersect5 = CullingUtils.LinePointFromKnownY(plane2.Point1, plane1.Point1, targetY);
                                        pointCount = 5;
                                    }
                                    else //Point1 is below
                                    {
                                        if (plane2.Point2.Y > targetY) //Point2 and Point3 are above
                                        {
                                            intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point3, plane2.Point4, targetY);
                                            intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point2, plane2.Point1, targetY);
                                            pointCount = 4;
                                        }
                                        else //only Point3 is above
                                        {
                                            intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point3, plane2.Point4, targetY);
                                            intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point3, plane2.Point2, targetY);
                                            intersect5 = CullingUtils.LinePointFromKnownY(plane2.Point2, plane1.Point2, targetY);
                                            pointCount = 5;
                                        }
                                    }
                                }
                            }
                            else //Point3 is below 
                            {
                                if (plane2.Point2.Y > targetY) //only Point2 is above
                                {
                                    intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point3, plane1.Point3, targetY);
                                    intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point2, plane2.Point3, targetY);
                                    intersect5 = CullingUtils.LinePointFromKnownY(plane2.Point2, plane2.Point1, targetY);
                                    pointCount = 5;
                                }
                                else //all points are below
                                {
                                    intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point3, plane1.Point3, targetY);
                                    intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point2, plane1.Point2, targetY);
                                    pointCount = 4;
                                }
                            }

                            #endregion

                        }
                        else //only point 2 is above the plane - Up 2
                        {
                            intersect1 = CullingUtils.LinePointFromKnownY(plane1.Point2, plane1.Point3, targetY);
                            intersect2 = CullingUtils.LinePointFromKnownY(plane1.Point2, plane1.Point3, targetY);
                            pointCount = 3;

                            #region PLANE 2 PROJECTION

                            if (plane2.Point2.Y < targetY) //All of plane 2 is below
                            {
                                intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point2, plane1.Point2, targetY);
                            }
                            else //Point2 is above
                            {
                                if (plane2.Point4.Y > targetY) //All of plane 2 is above
                                {
                                    intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point3, plane1.Point3, targetY);
                                    intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point4, plane1.Point4, targetY);
                                    intersect5 = CullingUtils.LinePointFromKnownY(plane2.Point1, plane1.Point1, targetY);
                                    pointCount = 5;
                                }
                                else //Point4 is below
                                {
                                    if (plane2.Point3.Y > targetY) //Point3 is above
                                    {
                                        intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point3, plane1.Point3, targetY);
                                        intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point3, plane2.Point4, targetY);
                                        pointCount = 4;
                                    }
                                    else //Point3 is below
                                    {
                                        intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point2, plane2.Point3, targetY);
                                        pointCount = 3;
                                    }

                                    if (plane2.Point1.Y > targetY) //Point1 is above
                                    {
                                        if (pointCount == 4)
                                        {
                                            intersect5 = CullingUtils.LinePointFromKnownY(plane2.Point1, plane2.Point4, targetY);
                                            intersect6 = CullingUtils.LinePointFromKnownY(plane2.Point1, plane1.Point1, targetY);
                                            pointCount = 6;
                                        }
                                        else //Point3 was below
                                        {
                                            intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point1, plane2.Point4, targetY);
                                            intersect5 = CullingUtils.LinePointFromKnownY(plane2.Point1, plane1.Point1, targetY);
                                            pointCount = 5;
                                        }
                                    }
                                    else //Point1 is below
                                    {
                                        if (pointCount == 4)
                                        {
                                            intersect5 = CullingUtils.LinePointFromKnownY(plane2.Point2, plane2.Point1, targetY);
                                            pointCount = 5;
                                        }
                                        else //Point3 was below, only Point2 is above
                                        {
                                            intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point2, plane2.Point1, targetY);
                                            pointCount = 4;
                                        }
                                    }
                                }
                            }

                            #endregion
                        }
                    }
                }
                else //point 2 is below the line
                {
                    if (plane1.Point3.Y > targetY) //point 3 is above the line
                    {
                        if (plane1.Point4.Y > targetY) //points 3 and 4 are above the line - Up 3 & 4
                        {
                            intersect1 = CullingUtils.LinePointFromKnownY(plane1.Point3, plane1.Point2, targetY);
                            intersect2 = CullingUtils.LinePointFromKnownY(plane1.Point4, plane1.Point1, targetY);

                            #region PLANE 2 PROJECTION
                            if (plane2.Point4.Y > targetY) //Point4 is above
                            {
                                if (plane2.Point1.Y > targetY) //Point1 is above
                                {
                                    if (plane2.Point2.Y > targetY) //All four points are above
                                    {
                                        intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point1, plane1.Point1, targetY);
                                        intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point2, plane1.Point2, targetY);
                                        pointCount = 4;
                                    }
                                    else //Point2 is below - Point3, Point4 and Point1 are above
                                    {
                                        intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point1, plane1.Point1, targetY);
                                        intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point1, plane2.Point2, targetY);
                                        intersect5 = CullingUtils.LinePointFromKnownY(plane2.Point3, plane2.Point2, targetY);
                                        pointCount = 5;
                                    }
                                }
                                else //Point1 is below
                                {
                                    if (plane2.Point2.Y > targetY) //Point3, Point4, and Point2 are above
                                    {
                                        intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point4, plane2.Point1, targetY);
                                        intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point2, plane2.Point1, targetY);
                                        intersect5 = CullingUtils.LinePointFromKnownY(plane2.Point2, plane1.Point2, targetY);
                                        pointCount = 5;
                                    }
                                    else //Point2 is below
                                    {
                                        if (plane2.Point3.Y > targetY) //Point3 and Point4 are above
                                        {
                                            intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point4, plane2.Point1, targetY);
                                            intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point3, plane2.Point2, targetY);
                                            pointCount = 4;
                                        }
                                        else //only Point4 is above
                                        {
                                            intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point4, plane2.Point1, targetY);
                                            intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point4, plane2.Point3, targetY);
                                            intersect5 = CullingUtils.LinePointFromKnownY(plane2.Point3, plane1.Point3, targetY);
                                            pointCount = 5;
                                        }
                                    }
                                }
                            }
                            else //Point4 is below 
                            {
                                if (plane2.Point3.Y > targetY) //only Point3 is above
                                {
                                    intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point4, plane1.Point4, targetY);
                                    intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point3, plane2.Point4, targetY);
                                    intersect5 = CullingUtils.LinePointFromKnownY(plane2.Point3, plane2.Point2, targetY);
                                    pointCount = 5;
                                }
                                else //all points are below
                                {
                                    intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point4, plane1.Point4, targetY);
                                    intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point3, plane1.Point3, targetY);
                                    pointCount = 4;
                                }
                            }

                            #endregion
                        }
                        else //only point 3 is above the line - Up 3
                        {
                            intersect1 = CullingUtils.LinePointFromKnownY(plane1.Point3, plane1.Point2, targetY);
                            intersect2 = CullingUtils.LinePointFromKnownY(plane1.Point3, plane1.Point4, targetY);
                            pointCount = 3;

                            #region PLANE 2 PROJECTION

                            if (plane2.Point3.Y < targetY) //All of plane 2 is below
                            {
                                intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point3, plane1.Point3, targetY);
                            }
                            else //Point3 is above
                            {
                                if (plane2.Point1.Y > targetY) //All of plane 2 is above
                                {
                                    intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point4, plane1.Point4, targetY);
                                    intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point1, plane1.Point1, targetY);
                                    intersect5 = CullingUtils.LinePointFromKnownY(plane2.Point2, plane1.Point2, targetY);
                                    pointCount = 5;
                                }
                                else //Point1 is below
                                {
                                    if (plane2.Point4.Y > targetY) //Point4 is above
                                    {
                                        intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point4, plane1.Point4, targetY);
                                        intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point4, plane2.Point1, targetY);
                                        pointCount = 4;
                                    }
                                    else //Point4 is below
                                    {
                                        intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point3, plane2.Point4, targetY);
                                        pointCount = 3;
                                    }

                                    if (plane2.Point2.Y > targetY) //Point2 is above
                                    {
                                        if (pointCount == 4)
                                        {
                                            intersect5 = CullingUtils.LinePointFromKnownY(plane2.Point2, plane2.Point1, targetY);
                                            intersect6 = CullingUtils.LinePointFromKnownY(plane2.Point2, plane1.Point2, targetY);
                                            pointCount = 6;
                                        }
                                        else //Point4 was below
                                        {
                                            intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point2, plane2.Point1, targetY);
                                            intersect5 = CullingUtils.LinePointFromKnownY(plane2.Point2, plane1.Point2, targetY);
                                            pointCount = 5;
                                        }
                                    }
                                    else //Point2 is below
                                    {
                                        if (pointCount == 4)
                                        {
                                            intersect5 = CullingUtils.LinePointFromKnownY(plane2.Point3, plane2.Point2, targetY);
                                            pointCount = 5;
                                        }
                                        else //Point4 was below, only Point3 is above
                                        {
                                            intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point3, plane2.Point2, targetY);
                                            pointCount = 4;
                                        }
                                    }
                                }
                            }

                            #endregion

                        }
                    }
                    else //points 1, 2 & 3 are all below, Up 4
                    {
                        intersect1 = CullingUtils.LinePointFromKnownY(plane1.Point4, plane1.Point3, targetY);
                        intersect2 = CullingUtils.LinePointFromKnownY(plane1.Point4, plane1.Point1, targetY);
                        pointCount = 3;

                        #region PLANE 2 PROJECTION

                        if (plane2.Point4.Y < targetY) //All of plane 2 is below
                        {
                            intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point4, plane1.Point4, targetY);
                        }
                        else //Point4 is above
                        {
                            if (plane2.Point2.Y > targetY) //All of plane 2 is above
                            {
                                intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point1, plane1.Point1, targetY);
                                intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point2, plane1.Point2, targetY);
                                intersect5 = CullingUtils.LinePointFromKnownY(plane2.Point3, plane1.Point3, targetY);
                                pointCount = 5;
                            }
                            else //Point2 is below
                            {
                                if (plane2.Point1.Y > targetY) //Point1 is above
                                {
                                    intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point1, plane1.Point1, targetY);
                                    intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point1, plane2.Point2, targetY);
                                    pointCount = 4;
                                }
                                else //Point1 is below
                                {
                                    intersect3 = CullingUtils.LinePointFromKnownY(plane2.Point4, plane2.Point1, targetY);
                                    pointCount = 3;
                                }

                                if (plane2.Point3.Y > targetY) //Point3 is above
                                {
                                    if (pointCount == 4)
                                    {
                                        intersect5 = CullingUtils.LinePointFromKnownY(plane2.Point3, plane2.Point2, targetY);
                                        intersect6 = CullingUtils.LinePointFromKnownY(plane2.Point3, plane1.Point3, targetY);
                                        pointCount = 6;
                                    }
                                    else //Point1 was below
                                    {
                                        intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point3, plane2.Point2, targetY);
                                        intersect5 = CullingUtils.LinePointFromKnownY(plane2.Point3, plane1.Point3, targetY);
                                        pointCount = 5;
                                    }
                                }
                                else //Point3 is below
                                {
                                    if (pointCount == 4)
                                    {
                                        intersect5 = CullingUtils.LinePointFromKnownY(plane2.Point4, plane2.Point3, targetY);
                                        pointCount = 5;
                                    }
                                    else //Point1 was below, only Point4 is above
                                    {
                                        intersect4 = CullingUtils.LinePointFromKnownY(plane2.Point4, plane2.Point3, targetY);
                                        pointCount = 4;
                                    }
                                }
                            }
                        }

                        #endregion

                    }
                }
            }

            var point1 = new Vector2(intersect1.X, intersect1.Z);
            var point2 = new Vector2(intersect2.X, intersect2.Z);
            var point3 = new Vector2(intersect3.X, intersect3.Z);
            var point4 = new Vector2(intersect4.X, intersect4.Z);
            var point5 = new Vector2(intersect5.X, intersect5.Z);
            var point6 = new Vector2(intersect6.X, intersect6.Z);

            switch (pointCount)
            {
                case 3:
                    return ViewClipShape.FromPoints(point1, point2, point3);

                case 4:
                    return ViewClipShape.FromPoints(point1, point2, point3, point4);

                case 5:
                    return ViewClipShape.FromPoints(point1, point2, point3, point4, point5);

                default:
                    return ViewClipShape.FromPoints(point1, point2, point3, point4, point5, point6);
            }
        }
    }
}
