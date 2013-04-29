#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace LiSPSMDemo
{
    public static class Vector3Helper
    {
        /// <summary>
        /// 負の Y 軸方向単位ベクトル。
        /// </summary>
        public static readonly Vector3 NegativeUnitY = new Vector3(0, -1, 0);

        /// <summary>
        /// ベクトルを同次変換します。
        /// 座標系の変換では、通常の Vector3.Transform ではなく、
        /// 同時変換を用いる必要があります。
        /// </summary>
        /// <param name="vector">ベクトル。</param>
        /// <param name="matrix">変換行列。</param>
        /// <param name="result">同次変換されたベクトル。</param>
        public static void TransformCoordinate(ref Vector3 vector, ref Matrix matrix, out Vector3 result)
        {
            Vector4 transformed;
            Vector4.Transform(ref vector, ref matrix, out transformed);

            Vector4 homogeneous;
            Vector4.Divide(ref transformed, transformed.W, out homogeneous);

            result = new Vector3(homogeneous.X, homogeneous.Y, homogeneous.Z);
        }
    }
}
