#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

#endregion

namespace TerrainDemo
{
    public sealed class FreeView : ViewBase
    {
        const float twoPi = 2 * MathHelper.Pi;

        /// <summary>
        /// カメラの姿勢。
        /// </summary>
        Vector3 orientation = Vector3.Zero;

        Vector3 rotationAmount;

        Vector3 moveDirection;

        float moveAmount;

        Vector3 position = Vector3.Zero;

        /// <summary>
        /// カメラの姿勢を取得します。
        /// ベクトルの X 要素は pitch、Y 要素は yaw、Z 要素は roll を表します。
        /// </summary>
        public Vector3 Orientation
        {
            get { return orientation; }
        }

        public Vector3 RotationAmount
        {
            get { return rotationAmount; }
            set
            {
                if (rotationAmount == value) return;

                rotationAmount = value;
                MatrixDirty = true;
            }
        }

        public Vector3 MoveDirection
        {
            get { return moveDirection; }
            set
            {
                if (moveDirection == value) return;

                moveDirection = value;
                MatrixDirty = true;
            }
        }

        public float MoveAmount
        {
            get { return moveAmount; }
            set
            {
                if (moveAmount == value) return;

                moveAmount = value;
                MatrixDirty = true;
            }
        }

        protected override void UpdateOverride()
        {
            // 新しい角度。
            orientation += rotationAmount;
            // 2 * pi 内に収める。
            orientation.X %= twoPi;
            orientation.Y %= twoPi;
            orientation.Z %= twoPi;

            Matrix rotation;
            Matrix.CreateFromYawPitchRoll(orientation.Y, orientation.X, orientation.Z, out rotation);

            if (moveAmount != 0)
            {
                var normalDirection = moveDirection;
                if (normalDirection.X != 0 || normalDirection.Y != 0 || normalDirection.Z != 0)
                {
                    normalDirection.Normalize();

                    Vector3 rotatedDirection;
                    Vector3.Transform(ref normalDirection, ref rotation, out rotatedDirection);

                    position += rotatedDirection * moveAmount;
                }
            }

            var target = position + rotation.Forward;
            var up = rotation.Up;
            Matrix.CreateLookAt(ref position, ref target, ref up, out Matrix);
        }
    }
}
