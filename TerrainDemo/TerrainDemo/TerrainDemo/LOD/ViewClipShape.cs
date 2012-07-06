#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace TerrainDemo.LOD
{
    public struct ViewClipShape
    {
        public Vector2 Point1 { get; private set; }
        public Vector2 Point2 { get; private set; }
        public Vector2 Point3 { get; private set; }
        public Vector2 Point4 { get; private set; }
        public Vector2 Point5 { get; private set; }
        public Vector2 Point6 { get; private set; }
        private Vector2 Axis1 { get; set; }
        private Vector2 Axis2 { get; set; }
        private Vector2 Axis3 { get; set; }
        private Vector2 Axis4 { get; set; }
        private Vector2 Axis5 { get; set; }
        private Vector2 Axis6 { get; set; }

        public Vector2 ViewPoint { get; internal set; }

        public int A1Min, A2Min, A3Min, A4Min, A5Min, A6Min;
        public int A1Max, A2Max, A3Max, A4Max, A5Max, A6Max;

        public int Count { get; private set; }

        public Vector2 this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return Point1;
                    case 1:
                        return Point2;
                    case 2:
                        return Point3;
                    case 3:
                        return Point4;
                    case 4:
                        return Point5;
                    case 5:
                        return Point6;
                    default:
                        throw new ArgumentException("Max number of points exceeded.");
                }
            }
        }

        ViewClipShape(Vector2 point1, Vector2 point2, Vector2 point3)
            : this()
        {
            Point1 = point1;
            Point2 = point2;
            Point3 = point3;

            Count = 3;
        }

        ViewClipShape(Vector2 point1, Vector2 point2, Vector2 point3, Vector2 point4)
            : this(point1, point2, point3)
        {
            Point4 = point4;
            Count = 4;
        }

        ViewClipShape(Vector2 point1, Vector2 point2, Vector2 point3, Vector2 point4, Vector2 point5)
            : this(point1, point2, point3, point4)
        {
            Point5 = point5;
            Count = 5;
        }

        ViewClipShape(Vector2 point1, Vector2 point2, Vector2 point3, Vector2 point4, Vector2 point5, Vector2 point6)
            : this(point1, point2, point3, point4, point5)
        {
            Point6 = point6;
            Count = 6;
        }

        public static ViewClipShape FromPoints(Vector2 point1, Vector2 point2, Vector2 point3)
        {
            var shape = new ViewClipShape(point1, point2, point3);
            shape.BuildAxis();
            return shape;
        }

        public static ViewClipShape FromPoints(Vector2 point1, Vector2 point2, Vector2 point3, Vector2 point4)
        {
            var shape = new ViewClipShape(point1, point2, point3, point4);
            shape.BuildAxis();
            return shape;
        }

        public static ViewClipShape FromPoints(Vector2 point1, Vector2 point2, Vector2 point3, Vector2 point4, Vector2 point5)
        {
            var shape = new ViewClipShape(point1, point2, point3, point4, point5);
            shape.BuildAxis();
            return shape;
        }

        public static ViewClipShape FromPoints(Vector2 point1, Vector2 point2, Vector2 point3, Vector2 point4, Vector2 point5, Vector2 point6)
        {

            var shape = new ViewClipShape(point1, point2, point3, point4, point5, point6);
            shape.BuildAxis();
            return shape;
        }

        public Vector2 GetAxis(int index)
        {
            if (index < 0 || index > (Count - 1))
                throw new ArgumentException("Index must be in the range of 0 to " + (Count - 1));

            switch (index)
            {
                case 0: return Axis1;
                case 1: return Axis2;
                case 2: return Axis3;
                case 3: return Axis4;
                case 4: return Axis5;
                default: return Axis6;
            }
        }

        void SetMaxScalar(int index, int value)
        {
            switch (index)
            {
                case 0:
                    A1Max = value;
                    break;
                case 1:
                    A2Max = value;
                    break;
                case 2:
                    A3Max = value;
                    break;
                case 3:
                    A4Max = value;
                    break;
                case 4:
                    A5Max = value;
                    break;
                case 5:
                    A6Max = value;
                    break;
            }
        }

        void SetMinScalar(int index, int value)
        {
            switch (index)
            {
                case 0:
                    A1Min = value;
                    break;
                case 1:
                    A2Min = value;
                    break;
                case 2:
                    A3Min = value;
                    break;
                case 3:
                    A4Min = value;
                    break;
                case 4:
                    A5Min = value;
                    break;
                case 5:
                    A6Min = value;
                    break;
            }

        }

        public int GetMinScalar(int index)
        {
            switch (index)
            {
                case 0: return A1Min;
                case 1: return A2Min;
                case 2: return A3Min;
                case 3: return A4Min;
                case 4: return A5Min;
                case 5: return A6Min;
            }

            return -1;
        }

        public int GetMaxScalar(int index)
        {
            switch (index)
            {
                case 0: return A1Max;
                case 1: return A2Max;
                case 2: return A3Max;
                case 3: return A4Max;
                case 4: return A5Max;
                case 5: return A6Max;
            }

            return -1;
        }

        void BuildAxis()
        {
            Axis1 = Point1 - Point2;
            Axis2 = Point2 - Point3;

            switch (Count)
            {
                case 3:
                    Axis3 = Point3 - Point1;
                    break;
                case 4:
                    Axis3 = Point3 - Point4;
                    Axis4 = Point4 - Point1;
                    break;
                case 5:
                    Axis3 = Point3 - Point4;
                    Axis4 = Point4 - Point5;
                    Axis5 = Point5 - Point1;
                    break;
                case 6:
                    Axis3 = Point3 - Point4;
                    Axis4 = Point4 - Point5;
                    Axis5 = Point5 - Point6;
                    Axis6 = Point6 - Point1;
                    break;
            }

            SetScalars();
        }

        void SetScalars()
        {
            for (var i = 0; i < Count; i++)
            {
                int min, max;
                var axis = GetAxis(i);
                Intersection.Project(axis, ref this, out min, out max);

                SetMinScalar(i, min);
                SetMaxScalar(i, max);
            }
        }

        public Vector2 GetViewPoint(Vector3 cameraPosition)
        {
            //Move the camera position to the same Y axis as the ViewClipShape
            var p1 = new Vector2(cameraPosition.X, cameraPosition.Z);

            if (ContainsPoint(p1))
                return p1;

            //Find the center position of the clip shape
            var p2xmin = Point1.X;
            var p2xmax = p2xmin;
            var p2ymin = Point1.Y;
            var p2ymax = p2ymin;

            //Closest points to camera location
            var p3 = Point1;
            var dist1 = Vector2.Distance(p1, Point1);
            var dist2 = dist1;
            var p4 = p3;

            for (var i = 1; i < Count; i++)
            {
                var current = this[i];

                var dist = Vector2.Distance(p1, current);

                if (dist <= dist1)
                    p3 = current;
                else if (dist <= dist2)
                    p4 = current;

                if (current.X <= p2xmin)
                    p2xmin = current.X;
                else
                    if (current.X > p2xmax)
                        p2xmax = current.X;

                if (current.Y <= p2ymin)
                    p2ymin = current.Y;
                else
                    if (current.Y > p2ymax)
                        p2ymax = current.Y;
            }

            //Point 2 is the center of the shape.  
            //Line 1 is the camera point to the center of the clip shape
            var p2 = new Vector2((p2xmin + p2xmax) / 2, (p2ymin + p2ymax) / 2);

            //Line 2 is a line drawn between the closest two points to the camera point
            //These are denoted by cp1 and cp2.

            var numerator = (p4.X - p3.X) * (p1.Y - p3.Y) - (p4.Y - p3.Y) * (p1.X - p3.X);
            var denominator = (p4.Y - p3.Y) * (p2.X - p1.X) - (p4.X - p3.X) * (p2.Y - p1.Y);

            var ua = numerator / denominator;

            var x = p1.X + ua * (p2.X - p1.X);
            var y = p1.Y + ua * (p2.Y - p1.Y);

            return new Vector2(x, y);

        }

        public void ReplacePoint(int pointIndex, Vector2 newPoint)
        {
            switch (pointIndex)
            {
                case 0:
                    Point1 = newPoint;
                    break;
                case 1:
                    Point2 = newPoint;
                    break;
                case 2:
                    Point3 = newPoint;
                    break;
                case 3:
                    Point4 = newPoint;
                    break;
                case 4:
                    Point5 = newPoint;
                    break;
                case 5:
                    Point6 = newPoint;
                    break;
            }

            //rebuild the axis
            BuildAxis();
        }

        public void InsertPointAt(int pointIndex, Vector2 newPoint)
        {
            if (pointIndex < 0 || pointIndex > 4)
                return;

            switch (pointIndex)
            {
                case 0:
                    Point6 = Point5;
                    Point5 = Point4;
                    Point4 = Point3;
                    Point3 = Point2;
                    Point2 = Point1;
                    Point1 = newPoint;
                    break;
                case 1:
                    Point6 = Point5;
                    Point5 = Point4;
                    Point4 = Point3;
                    Point3 = Point2;
                    Point2 = newPoint;
                    break;
                case 2:
                    Point6 = Point5;
                    Point5 = Point4;
                    Point4 = Point3;
                    Point3 = newPoint;
                    break;
                case 3:
                    Point6 = Point5;
                    Point5 = Point4;
                    Point4 = newPoint;
                    break;
                case 4:
                    Point6 = Point5;
                    Point5 = newPoint;
                    break;
                case 5:
                    Point6 = newPoint;
                    break;

            }

            Count++;
            //rebuild the axis
            BuildAxis();
        }

        /// <summary>
        /// Adapted from http://paulbourke.net/geometry/insidepoly/
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool ContainsPoint(Vector2 point)
        {
            var counter = 0;
            int i;
            Vector2 p2;

            var p1 = this[0];
            for (i = 1; i <= Count; i++)
            {
                p2 = this[i % Count];
                if (point.Y > Math.Min(p1.Y, p2.Y))
                {
                    if (point.Y <= Math.Max(p1.Y, p2.Y))
                    {
                        if (point.X <= Math.Max(p1.X, p2.X))
                        {
                            if (p1.Y != p2.Y)
                            {
                                float xinters = (point.Y - p1.Y) * (p2.X - p1.X) / (p2.Y - p1.Y) + p1.X;
                                if (p1.X == p2.X || point.X <= xinters)
                                    counter++;
                            }
                        }
                    }
                }
                p1 = p2;
            }

            return counter % 2 != 0;
        }
    }
}
