#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace PSSMDemo
{
    public static class RayHelper
    {
        /// <summary>
        /// レイの始点からレイに沿って指定のサイズにある点を取得します。
        /// </summary>
        /// <param name="ray">レイ。</param>
        /// <param name="size">サイズ。</param>
        /// <returns>指定の点。</returns>
        public static Vector3 GetPoint(ref Ray ray, float size)
        {
            Vector3 result;
            GetPoint(ref ray, size, out result);
            return result;
        }

        /// <summary>
        /// レイの始点からレイに沿って指定の長さにある点を取得します。
        /// </summary>
        /// <param name="ray">レイ。</param>
        /// <param name="size">サイズ。</param>
        /// <param name="result">指定の点。</param>
        public static void GetPoint(ref Ray ray, float size, out Vector3 result)
        {
            Vector3 vector;
            Vector3.Multiply(ref ray.Direction, size, out vector);

            Vector3.Add(ref ray.Position, ref vector, out result);
        }
    }
}
