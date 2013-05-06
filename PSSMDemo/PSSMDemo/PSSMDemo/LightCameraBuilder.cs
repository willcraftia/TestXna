#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace PSSMDemo
{
    /// <summary>
    /// ライト カメラの構築を担うクラスの抽象基底クラスです。
    /// </summary>
    public abstract class LightCameraBuilder
    {
        #region DirtyFlags

        /// <summary>
        /// ダーティ フラグ。
        /// </summary>
        [Flags]
        enum DirtyFlags
        {
            /// <summary>
            /// 表示カメラのビュー行列に関するプロパティがダーティ。
            /// </summary>
            EyeViewProperties = (1 << 0),

            /// <summary>
            /// 表示カメラの射影行列に関するプロパティがダーティ。
            /// </summary>
            EyeProjectionProperties = (1 << 1),

            /// <summary>
            /// 表示カメラの境界錐台がダーティ。
            /// </summary>
            EyeFrustum = (1 << 2),
        }

        #endregion

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
        protected Matrix invertEyeView;

        /// <summary>
        /// 表示カメラの射影行列の種類。
        /// </summary>
        protected ProjectionType eyeProjectionType;

        /// <summary>
        /// 表示カメラの射影行列の左クリップ面位置。
        /// </summary>
        protected float eyeProjectionLeft;

        /// <summary>
        /// 表示カメラの射影行列の右クリップ面位置。
        /// </summary>
        protected float eyeProjectionRight;

        /// <summary>
        /// 表示カメラの射影行列の下クリップ面位置。
        /// </summary>
        protected float eyeProjectionBottom;

        /// <summary>
        /// 表示カメラの射影行列の上クリップ面位置。
        /// </summary>
        protected float eyeProjectionTop;

        /// <summary>
        /// 表示カメラの射影行列の近クリップ面位置。
        /// </summary>
        protected float eyeProjectionNear;

        /// <summary>
        /// 表示カメラの射影行列の遠クリップ面位置。
        /// </summary>
        protected float eyeProjectionFar;

        /// <summary>
        /// 表示カメラの射影行列の視野角。
        /// </summary>
        protected float eyeProjectionFov;

        /// <summary>
        /// 表示カメラの射影行列のアスペクト比。
        /// </summary>
        protected float eyeProjectionAspectRatio;

        /// <summary>
        /// 表示カメラの境界錐台。
        /// </summary>
        protected BoundingFrustum eyeFrustum;

        /// <summary>
        /// ライトの方向 (進行方向)。
        /// </summary>
        protected Vector3 lightDirection;

        /// <summary>
        /// ダーティ フラグ。
        /// </summary>
        DirtyFlags dirtyFlags;

        /// <summary>
        /// 表示シーン領域を取得または設定します。
        /// </summary>
        public BoundingBox SceneBox
        {
            get { return sceneBox; }
            set { sceneBox = value; }
        }

        /// <summary>
        /// 表示カメラのビュー行列を取得または設定します。
        /// </summary>
        public Matrix EyeView
        {
            get { return eyeView; }
            set
            {
                eyeView = value;

                dirtyFlags |= DirtyFlags.EyeViewProperties | DirtyFlags.EyeFrustum;
            }
        }

        /// <summary>
        /// 表示カメラの射影行列を取得または設定します。
        /// </summary>
        public Matrix EyeProjection
        {
            get { return eyeProjection; }
            set
            {
                eyeProjection = value;

                dirtyFlags |= DirtyFlags.EyeProjectionProperties | DirtyFlags.EyeFrustum;
            }
        }

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

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        protected LightCameraBuilder()
        {
            eyeView = Matrix.Identity;
            eyeProjection = Matrix.Identity;
            eyeFrustum = new BoundingFrustum(Matrix.Identity);

            dirtyFlags |= DirtyFlags.EyeViewProperties | DirtyFlags.EyeProjectionProperties | DirtyFlags.EyeFrustum;
        }

        /// <summary>
        /// ライトのビュー行列および射影行列を構築します。
        /// </summary>
        /// <param name="lightView">ライトのビュー行列。</param>
        /// <param name="lightProjection">ライトの射影行列。</param>
        public void Build(out Matrix lightView, out Matrix lightProjection)
        {
            UpdateEyeViewProperties();
            UpdateEyeProjectionProperties();
            UpdateEyeFrustum();

            BuildCore(out lightView, out lightProjection);
        }

        /// <summary>
        /// ライトのビュー行列および射影行列を構築します。
        /// Build メソッドは、表示カメラのビュー行列および射影行列より、
        /// それらを構成するプロパティを抽出した後、このメソッドを呼び出します。
        /// </summary>
        /// <param name="lightView"></param>
        /// <param name="lightProjection"></param>
        protected abstract void BuildCore(out Matrix lightView, out Matrix lightProjection);

        /// <summary>
        /// 表示カメラのビュー行列を構成する要素を抽出します。
        /// </summary>
        void UpdateEyeViewProperties()
        {
            if ((dirtyFlags & DirtyFlags.EyeViewProperties) != 0)
            {
                Matrix.Invert(ref eyeView, out invertEyeView);

                eyePosition = invertEyeView.Translation;
                eyeDirection = invertEyeView.Forward;
                eyeDirection.Normalize();
                eyeUp = invertEyeView.Up;
                eyeUp.Normalize();

                dirtyFlags &= ~DirtyFlags.EyeViewProperties;
            }
        }

        /// <summary>
        /// 表示カメラの射影行列を構成する要素を抽出します。
        /// </summary>
        void UpdateEyeProjectionProperties()
        {

            if ((dirtyFlags & DirtyFlags.EyeProjectionProperties) != 0)
            {

                // eyeProjection に正しい射影行列が設定されている事を仮定。
                if (eyeProjection.M44 == 1.0f)
                {
                    // 正射影。
                    eyeProjectionType = ProjectionType.Orthographic;
                    MatrixHelper.ExtractOrthographic(
                        ref eyeProjection,
                        out eyeProjectionLeft,
                        out eyeProjectionRight,
                        out eyeProjectionBottom,
                        out eyeProjectionTop,
                        out eyeProjectionNear,
                        out eyeProjectionFar);

                    eyeProjectionFov = float.NaN;
                    eyeProjectionAspectRatio = float.NaN;
                }
                else
                {
                    // 透視射影。
                    eyeProjectionType = ProjectionType.Perspective;
                    MatrixHelper.ExtractPerspective(
                        ref eyeProjection,
                        out eyeProjectionFov,
                        out eyeProjectionAspectRatio,
                        out eyeProjectionLeft,
                        out eyeProjectionRight,
                        out eyeProjectionBottom,
                        out eyeProjectionTop,
                        out eyeProjectionNear,
                        out eyeProjectionFar);
                }

                dirtyFlags &= ~DirtyFlags.EyeProjectionProperties;
            }
        }

        /// <summary>
        /// 表示カメラの境界錐台を更新します。
        /// </summary>
        void UpdateEyeFrustum()
        {
            if ((dirtyFlags & DirtyFlags.EyeFrustum) != 0)
            {
                Matrix eyeViewProjection;
                Matrix.Multiply(ref eyeView, ref eyeProjection, out eyeViewProjection);

                eyeFrustum.Matrix = eyeViewProjection;

                dirtyFlags &= ~DirtyFlags.EyeFrustum;
            }
        }
    }
}
