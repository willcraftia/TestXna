#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace TerrainDemo
{
    internal class Intersection
    {
        internal static bool Intersects(ref BoundingRect shape1, ref ViewClipShape shape2)
        {
            int min, max;

            //Check against rectangle axis
            Project(shape1.Axis1, ref shape2, out min, out max);
            if (shape1.A1Min > max || shape1.A1Max < min)
                return false;

            Project(shape1.Axis2, ref shape2, out min, out max);
            if (shape1.A2Min > max || shape1.A2Max < min)
                return false;

            //Check against the viewclipshape axis - thre are at least 3
            for (var i = 0; i < shape2.Count; i++)
            {
                Project(shape2.GetAxis(i), ref shape1, out min, out max);
                if (shape2.GetMinScalar(i) > max || shape2.GetMaxScalar(i) < min)
                    return false;
            }

            return true;
        }

        internal static void Project(Vector2 axis, ref BoundingRect shape, out int min, out int max)
        {
            min = GenerateScalar(shape.Point1, ref axis);
            max = min;

            var current = GenerateScalar(shape.Point2, ref axis);

            if (current <= min)
                min = current;
            else
                if (current > max)
                    max = current;

            current = GenerateScalar(shape.Point3, ref axis);

            if (current <= min)
                min = current;
            else
                if (current > max)
                    max = current;

            current = GenerateScalar(shape.Point4, ref axis);

            if (current <= min)
                min = current;
            else
                if (current > max)
                    max = current;

        }

        internal static void Project(Vector2 axis, ref ViewClipShape shape, out int min, out int max)
        {
            min = GenerateScalar(shape.Point1, ref axis);
            max = min;

            var current = GenerateScalar(shape.Point2, ref axis);

            if (current <= min)
                min = current;
            else
                if (current > max)
                    max = current;

            current = GenerateScalar(shape.Point3, ref axis);

            if (current <= min)
                min = current;
            else
                if (current > max)
                    max = current;

            if (shape.Count < 4) return;
            current = GenerateScalar(shape.Point4, ref axis);

            if (current <= min)
                min = current;
            else if (current > max)
                max = current;

            if (shape.Count < 5) return;
            current = GenerateScalar(shape.Point5, ref axis);

            if (current <= min)
                min = current;
            else if (current > max)
                max = current;

            if (shape.Count < 6) return;
            current = GenerateScalar(shape.Point6, ref axis);

            if (current <= min)
                min = current;
            else if (current > max)
                max = current;
        }

        /// <summary>
        /// Generate scalar value for axis projection - based on George Clingerman's sample
        /// </summary>
        /// <param name="point">Point to project</param>
        /// <param name="axis">Target Axis</param>
        /// <returns>Generated Scalar</returns>
        internal static int GenerateScalar(Vector2 point, ref Vector2 axis)
        {
            var numerator = (point.X * axis.X) + (point.Y * axis.Y);
            var denominator = (axis.X * axis.X) + (axis.Y * axis.Y);
            var divisionResult = numerator / denominator;

            return (int) ((axis.X * (divisionResult * axis.X)) + (axis.Y * (divisionResult * axis.Y)));
        }
    }
}
