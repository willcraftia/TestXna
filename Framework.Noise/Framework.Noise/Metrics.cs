#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Noise
{
    // for voronoi.
    public static class Metrics
    {
        /// <summary>
        /// Calculate Euclidean distance squared.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static float Squared(float x, float y, float z)
        {
            return x * x + y * y + z * z;
        }

        /// <summary>
        /// Calculate Euclidean distance.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static float Real(float x, float y, float z)
        {
            return (float) Math.Sqrt(Squared(x, y, z));
        }

        /// <summary>
        /// Calculate Chebychev distance.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static float Chebychev(float x, float y, float z)
        {
            x = Math.Abs(x);
            y = Math.Abs(y);
            z = Math.Abs(z);
            return Math.Max(Math.Max(x, y), z);
        }

        /// <summary>
        /// Calculate Manhattan/Cityblock distance.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static float Manhattan(float x, float y, float z)
        {
            return Math.Abs(x) + Math.Abs(y) + Math.Abs(z);
        }

        /// <summary>
        /// Calculate Minkowski distance, the general case.
        /// 
        /// p = 1, Manhattan distance.
        /// p = 2, Euclidean distance.
        /// p = infinite, Chebychev distance.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static float Minkowski(float x, float y, float z, float p)
        {
            return (float) Math.Pow(Math.Pow(Math.Abs(x), p) + Math.Pow(Math.Abs(y), p) + Math.Pow(Math.Abs(z), p), 1.0f / p);
        }

        /// <summary>
        /// Calculate Minkowski distance with p = 0.5.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static float MinkowskiH(float x, float y, float z)
        {
            float d = (float) (Math.Sqrt(Math.Abs(x)) + Math.Sqrt(Math.Abs(y)) + Math.Sqrt(Math.Abs(z)));
            return d * d;
        }

        /// <summary>
        /// Calculate Minkowski distance with p = 4.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static float Minkowski4(float x, float y, float z)
        {
            x *= x;
            y *= y;
            z *= z;
            return (float) Math.Sqrt(Math.Sqrt(x * x + y * y + z * z));
        }
    }
}
