#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace PSSMDemo
{
    public static class MatrixHelper
    {
        /// <summary>
        /// カメラの位置、視線方向、UP ベクトルでビュー行列を生成します。
        /// これは、Matrix.CreateLookAt(position, position + direction, up) と等価です。
        /// なお、Matrix.CreateLookAt はその内部で position と target から direction を算出しています。
        /// </summary>
        /// <param name="position"></param>
        /// <param name="direction"></param>
        /// <param name="up"></param>
        /// <param name="result"></param>
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

        public static bool ExtractOrthographic(
            ref Matrix matrix,
            out float left,
            out float right,
            out float bottom,
            out float top,
            out float nearPlaneDistance,
            out float farPlaneDistance)
        {
            const float zeroTolerance = 1e-6f;

            // ゼロ除算になるケースは分解不能で終わらせる。
            if (Math.Abs(matrix.M11) < zeroTolerance ||
                Math.Abs(matrix.M22) < zeroTolerance ||
                Math.Abs(matrix.M33) < zeroTolerance)
            {
                left = float.NaN;
                right = float.NaN;
                bottom = float.NaN;
                top = float.NaN;
                nearPlaneDistance = float.NaN;
                farPlaneDistance = float.NaN;

                return false;
            }

            // 参考:
            //      M43 = near * M33
            //      near = M43 / M33
            nearPlaneDistance = matrix.M43 / matrix.M33;

            // 参考:
            //      M33 = 1 / (near - far)
            //      M43 = near * M33
            //      far = (M43 - 1) / M33
            farPlaneDistance = (matrix.M43 - 1.0f) / matrix.M33;

            // 参考:
            //      M11 = 2 / (right - left)
            //      M41 = (left + right) / (left - right)
            //      left = -(M41 + 1) / M11
            left = -(matrix.M41 + 1.0f) / matrix.M11;

            // 参考:
            //      M11 = 2 / (right - left)
            //      M41 = (right + left) / (right - left)
            //      right = -(M41 - 1) / M11
            right = -(matrix.M41 - 1.0f) / matrix.M11;

            // 参考:
            //      M22 = 2 / (top - bottom)
            //      M42 = (top + bottom) / (top - bottom)
            //      bottom = -(M42 + 1) / M22
            bottom = -(matrix.M42 + 1.0f) / matrix.M22;

            // 参考:
            //      M22 = 2 / (top - bottom)
            //      M42 = (top + bottom) / (top - bottom)
            //      top = -(M42 - 1) / M22
            top = -(matrix.M42 - 1.0f) / matrix.M22;

            return true;
        }

        public static bool ExtractPerspective(
            ref Matrix matrix,
            out float fieldOfView,
            out float aspectRatio,
            out float left,
            out float right,
            out float bottom,
            out float top,
            out float nearPlaneDistance,
            out float farPlaneDistance)
        {
            const float zeroTolerance = 1e-6f;

            // ゼロ除算になるケースは抽出失敗で終わらせる。
            if (Math.Abs(matrix.M11) < zeroTolerance ||
                Math.Abs(matrix.M22) < zeroTolerance ||
                Math.Abs(matrix.M33) < zeroTolerance ||
                Math.Abs(matrix.M33 + 1.0f) < zeroTolerance)
            {
                fieldOfView = float.NaN;
                aspectRatio = float.NaN;
                left = float.NaN;
                right = float.NaN;
                bottom = float.NaN;
                top = float.NaN;
                nearPlaneDistance = float.NaN;
                farPlaneDistance = float.NaN;

                return false;
            }

            // 参考:
            //      M43 = near * M33
            //      near = M43 / M33
            nearPlaneDistance = matrix.M43 / matrix.M33;

            // 参考:
            //      M33 = far / (near - far)
            //      M43 = near * M33
            //      far = M43 / (M33 + 1)
            farPlaneDistance = matrix.M43 / (matrix.M33 + 1.0f);

            // 参考:
            //      M11 = 2 * near / (right - left)
            //      M31 = (right + left) / (right - left)
            //      left = near * (M31 - 1) / M11
            left = nearPlaneDistance * (matrix.M31 - 1.0f) / matrix.M11;

            // 参考:
            //      M11 = 2 * near / (right - left)
            //      M31 = (right + left) / (right - left)
            //      right = near * (M31 + 1) / M11
            right = nearPlaneDistance * (matrix.M31 + 1.0f) / matrix.M11;

            // 参考:
            //      M22 = 2 * near / (top - bottom)
            //      M32 = (top + bottom) / (top - bottom)
            //      bottom = near * (M32 - 1) / M22
            bottom = nearPlaneDistance * (matrix.M32 - 1.0f) / matrix.M22;

            // 参考:
            //      M22 = 2 * near / (top - bottom)
            //      M32 = (top + bottom) / (top - bottom)
            //      top = near * (M32 + 1) / M22
            top = nearPlaneDistance * (matrix.M32 + 1.0f) / matrix.M22;

            // 参考:
            //      M22 = 1 / tan(fov / 2)
            //      fov = atan(1 / M22) * 2
            fieldOfView = (float) Math.Atan(1.0f / matrix.M22) * 2.0f;

            // 参考:
            //      M11 = M22 / aspectRatio
            //      aspectRatio = M11 / M22
            aspectRatio = matrix.M22 / matrix.M11;

            return true;
        }
    }
}
