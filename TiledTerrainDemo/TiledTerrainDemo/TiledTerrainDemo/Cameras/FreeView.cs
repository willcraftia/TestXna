#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Willcraftia.Xna.Framework.Cameras;

#endregion

namespace TiledTerrainDemo.Cameras
{
    public sealed class FreeView : ViewBase
    {
        Vector3 orientation = Vector3.Zero;

        Vector3 position = Vector3.Zero;

        Matrix inverseMatrix;

        /// <summary>
        /// The orientation of the camera.
        /// X = pitch, Y = yaw, Z = roll.
        /// </summary>
        public Vector3 Orientation
        {
            get { return orientation; }
        }

        public Vector3 Position
        {
            get { return position; }
            set
            {
                if (position == value) return;

                position = value;
                MatrixDirty = true;
            }
        }

        public void Move(float distance)
        {
            if (distance == 0) return;

            var direction = inverseMatrix.Forward;
            position += direction * distance;
            MatrixDirty = true;
        }

        public void Strafe(float distance)
        {
            if (distance == 0) return;

            var direction = inverseMatrix.Left;
            position += direction * distance;
            MatrixDirty = true;
        }

        public void Up(float distance)
        {
            if (distance == 0) return;

            var direction = inverseMatrix.Up;
            position += direction * distance;
            MatrixDirty = true;
        }

        public void Yaw(float amount)
        {
            if (amount == 0) return;

            orientation.Y += amount;
            orientation.Y %= MathHelper.TwoPi;
            MatrixDirty = true;
        }

        public void Pitch(float amount)
        {
            if (amount == 0) return;

            orientation.X += amount;
            orientation.X %= MathHelper.TwoPi;
            MatrixDirty = true;
        }

        public void Roll(float amount)
        {
            if (amount == 0) return;

            orientation.Z += amount;
            orientation.Z %= MathHelper.TwoPi;
            MatrixDirty = true;
        }

        protected override void UpdateOverride()
        {
            Matrix rotation;
            Matrix.CreateFromYawPitchRoll(orientation.Y, orientation.X, orientation.Z, out rotation);

            var target = position + rotation.Forward;
            var up = rotation.Up;
            Matrix.CreateLookAt(ref position, ref target, ref up, out Matrix);

            // 逆行列も更新。
            Matrix.Invert(ref Matrix, out inverseMatrix);

            MatrixDirty = false;
        }
    }
}
