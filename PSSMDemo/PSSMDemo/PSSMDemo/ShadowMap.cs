#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace PSSMDemo
{
    /// <summary>
    /// シャドウ マップを描画するクラスです。
    /// </summary>
    /// <remarks>
    /// このクラスはシャドウ マップの描画に専念するため、
    /// VSM 形式でシャドウ マップを生成する場合、
    /// 別途、このクラスで描画したシャドウ マップに対してブラーを適用する必要があります。
    /// </remarks>
    public sealed class ShadowMap : IDisposable
    {
        /// <summary>
        /// 投影オブジェクトを描画する際に呼び出されるコールバック デリゲートです。
        /// コールバックを受けたクラスは、シャドウ マップ エフェクトを用いて投影オブジェクトを描画します。
        /// 描画する投影オブジェクトの選択は、コールバックを受けたクラスが決定します。
        /// </summary>
        /// <param name="eyeView">現在の表示カメラのビュー行列。</param>
        /// <param name="eyeProjection">現在の表示カメラの射影行列。</param>
        /// <param name="effect">シャドウ マップ エフェクト。</param>
        public delegate void DrawShadowCastersCallback(Matrix eyeView, Matrix eyeProjection, ShadowMapEffect effect);

        /// <summary>
        /// シャドウ マップ エフェクト。
        /// </summary>
        ShadowMapEffect shadowMapEffect;

        /// <summary>
        /// シャドウ マップのサイズ。
        /// </summary>
        int size;

        /// <summary>
        /// デバイスを取得します。
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// シャドウ マップ形式を取得または設定します。
        /// </summary>
        public ShadowMapForm Form
        {
            get { return shadowMapEffect.Form; }
            set
            {
                if (shadowMapEffect.Form == value) return;

                var previous = shadowMapEffect.Form;

                shadowMapEffect.Form = value;

                // VSM に関する切り替えならばレンダ ターゲットの再作成が必要。
                if (previous == ShadowMapForm.Variance ||
                    shadowMapEffect.Form == ShadowMapForm.Variance)
                {
                    InvalidateRenderTarget();
                }
            }
        }

        /// <summary>
        /// シャドウ マップのサイズを取得または設定します。
        /// </summary>
        public int Size
        {
            get { return size; }
            set
            {
                if (size == value) return;

                size = value;

                InvalidateRenderTarget();
            }
        }

        /// <summary>
        /// シャドウ マップが描画されるレンダ ターゲットを取得します。
        /// </summary>
        /// <remarks>
        /// レンダ ターゲットは、初回の Draw メソッドが呼び出されるまで生成されません。
        /// また、シャドウ マップ形式を変更した場合、
        /// Draw メソッドでレンダ ターゲットが再生成される可能性があります。
        /// </remarks>
        public RenderTarget2D RenderTarget { get; private set; }

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        /// <param name="device">デバイス。</param>
        /// <param name="shadowMapEffect">シャドウ マップ エフェクト。</param>
        public ShadowMap(GraphicsDevice device, ShadowMapEffect shadowMapEffect)
        {
            if (device == null) throw new ArgumentNullException("device");

            GraphicsDevice = device;
            this.shadowMapEffect = shadowMapEffect;
        }

        /// <summary>
        /// シャドウ マップを描画します。
        /// </summary>
        /// <param name="eyeView">表示カメラのビュー行列。</param>
        /// <param name="eyeProjection">表示カメラの射影行列。</param>
        /// <param name="lightView">ライト カメラのビュー行列。</param>
        /// <param name="lightProjection">ライト カメラの射影行列。</param>
        /// <param name="drawShadowCasters">投影オブジェクト描画コールバック。</param>
        public void Draw(
            Matrix eyeView, Matrix eyeProjection,
            Matrix lightView, Matrix lightProjection,
            DrawShadowCastersCallback drawShadowCasters)
        {
            if (drawShadowCasters == null) throw new ArgumentNullException("drawShadowCasters");

            PrepareRenderTarget();

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;

            // エフェクトを設定。
            shadowMapEffect.View = lightView;
            shadowMapEffect.Projection = lightProjection;

            GraphicsDevice.SetRenderTarget(RenderTarget);
            GraphicsDevice.Clear(Color.White);

            // 描画をコールバック。
            // 描画する投影オブジェクトの選別は、コールバックされる側のクラスで決定。
            drawShadowCasters(eyeView, eyeProjection, shadowMapEffect);

            GraphicsDevice.SetRenderTarget(null);
        }

        void PrepareRenderTarget()
        {
            if (RenderTarget == null)
            {
                var format = SurfaceFormat.Single;
                if (shadowMapEffect.Form == ShadowMapForm.Variance)
                    format = SurfaceFormat.Vector2;

                RenderTarget = new RenderTarget2D(
                    GraphicsDevice,
                    size, size, false, format,
                    DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.PreserveContents);
            }
        }

        void InvalidateRenderTarget()
        {
            if (RenderTarget != null)
            {
                RenderTarget.Dispose();
                RenderTarget = null;
            }
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed;

        ~ShadowMap()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                if (RenderTarget != null)
                    RenderTarget.Dispose();
            }

            disposed = true;
        }

        #endregion
    }
}
