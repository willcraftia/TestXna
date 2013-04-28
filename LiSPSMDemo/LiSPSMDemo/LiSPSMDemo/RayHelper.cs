#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace LiSPSMDemo
{
    public static class RayHelper
    {
        public static Vector3 GetPoint(ref Ray ray, float size)
        {
            Vector3 result;
            GetPoint(ref ray, size, out result);
            return result;
        }

        public static void GetPoint(ref Ray ray, float size, out Vector3 result)
        {
            Vector3 vector;
            Vector3.Multiply(ref ray.Direction, size, out vector);

            Vector3.Add(ref ray.Position, ref vector, out result);
        }
    }
}
