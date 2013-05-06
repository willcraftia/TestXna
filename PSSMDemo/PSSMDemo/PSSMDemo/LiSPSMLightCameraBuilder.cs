#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace PSSMDemo
{
    /// <summary>
    /// LiSPSM のライト カメラを構築するクラスです。
    /// </summary>
    public sealed class LiSPSMLightCameraBuilder : UniformLightCameraBuilder
    {
        /// <summary>
        /// 算出した最適 N 値の調整係数。
        /// </summary>
        float adjustOptimalNFactor;

        /// <summary>
        /// キャッシュされている GetNearCameraPointWS の演算結果。
        /// </summary>
        Vector3 cachedNearCameraPointWS;

        /// <summary>
        /// GetNearCameraPointWS の演算結果のキャッシュが有効であるか否かを示す値。
        /// </summary>
        bool nearCameraPointWSCalculated;

        /// <summary>
        /// 視線とライトが平行であると見做す内積の値 (絶対値) を取得または設定します。
        /// </summary>
        /// <remarks>
        /// AdjustOptimalN = true の場合、
        /// このプロパティ値に基いて視線とライトが並行であるか否かを判定し、
        /// 並行である場合には最適 N 値を調整します。
        /// </remarks>
        public float EyeDotLightThreshold { get; set; }

        /// <summary>
        /// 視線とライトが平行である場合に、最適 N 値を調整するか否かを取得または設定します。
        /// </summary>
        /// <value>
        /// true (最適 N を調整する場合)、false (それ以外の場合)。
        /// </value>
        public bool AdjustOptimalN { get; set; }

        /// <summary>
        /// 明示した N 値を使用するか否かを示す値を取得または設定します。
        /// </summary>
        /// <value>
        /// true (明示した N 値を使用する場合)、false (それ以外の場合)。
        /// </value>
        public bool UseExplicitN { get; set; }

        /// <summary>
        /// 明示する N 値を取得または設定します。
        /// </summary>
        public float ExplicitN { get; set; }

        /// <summary>
        /// 古い最適 N 値算出式を使用するか否かを示す値を取得または設定します。
        /// </summary>
        /// <value>
        /// true (古い最適 N 値算出式を使用する場合)、false (それ以外の場合)。
        /// </value>
        public bool UseOldOptimalNFormula { get; set; }

        public LiSPSMLightCameraBuilder()
        {
            EyeDotLightThreshold = 0.9f;
            AdjustOptimalN = true;
            adjustOptimalNFactor = 1.0f;
            UseExplicitN = false;
            ExplicitN = 0.0f;
            UseOldOptimalNFormula = false;
        }

        protected override void BuildCore(out Matrix lightView, out Matrix lightProjection)
        {
            nearCameraPointWSCalculated = false;

            // 標準的なライト空間行列の算出。
            CalculateStandardLightSpaceMatrices(out lightView, out lightProjection);

            // 凸体 B の算出。
            CalculateBodyB();

            // 凸体 B が空の場合は生成する影が無いため、
            // 算出された行列をそのまま利用。
            if (bodyBPoints.Count == 0)
            {
                return;
            }

            // 凸体 LVS の算出。
            CalculateBodyLVS();

            ResolveAdjustOptimalNFactor();

            Matrix lightSpace;
            Matrix transform;

            // 軸の変換。
            transform = NormalToLightSpace;
            TransformLightProjection(ref lightProjection, ref transform);

            // ライト空間におけるカメラ方向へ変換。
            Matrix.Multiply(ref lightView, ref lightProjection, out lightSpace);
            CreateLightLook(ref lightSpace, out transform);
            TransformLightProjection(ref lightProjection, ref transform);

            // LiSPSM 射影。
            Matrix.Multiply(ref lightView, ref lightProjection, out lightSpace);
            CreateLiSPSMProjection(ref lightSpace, out transform);
            TransformLightProjection(ref lightProjection, ref transform);

            // 単位立方体へ射影。
            Matrix.Multiply(ref lightView, ref lightProjection, out lightSpace);
            CreateTransformToUnitCube(ref lightSpace, out transform);
            TransformLightProjection(ref lightProjection, ref transform);

            // 軸の変換 (元へ戻す)。
            transform = LightSpaceToNormal;
            TransformLightProjection(ref lightProjection, ref transform);

            // DirectX クリッピング空間へ変換。
            Matrix.CreateOrthographicOffCenter(-1, 1, -1, 1, -1, 1, out transform);
            TransformLightProjection(ref lightProjection, ref transform);
        }

        void ResolveAdjustOptimalNFactor()
        {
            adjustOptimalNFactor = 1.0f;

            if (AdjustOptimalN)
            {
                float dot;
                Vector3.Dot(ref eyeDirection, ref lightDirection, out dot);
                dot = Math.Abs(dot);

                // 視線とライトが並行であると見なせる場合。
                if (EyeDotLightThreshold <= dot)
                {
                    adjustOptimalNFactor += 20.0f * (dot - EyeDotLightThreshold) / (1.0f - EyeDotLightThreshold);
                }
            }
        }

        void CreateLiSPSMProjection(ref Matrix lightSpace, out Matrix result)
        {
            // 凸体 B のライト空間における AABB。
            BoundingBox bodyBBoxLS;
            CreateTransformedBodyBBox(ref lightSpace, out bodyBBoxLS);

            // 錐台 P の n (近平面)。
            var n = ResolveN(ref lightSpace, ref bodyBBoxLS);
            if (n <= 0.0f)
            {
                // n が 0 以下になる場合、歪み無しとして処理。
                result = Matrix.Identity;
                return;
            }

            // 錐台 P の d (近平面から遠平面までの距離)。
            var d = Math.Abs(bodyBBoxLS.Max.Z - bodyBBoxLS.Min.Z);

            Vector3 cameraPointWS;
            if (nearCameraPointWSCalculated)
            {
                cameraPointWS = cachedNearCameraPointWS;
            }
            else
            {
                GetNearCameraPointWS(out cameraPointWS);

                cachedNearCameraPointWS = cameraPointWS;
                nearCameraPointWSCalculated = true;
            }

            Vector3 cameraPointLS;
            Vector3Helper.TransformCoordinate(ref cameraPointWS, ref lightSpace, out cameraPointLS);

            // 錐台 P の視点位置。
            var pPositionBase = new Vector3(cameraPointLS.X, cameraPointLS.Y, bodyBBoxLS.Max.Z);
            var pPosition = pPositionBase + n * Vector3.Backward;

            // 錐台 P の視点位置への移動行列。
            var pTranslation = Matrix.CreateTranslation(-pPosition);

            // 錐台 P の透視射影。
            Matrix pPerspective;
            CreatePerspective(-1, 1, -1, 1, n + d, n, out pPerspective);

            // 最終的な LiSPSM 射影行列。
            Matrix.Multiply(ref pTranslation, ref pPerspective, out result);
        }

        float ResolveN(ref Matrix lightSpace, ref BoundingBox bodyBBoxLS)
        {
            if (UseExplicitN)
            {
                return ExplicitN;
            }

            float optimalN;

            if (UseOldOptimalNFormula)
            {
                optimalN = CalculateOldOptimalN();
            }
            else
            {
                optimalN = CalculateOptimalN(ref lightSpace, ref bodyBBoxLS);
            }

            // Ogre3d の LiSPSMShadowCameraSetup に倣い、ほぼ平行と見做す場合に調整。
            // これにより、若干、綺麗になる。
            optimalN *= adjustOptimalNFactor;

            return optimalN;
        }

        float CalculateOptimalN(ref Matrix lightSpace, ref BoundingBox bodyBBoxLS)
        {
            Matrix inverseLightSpace;
            Matrix.Invert(ref lightSpace, out inverseLightSpace);

            Vector3 cameraPointWS;
            if (nearCameraPointWSCalculated)
            {
                cameraPointWS = cachedNearCameraPointWS;
            }
            else
            {
                GetNearCameraPointWS(out cameraPointWS);

                cachedNearCameraPointWS = cameraPointWS;
                nearCameraPointWSCalculated = true;
            }

            // z0 と z1 の算出。

            // ライト空間。
            Vector3 z0LS;
            Vector3 z1LS;
            CalculateZ0LS(ref lightSpace, ref cameraPointWS, ref bodyBBoxLS, out z0LS);
            z1LS = new Vector3(z0LS.X, z0LS.Y, bodyBBoxLS.Min.Z);

            // ワールド空間。
            Vector3 z0WS;
            Vector3 z1WS;
            Vector3Helper.TransformCoordinate(ref z0LS, ref inverseLightSpace, out z0WS);
            Vector3Helper.TransformCoordinate(ref z1LS, ref inverseLightSpace, out z1WS);

            // 表示カメラ空間。
            Vector3 z0ES;
            Vector3 z1ES;
            Vector3Helper.TransformCoordinate(ref z0WS, ref eyeView, out z0ES);
            Vector3Helper.TransformCoordinate(ref z1WS, ref eyeView, out z1ES);

            var z0 = z0ES.Z;
            var z1 = z1ES.Z;

            float d = Math.Abs(bodyBBoxLS.Max.Z - bodyBBoxLS.Min.Z);

            return d / ((float) Math.Sqrt(z1 / z0) - 1.0f);
        }

        float CalculateOldOptimalN()
        {
            var n = eyeProjectionNear;
            var f = eyeProjectionFar;
            var d = Math.Abs(f - n);

            float dot;
            Vector3.Dot(ref eyeDirection, ref lightDirection, out dot);
            float sinGamma = (float) Math.Sin(Math.Abs(Math.Acos(dot)));

            return (n + (float) Math.Sqrt(n * (n + d * sinGamma))) / sinGamma;
        }

        void CalculateZ0LS(ref Matrix lightSpace, ref Vector3 cameraWS, ref BoundingBox bodyBBoxLS, out Vector3 result)
        {
            var plane = PlaneHelper.CreatePlane(eyeDirection, cameraWS);
            Plane.Transform(ref plane, ref lightSpace, out plane);

            Vector3 cameraLS;
            Vector3Helper.TransformCoordinate(ref cameraWS, ref lightSpace, out cameraLS);

            // オリジナルのままでは、ライトのある方向へカメラを向けた場合に、
            // 正常に描画されなくなる。
            // ここでは Ogre3d の LiSPSMShadowCameraSetup に倣い、
            // 平面との交差を異なる符号に対しても実施。
            //
            // 以下、オリジナル コードの場合 (オリジナルの Plane は D の符号が逆である点に注意)。
            //result.X = cameraLS.X;
            //result.Y = -plane.D - (plane.Normal.Z * bodyBBoxLS.Max.Z - plane.Normal.X * cameraLS.X) / plane.Normal.Y;
            //result.Z = bodyBBoxLS.Max.Z;

            var ray = new Ray(new Vector3(cameraLS.X, 0.0f, bodyBBoxLS.Max.Z), Vector3.UnitY);
            var intersect = ray.Intersects(plane);

            if (intersect != null)
            {
                RayHelper.GetPoint(ref ray, intersect.Value, out result);
            }
            else
            {
                ray.Direction = Vector3Helper.NegativeUnitY;
                intersect = ray.Intersects(plane);

                if (intersect != null)
                {
                    RayHelper.GetPoint(ref ray, intersect.Value, out result);
                }
                else
                {
                    result = Vector3.Zero;
                }
            }
        }

        void CreatePerspective(float left, float right, float bottom, float top, float near, float far, out Matrix result)
        {
            // 即ち、glFrustum。XNA CreatePerspectiveOffCenter に相当。
            // http://msdn.microsoft.com/ja-jp/library/windows/desktop/dd373537(v=vs.85).aspx
            // z 軸の範囲が OpenGL では (-1, 1)、XNA/DirectX では (-1, 0)。

            result = new Matrix();

            result.M11 = 2.0f * near / (right - left);
            result.M22 = 2.0f * near / (top - bottom);
            result.M31 = (right + left) / (right - left);
            result.M32 = (top + bottom) / (top - bottom);
            result.M33 = -(far + near) / (far - near);
            result.M34 = -1.0f;
            result.M43 = -2.0f * far * near / (far - near);
        }
    }
}
