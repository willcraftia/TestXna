#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace LiSPSMDemo
{
    public abstract class LightCamera
    {
        // 光源カメラのビュー行列
        public Matrix LightView;

        // 光源カメラの射影行列
        public Matrix LightProjection;

        /// <summary>
        /// 表示カメラのビュー行列。
        /// </summary>
        protected Matrix eyeView;

        /// <summary>
        /// 表示カメラの射影行列。
        /// </summary>
        protected Matrix eyeProjection;

        /// <summary>
        /// 表示シーン領域。
        /// </summary>
        protected BoundingBox sceneBox;

        /// <summary>
        /// 表示カメラの位置。
        /// </summary>
        protected Vector3 eyePosition;

        /// <summary>
        /// 表示カメラの方向。
        /// </summary>
        protected Vector3 eyeDirection;

        /// <summary>
        /// 表示カメラの UP ベクトル。
        /// </summary>
        protected Vector3 eyeUp;

        /// <summary>
        /// 表示カメラのビュー行列の逆行列。
        /// </summary>
        protected Matrix inverseEyeView;

        /// <summary>
        /// 表示カメラの視錐台。
        /// </summary>
        protected BoundingFrustum eyeFrustum;

        // ライトの方向 (進行方向)
        protected Vector3 lightDirection;

        /// <summary>
        /// ライトの方向 (進行方向) を取得または設定します。
        /// </summary>
        /// <remarks>
        /// ライト方向は単位ベクトルで指定します。
        /// </remarks>
        public Vector3 LightDirection
        {
            get { return lightDirection; }
            set { lightDirection = value; }
        }

        protected LightCamera()
        {
            LightView = Matrix.Identity;
            LightProjection = Matrix.Identity;
            eyeView = Matrix.Identity;
            eyePosition = Vector3.Zero;
            eyeDirection = Vector3.Forward;
            eyeUp = Vector3.Up;
            eyeFrustum = new BoundingFrustum(Matrix.Identity);
            lightDirection = Vector3.Down;
        }

        public void Update(Matrix eyeView, Matrix eyeProjection, BoundingBox sceneBox)
        {
            this.eyeView = eyeView;
            this.eyeProjection = eyeProjection;
            this.sceneBox = sceneBox;

            Matrix.Invert(ref eyeView, out inverseEyeView);

            eyePosition = inverseEyeView.Translation;
            eyeDirection = inverseEyeView.Forward;
            eyeDirection.Normalize();
            eyeUp = inverseEyeView.Up;
            eyeUp.Normalize();

            Matrix eyeViewProjection;
            Matrix.Multiply(ref eyeView, ref eyeProjection, out eyeViewProjection);

            eyeFrustum.Matrix = eyeViewProjection;

            Update();
        }

        protected abstract void Update();
    }
}
