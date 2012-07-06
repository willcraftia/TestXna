#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace TerrainDemo.LOD
{
    public struct ClipPlane
    {
        public Vector3 Point1 { get; private set; }
        public Vector3 Point2 { get; private set; }
        public Vector3 Point3 { get; private set; }
        public Vector3 Point4 { get; private set; }

        ClipPlane(Vector3 point1, Vector3 point2, Vector3 point3, Vector3 point4)
            : this()
        {
            Point1 = point1;
            Point2 = point2;
            Point3 = point3;
            Point4 = point4;
        }

        public static ClipPlane FromPoints(Vector3 point1, Vector3 point2, Vector3 point3, Vector3 point4)
        {
            return new ClipPlane(point1, point2, point3, point4);
        }

        public PlaneIntersectType PlaneInsersection(float targetY)
        {
            if (Point1.Y > targetY && Point2.Y > targetY && Point3.Y > targetY && Point4.Y > targetY)
                return PlaneIntersectType.Above;

            if (Point1.Y < targetY && Point2.Y < targetY && Point3.Y < targetY && Point4.Y < targetY)
                return PlaneIntersectType.Below;

            return PlaneIntersectType.Intersects;
        }

    }
}
