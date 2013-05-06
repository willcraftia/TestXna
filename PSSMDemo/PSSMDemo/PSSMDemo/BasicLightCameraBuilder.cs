#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace PSSMDemo
{
    /// <summary>
    /// ライト カメラを簡易に構築するクラスです。
    /// </summary>
    /// <remarks>
    /// このクラスは、表示カメラの視錐台を元にシーン領域を決定し、
    /// ライト空間行列の算出に利用します。
    /// </remarks>
    public sealed class BasicLightCameraBuilder : LightCameraBuilder
    {
        Vector3[] corners;

        /// <summary>
        /// ライトのある方向へシーン領域を押し出す距離を取得または設定します。
        /// </summary>
        /// <remarks>
        /// ライトのある方向へシーン領域を押し出すことで、
        /// 視錐台の外に位置する投影オブジェクトをシーン領域へ含める事ができます。
        /// </remarks>
        public float SceneExtrudeDistance { get; set; }

        public BasicLightCameraBuilder()
        {
            corners = new Vector3[8];

            SceneExtrudeDistance = 500.0f;
        }

        protected override void BuildCore(out Matrix lightView, out Matrix lightProjection)
        {
            // ライトの仮位置と仮 UP ベクトル。
            var position = Vector3.Zero;
            var up = Vector3.Up;

            const float zeroTolerance = 1e-6f;

            // ライト方向と仮 UP ベクトルが並行な場合、他の軸を利用。
            float dot;
            Vector3.Dot(ref up, ref lightDirection, out dot);
            if ((1 - Math.Abs(dot) < zeroTolerance))
            {
                up = Vector3.Forward;
            }

            // 仮ライト ビュー行列。
            MatrixHelper.CreateLook(ref position, ref lightDirection, ref up, out lightView);

            // ライト位置へ向かうレイ。
            //var ray = new Ray();
            //Vector3.Negate(ref lightDirection, out ray.Direction);

            // 仮ライト ビュー空間における表示カメラの境界錐台を包む境界ボックス。
            var boxLV = BoundingBoxHelper.Empty;
            eyeFrustum.GetCorners(corners);
            for (int i = 0; i < corners.Length; i++)
            {
                Vector3 cornerLV;
                Vector3.Transform(ref corners[i], ref lightView, out cornerLV);

                BoundingBoxHelper.Merge(ref boxLV, ref cornerLV);
            }

            // 境界ボックスのサイズを算出。
            Vector3 boxSizeLV;
            BoundingBoxHelper.GetSize(ref boxLV, out boxSizeLV);

            Vector3 halfBoxSizeLV;
            BoundingBoxHelper.GetHalfSize(ref boxLV, out halfBoxSizeLV);

            // 境界ボックスの近平面の中心へライト カメラの位置を合わせる。
            var positionLV = new Vector3(
                boxLV.Min.X + halfBoxSizeLV.X,
                boxLV.Min.Y + halfBoxSizeLV.Y,
                boxLV.Min.Z);

            // 仮ビュー行列の逆行列。
            Matrix invLightView;
            Matrix.Invert(ref lightView, out invLightView);

            // 仮ビュー行列の逆行列を掛ける事で仮ビュー空間におけるライト カメラ位置をワールド空間へ。
            Vector3.Transform(ref positionLV, ref invLightView, out position);

            // 決定したライト カメラ位置によりライトのビュー行列を決定。
            MatrixHelper.CreateLook(ref position, ref lightDirection, ref up, out lightView);

            // ビュー空間における表示カメラの境界錐台を包む境界ボックス。
            boxLV = BoundingBoxHelper.Empty;
            for (int i = 0; i < corners.Length; i++)
            {
                // ビュー空間へ頂点を変換してマージ。
                Vector3 cornerLV;
                Vector3.Transform(ref corners[i], ref lightView, out cornerLV);

                BoundingBoxHelper.Merge(ref boxLV, ref cornerLV);
            }

            // 境界ボックスのある範囲で正射影。
            // ただし、SceneExtrudeDistance に応じて near を後退させておく。
            Matrix.CreateOrthographicOffCenter(
                boxLV.Min.X, boxLV.Max.X,
                boxLV.Min.Y, boxLV.Max.Y,
                -boxLV.Max.Z - SceneExtrudeDistance, -boxLV.Min.Z,
                out lightProjection);
        }
    }
}
