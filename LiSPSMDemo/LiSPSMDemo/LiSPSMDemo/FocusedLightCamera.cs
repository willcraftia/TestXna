#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

#endregion

namespace LiSPSMDemo
{
    /// <summary>
    /// シーン領域への焦点合わせを伴うライト カメラ。
    /// </summary>
    public class FocusedLightCamera : LightCamera
    {
        // y -> -z
        // z -> y
        protected static readonly Matrix NormalToLightSpace = new Matrix(
            1, 0, 0, 0,
            0, 0, 1, 0,
            0, -1, 0, 0,
            0, 0, 0, 1);

        // y -> z
        // z -> -y
        protected static readonly Matrix LightSpaceToNormal = new Matrix(
            1, 0, 0, 0,
            0, 0, -1, 0,
            0, 1, 0, 0,
            0, 0, 0, 1);

        /// <summary>
        /// 凸体 B。
        /// </summary>
        protected ConvexBody bodyB;

        /// <summary>
        /// 凸体 LVS。
        /// </summary>
        protected ConvexBody bodyLVS;

        /// <summary>
        /// 凸体 B の頂点のリスト。
        /// </summary>
        protected List<Vector3> bodyBPoints;

        /// <summary>
        /// 凸体 LVS の頂点のリスト。
        /// </summary>
        protected List<Vector3> bodyLVSPoints;

        Vector3[] corners;

        /// <summary>
        /// 表示カメラの近平面までの距離を取得または設定します。
        /// </summary>
        public float EyeNearDistance { get; set; }

        /// <summary>
        /// 表示カメラの遠平面までの距離を取得または設定します。
        /// </summary>
        public float EyeFarDistance { get; set; }

        /// <summary>
        /// ライトの遠平面までの距離を取得または設定します。
        /// </summary>
        /// <remarks>
        /// この値を 0 より大きくした場合、その距離で凸体 B がクリップされます。
        /// そのようにして影を有効とする範囲を狭めることにより、
        /// シャドウ マップの精度を向上させることができます。
        /// </remarks>
        public float LightFarDistance { get; set; }

        public FocusedLightCamera()
        {
            bodyB = new ConvexBody();
            bodyLVS = new ConvexBody();
            bodyBPoints = new List<Vector3>();
            bodyLVSPoints = new List<Vector3>();
            corners = new Vector3[BoundingBox.CornerCount];

            EyeNearDistance = 1.0f;
            EyeFarDistance = 1000.0f;
            LightFarDistance = 0.0f;
        }

        protected override void Update()
        {
            // 標準的なライト空間行列の算出。
            CalculateStandardLightSpaceMatrices();

            // 凸体 B の算出。
            CalculateBodyB();

            // 凸体 B が空の場合は生成する影が無いため、
            // 先に算出した行列をそのまま利用。
            if (bodyBPoints.Count == 0)
            {
                return;
            }

            // 凸体 LVS の算出。
            CalculateBodyLVS();

            Matrix lightSpace;
            Matrix transform;

            // 軸の変換。
            transform = NormalToLightSpace;
            TransformLightProjection(ref transform);

            // ライト空間におけるカメラ方向へ変換。
            CreateCurrentLightSpace(out lightSpace);
            CreateLightLook(ref lightSpace, out transform);
            TransformLightProjection(ref transform);

            // 単位立方体へ射影。
            CreateCurrentLightSpace(out lightSpace);
            CreateTransformToUnitCube(ref lightSpace, out transform);
            TransformLightProjection(ref transform);

            // 軸の変換 (元へ戻す)。
            transform = LightSpaceToNormal;
            TransformLightProjection(ref transform);

            // DirectX クリッピング空間へ変換。
            Matrix.CreateOrthographicOffCenter(-1, 1, -1, 1, -1, 1, out transform);
            TransformLightProjection(ref transform);
        }

        protected void CalculateStandardLightSpaceMatrices()
        {
            // 方向性光源のための行列。
            MatrixHelper.CreateLook(ref eyePosition, ref lightDirection, ref eyeDirection, out LightView);
            LightProjection = Matrix.Identity;
        }

        protected void CalculateBodyB()
        {
            bodyBPoints.Clear();

            bodyB.Define(eyeFrustum);
            bodyB.Clip(sceneBox);

            var farDistance = LightFarDistance;
            if (0.0f < farDistance)
            {
                var point = eyePosition + eyeDirection * farDistance;
                var plane = PlaneHelper.CreatePlane(eyeDirection, point);
                bodyB.Clip(plane);
            }

            var ray = new Ray();
            ray.Direction = -lightDirection;

            for (int ip = 0; ip < bodyB.Polygons.Count; ip++)
            {
                var polygon = bodyB.Polygons[ip];

                for (int iv = 0; iv < polygon.VertexCount; iv++)
                {
                    Vector3 v;
                    polygon.GetVertex(iv, out v);

                    // 重複頂点を削除するか否か (接する多角形同士の頂点は重複する)。
                    if (!bodyBPoints.Contains(v))
                    {
                        bodyBPoints.Add(v);
                    }

                    Vector3 newPoint;

                    // ライトが存在する方向へレイを伸ばし、シーン AABB との交点を追加。
                    float? intersect;
                    ray.Intersects(ref sceneBox, out intersect);

                    if (intersect != null)
                    {
                        RayHelper.GetPoint(ref ray, intersect.Value, out newPoint);

                        bodyBPoints.Add(newPoint);
                    }
                }
            }
        }

