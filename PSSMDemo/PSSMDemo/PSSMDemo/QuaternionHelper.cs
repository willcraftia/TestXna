#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace PSSMDemo
{
    public static class QuaternionHelper
    {
        public static void CreateRotationBetween(ref Vector3 start, ref Vector3 destination, out Quaternion result)
        {
            // http://www.opengl-tutorial.org/intermediate-tutorials/tutorial-17-quaternions/

            var v0 = start;
            var v1 = destination;

            v0.Normalize();
            v1.Normalize();

            float dot;
            Vector3.Dot(ref v0, ref v1, out dot);

            const float zeroTolerance = 1e-6f;

            if (1.0f <= dot)
            {
                // 同じベクトルならば回転無し。
                result = Quaternion.Identity;
            }
            else if (dot < (zeroTolerance - 1.0f))
            {
                // ベクトル同士の方向が真逆になる場合は特殊。
                // この場合、理想的な回転軸が存在しないため、回転軸の推測を行う。
                // 回転軸は、ベクトル start に対して垂直であれば良い。
                var temp = Vector3.UnitZ;
                Vector3 axis;
                Vector3.Cross(ref temp, ref start, out axis);

                if (axis.LengthSquared() == 0.0f)
                {
                    temp = Vector3.UnitX;
                    Vector3.Cross(ref temp, ref start, out axis);
                }

                axis.Normalize();

                Quaternion.CreateFromAxisAngle(ref axis, MathHelper.Pi, out result);
            }
            else
            {
                float s = (float) Math.Sqrt((1.0f + dot) * 2);
                float invs = 1 / s;

                Vector3 axis;
                Vector3.Cross(ref start, ref destination, out axis);

                result.X = axis.X * invs;
                result.Y = axis.Y * invs;
                result.Z = axis.Z * invs;
                result.W = s * 0.5f;
            }
        }
    }
}
