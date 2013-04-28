#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace LiSPSMDemo
{
    public static class PlaneHelper
    {
        /// <summary>
        /// 指定の法線を持ち、指定の点を含む平面を生成します。
        /// </summary>
        /// <param name="normal">法線。</param>
        /// <param name="point">平面上の点。</param>
        /// <returns>平面。</returns>
        public static Plane CreatePlane(Vector3 normal, Vector3 point)
        {
            float d;
            Vector3.Dot(ref normal, ref point, out d);

            return new Plane(normal, -d);
        }
    }
}
