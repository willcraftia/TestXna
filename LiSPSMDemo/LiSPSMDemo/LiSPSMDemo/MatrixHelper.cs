#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace LiSPSMDemo
{
    public static class MatrixHelper
    {
        public static void CreateLook(ref Vector3 position, ref Vector3 direction, ref Vector3 up, out Matrix result)
        {
            Vector3 xaxis, yaxis, zaxis;

            Vector3.Negate(ref direction, out zaxis);
            zaxis.Normalize();

            Vector3.Cross(ref up, ref zaxis, out xaxis);
            xaxis.Normalize();

            Vector3.Cross(ref zaxis, ref xaxis, out yaxis);

            result = Matrix.Identity;

            result.M11 = xaxis.X;
            result.M21 = xaxis.Y;
            result.M31 = xaxis.Z;

            result.M12 = yaxis.X;
            result.M22 = yaxis.Y;
            result.M32 = yaxis.Z;

            result.M13 = zaxis.X;
            result.M23 = zaxis.Y;
            result.M33 = zaxis.Z;

            Vector3.Dot(ref xaxis, ref position, out result.M41);
            Vector3.Dot(ref yaxis, ref position, out result.M42);
            Vector3.Dot(ref zaxis, ref position, out result.M43);

            result.M41 = -result.M41;
            result.M42 = -result.M42;
            result.M43 = -result.M43;
        }
    }
}
