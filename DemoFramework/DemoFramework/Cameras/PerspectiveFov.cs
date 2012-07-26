#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework.Cameras
{
    /// <summary>
    /// 視野に基づいた Perspective 行列を管理するクラスです。
    /// </summary>
    public sealed class PerspectiveFov : ProjectionBase
    {
        /// <summary>
        /// デフォルトの FOV。
        /// </summary>
        public const float DefaultFov = MathHelper.PiOver4;

        /// <summary>
        /// 1 : 1 のアスペクト比。
        /// </summary>
        /// <remarks>
        /// この比率は、Shadow Map 生成で用いるライト カメラのアスペクト比などで利用されます。
        /// </remarks>
        public const float AspectRatio1x1 = 1.0f;

        /// <summary>
        /// 4 : 3 のアスペクト比。
        /// </summary>
        /// <remarks>
        /// この比率は、解像度 800x600 のスクリーンに相当します。
        /// </remarks>
        public const float AspectRatio4x3 = 4.0f / 3.0f;

        /// <summary>
        /// 16 : 9 のアスペクト比。
        /// </summary>
        /// <remarks>
        /// この比率は、解像度 1280x720 のスクリーンに相当します。
        /// </remarks>
        public const float AspectRatio16x9 = 16.0f / 9.0f;

        /// <summary>
        ///  y 方向の視野角 (ラジアン単位)
        /// </summary>
        float fov = DefaultFov;

        /// <summary>
        /// アスペクト比。
        /// </summary>
        float aspectRatio = AspectRatio4x3;

        /// <summary>
        /// y 方向の視野角 (ラジアン単位) を取得または設定します。
        /// </summary>
        public float Fov
        {
            get { return fov; }
            set
            {
                if (fov == value) return;

                fov = value;
                MatrixDirty = true;
            }
        }

        /// <summary>
        /// アスペクト比を取得または設定します。
        /// </summary>
        public float AspectRatio
        {
            get { return aspectRatio; }
            set
            {
                if (aspectRatio == value) return;

                aspectRatio = value;
                MatrixDirty = true;
            }
        }

        protected override void UpdateOverride()
        {
            Matrix.CreatePerspectiveFieldOfView(Fov, AspectRatio, NearPlaneDistance, FarPlaneDistance, out Matrix);
            MatrixDirty = false;
        }
    }
}
