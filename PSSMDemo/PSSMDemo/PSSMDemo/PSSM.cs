#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace PSSMDemo
{
    /// <summary>
    /// PSSM (Parallel Split Shadow Maps) による視錐台分割を担うクラスです。
    /// </summary>
    public sealed class PSSM
    {
        /// <summary>
        /// 分割数。
        /// </summary>
        int count;

        /// <summary>
        /// 視野角。
        /// </summary>
        float fov;

        /// <summary>
        /// アスペクト比。
        /// </summary>
        float aspectRatio;

        /// <summary>
        /// 近クリップ面距離。
        /// </summary>
        float nearClipDistance;

        /// <summary>
        /// 遠クリップ面距離。
        /// </summary>
        float farClipDistance;

        /// <summary>
        /// ラムダ値。
        /// </summary>
        float lambda;

        /// <summary>
        /// ビュー行列。
        /// </summary>
        Matrix view;

        /// <summary>
        /// シーン領域。
        /// </summary>
        BoundingBox sceneBox;

        Vector3[] corners;

        /// <summary>
        /// 分割数を取得または設定します。
        /// </summary>
        public int Count
        {
            get { return count; }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException("value");

                count = value;
            }
        }

        /// <summary>
        /// 視野角を取得または設定します。
        /// </summary>
        public float Fov
        {
            get { return fov; }
            set
            {
                if (value <= 0.0f || MathHelper.Pi <= value) throw new ArgumentOutOfRangeException("value");

                fov = value;
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
                if (value <= 0.0f) throw new ArgumentOutOfRangeException("value");

                aspectRatio = value;
            }
        }

        /// <summary>
        /// 近クリップ面距離を取得または設定します。
        /// </summary>
        public float NearClipDistance
        {
            get { return nearClipDistance; }
            set
            {
                if (nearClipDistance < 0.0f) throw new ArgumentOutOfRangeException("value");

                nearClipDistance = value;
            }
        }

        /// <summary>
        /// 遠クリップ面距離を取得または設定します。
        /// </summary>
        public float FarClipDistance
        {
            get { return farClipDistance; }
            set
            {
                if (farClipDistance < 0.0f) throw new ArgumentOutOfRangeException("value");

                farClipDistance = value;
            }
        }

        /// <summary>
        /// ラムダ値を取得または設定します。
        /// </summary>
        /// <remarks>
        /// ラムダ値は [0, 1] の範囲で分割の形式を決定します。
        /// 0 に近い程に均等な分割になり、1 に近い程に対数的な分割
        /// (カメラ手前の分割範囲が狭く、奥に行く程に分割範囲が広がる) になります。
        /// </remarks>
        public float Lambda
        {
            get { return lambda; }
            set
            {
                if (value < 0.0f || 1.0f < value) throw new ArgumentOutOfRangeException("value");

                lambda = value;
            }
        }

        /// <summary>
        /// ビュー行列を取得または設定します。
        /// </summary>
        public Matrix View
        {
            get { return view; }
            set { view = value; }
        }

        /// <summary>
        /// シーン領域を取得または設定します。
        /// </summary>
        public BoundingBox SceneBox
        {
            get { return sceneBox; }
            set { sceneBox = value; }
        }

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        public PSSM()
        {
            lambda = 0.5f;

            corners = new Vector3[BoundingBox.CornerCount];
        }

        /// <summary>
        /// 視錐台を分割します。
        /// </summary>
        /// <param name="distances">分割された視錐台の各距離。</param>
        /// <param name="projections">分割された視錐台の各射影行列。</param>
        public void Split(float[] distances, Matrix[] projections)
        {
            if (distances == null) throw new ArgumentNullException("distances");
            if (projections == null) throw new ArgumentNullException("projections");
            if (distances.Length < count + 1) throw new ArgumentException("Insufficient size.", "distances");
            if (projections.Length < count) throw new ArgumentException("Insufficient size.", "projections");

            // シーン領域を含みうる最小限の遠クリップ面を算出。
            float adjustedFar = CalculateFarClipDistance(ref view, ref sceneBox, nearClipDistance);
            adjustedFar = Math.Min(farClipDistance, adjustedFar);

            // 近クリップ面から遠クリップ面の範囲を分割。
            SplitDistance(nearClipDistance, adjustedFar, distances);

            // 分割毎に射影行列を構築。
            for (int i = 0; i < count; i++)
            {
                Matrix.CreatePerspectiveFieldOfView(fov, aspectRatio, distances[i], distances[i + 1], out projections[i]);
            }
        }

        float CalculateFarClipDistance(ref Matrix view, ref BoundingBox sceneBox, float nearClipDistance)
        {
            // シーン AABB の頂点の中で最もカメラから遠い点の z 値を探す。

            float maxFarZ = 0.0f;

            sceneBox.GetCorners(corners);
            for (int i = 0; i < corners.Length; i++)
            {
                // z についてのみビュー座標へ変換。
                float z =
                    corners[i].X * view.M13 +
                    corners[i].Y * view.M23 +
                    corners[i].Z * view.M33 +
                    view.M43;

                // より小さな値がより遠くの点。
                if (z < maxFarZ) maxFarZ = z;
            }

            // maxFarZ の符号を反転させて距離を算出。
            return nearClipDistance - maxFarZ;
        }

        void SplitDistance(float nearClipDistance, float farClipDistance, float[] distances)
        {
            if (count == 1)
            {
                // 分割無しの場合における最適化。
                distances[0] = nearClipDistance;
                distances[1] = farClipDistance;
                return;
            }

            float n = nearClipDistance;
            float f = farClipDistance;
            float m = count;

            float fdn = f / n;
            float fsn = f - n;
            float invLambda = 1.0f - lambda;

            for (int i = 0; i < count + 1; i++)
            {
                float idm = i / m;

                // CL = n * (f / n)^(i / m)
                // CU = n + (f - n) * (i / m)
                // C = CL * lambda + CU * (1 - lambda)

                // CL は対数分割での値。
                // CU は均等分割での値。
                // パラメータ lambda で対数分割の程度を調整。
                // lambda = 1 ならば対数分割のみ。
                // lambda = 0 ならば均等分割のみ。

                float log = n * (float) Math.Pow(fdn, idm);
                float uniform = n + fsn * idm;
                distances[i] = log * lambda + uniform * invLambda;
            }

            distances[0] = n;
            distances[distances.Length - 1] = f;
        }
    }
}