        protected void CalculateBodyLVS()
        {
            bodyLVSPoints.Clear();

            bodyLVS.Define(eyeFrustum);
            bodyLVS.Clip(sceneBox);

            for (int ip = 0; ip < bodyLVS.Polygons.Count; ip++)
            {
                var polygon = bodyLVS.Polygons[ip];

                for (int iv = 0; iv < polygon.VertexCount; iv++)
                {
                    Vector3 v;
                    polygon.GetVertex(iv, out v);

                    // 重複頂点を削除するか否か (接する多角形同士の頂点は重複する)。
                    if (!bodyLVSPoints.Contains(v))
                        bodyLVSPoints.Add(v);
                }
            }
        }

        protected void CreateCurrentLightSpace(out Matrix result)
        {
            Matrix.Multiply(ref LightView, ref LightProjection, out result);
        }

        protected void CreateLightLook(ref Matrix lightSpace, out Matrix result)
        {
            Vector3 position = Vector3.Zero;
            Vector3 up = Vector3.Up;
            Vector3 direction;

            // 焦点合わせ。
            GetCameraDirectionLS(ref lightSpace, out direction);
            MatrixHelper.CreateLook(ref position, ref direction, ref up, out result);
        }

        protected void GetNearCameraPointWS(out Vector3 result)
        {
            // 凸体 LVS から算出。

            if (bodyLVSPoints.Count == 0)
            {
                result = Vector3.Zero;
            }
            else
            {
                Vector3 nearWS = bodyLVSPoints[0];
                Vector3 nearES;
                Vector3Helper.TransformCoordinate(ref nearWS, ref eyeView, out nearES);

                for (int i = 1; i < bodyLVSPoints.Count; i++)
                {
                    Vector3 pointWS = bodyLVSPoints[i];
                    Vector3 pointES;
                    Vector3Helper.TransformCoordinate(ref pointWS, ref eyeView, out pointES);

                    if (nearES.Z < pointES.Z)
                    {
                        nearES = pointES;
                        nearWS = pointWS;
                    }
                }

                result = nearWS;
            }
        }

        protected void GetCameraDirectionLS(ref Matrix lightSpace, out Vector3 result)
        {
            Vector3 e;
            Vector3 b;

            GetNearCameraPointWS(out e);
            b = e + eyeDirection;

            // ライト空間へ変換。
            Vector3 eLS;
            Vector3 bLS;
            Vector3Helper.TransformCoordinate(ref e, ref lightSpace, out eLS);
            Vector3Helper.TransformCoordinate(ref b, ref lightSpace, out bLS);

            // 方向。
            result = bLS - eLS;

            // xz 平面 (シャドウ マップ) に平行 (射影)。
            result.Y = 0.0f;

            // 正規化。
            const float zeroTolerance = 1e-6f;
            if (result.Length() < zeroTolerance)
            {
                // 概ねゼロ ベクトルである場合はデフォルトの前方方向 (0, 0, -1)。
                result = Vector3.Forward;
            }
            else
            {
                result.Normalize();
            }
        }

        protected void CreateTransformToUnitCube(ref Matrix lightSpace, out Matrix result)
        {
            // 凸体 B を収める単位立方体。

            BoundingBox bodyBBox;
            CreateTransformedBodyBBox(ref lightSpace, out bodyBBox);

            CreateTransformToUnitCube(ref bodyBBox.Min, ref bodyBBox.Max, out result);
        }

        void CreateTransformToUnitCube(ref Vector3 min, ref Vector3 max, out Matrix result)
        {
            // 即ち glOrtho と等価。
            // http://msdn.microsoft.com/en-us/library/windows/desktop/dd373965(v=vs.85).aspx
            // ただし、右手系から左手系への変換を省くために z スケールの符号を反転。

            result = new Matrix();

            result.M11 = 2.0f / (max.X - min.X);
            result.M22 = 2.0f / (max.Y - min.Y);
            result.M33 = 2.0f / (max.Z - min.Z);
            result.M41 = -(max.X + min.X) / (max.X - min.X);
            result.M42 = -(max.Y + min.Y) / (max.Y - min.Y);
            result.M43 = -(max.Z + min.Z) / (max.Z - min.Z);
            result.M44 = 1.0f;
        }

        protected void CreateTransformedBodyBBox(ref Matrix matrix, out BoundingBox result)
        {
            result = new BoundingBox(new Vector3(float.MaxValue), new Vector3(float.MinValue));
            for (int i = 0; i < bodyBPoints.Count; i++)
            {
                var point = bodyBPoints[i];

                Vector3 transformed;
                Vector3Helper.TransformCoordinate(ref point, ref matrix, out transformed);

                Vector3.Min(ref result.Min, ref transformed, out result.Min);
                Vector3.Max(ref result.Max, ref transformed, out result.Max);
            }
        }

        protected void TransformLightProjection(ref Matrix matrix)
        {
            Matrix result;
            Matrix.Multiply(ref LightProjection, ref matrix, out result);

            LightProjection = result;
        }
    }
}
