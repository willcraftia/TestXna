#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace PSSMDemo
{
    public static class BoundingBoxHelper
    {
        public static readonly BoundingBox Empty = new BoundingBox(new Vector3(float.MaxValue), new Vector3(float.MinValue));

        public static Vector3 GetSize(BoundingBox box)
        {
            Vector3 result;
            GetSize(ref box, out result);
            return result;
        }

        public static BoundingBox CreateMerged(BoundingBox original, Vector3 additional)
        {
            BoundingBox result;
            CreateMerged(ref original, ref additional, out result);
            return result;
        }

        public static void CreateMerged(ref BoundingBox original, ref Vector3 additional, out BoundingBox result)
        {
            Vector3.Min(ref original.Min, ref additional, out result.Min);
            Vector3.Max(ref original.Max, ref additional, out result.Max);
        }

        public static void Merge(ref BoundingBox box, Vector3 additional)
        {
            CreateMerged(ref box, ref additional, out box);
        }

        public static void Merge(ref BoundingBox box, ref Vector3 additional)
        {
            CreateMerged(ref box, ref additional, out box);
        }

        public static void Merge(ref BoundingBox box, BoundingBox additional)
        {
            Merge(ref box, ref additional);
        }

        public static void Merge(ref BoundingBox box, ref BoundingBox additional)
        {
            BoundingBox.CreateMerged(ref box, ref additional, out box);
        }

        public static void GetSize(ref BoundingBox box, out Vector3 result)
        {
            Vector3.Subtract(ref box.Max, ref box.Min, out result);
        }

        public static Vector3 GetHalfSize(BoundingBox box)
        {
            Vector3 result;
            GetHalfSize(ref box, out result);
            return result;
        }

        public static void GetHalfSize(ref BoundingBox box, out Vector3 result)
        {
            Vector3 size;
            Vector3.Subtract(ref box.Max, ref box.Min, out size);
            Vector3.Divide(ref size, 2, out result);
        }
    }
}
