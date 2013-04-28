#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace LiSPSMDemo
{
    public sealed class GaussianBlur : IDisposable
    {
        Effect effect;

        GaussianBlurEffect gaussianBlurEffect;

        RenderTarget2D backingRenderTarget;

        SpriteBatch spriteBatch;

        public GraphicsDevice GraphicsDevice { get; private set; }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public SurfaceFormat Format { get; private set; }

        public int Radius
        {
            get { return gaussianBlurEffect.Radius; }
            set { gaussianBlurEffect.Radius = value; }
        }

        public float Amount
        {
            get { return gaussianBlurEffect.Amount; }
            set { gaussianBlurEffect.Amount = value; }
        }

        public GaussianBlur(GraphicsDevice graphicsDevice, int width, int height, SurfaceFormat format, Effect effect)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");
            if (width < 1) throw new ArgumentOutOfRangeException("width");
            if (height < 1) throw new ArgumentOutOfRangeException("height");
            if (height < 1) throw new ArgumentOutOfRangeException("height");

            GraphicsDevice = graphicsDevice;
            Width = width;
            Height = height;
            this.effect = effect;

            gaussianBlurEffect = new GaussianBlurEffect(effect);
            gaussianBlurEffect.Width = width;
            gaussianBlurEffect.Height = height;

            spriteBatch = new SpriteBatch(graphicsDevice);

            backingRenderTarget = new RenderTarget2D(graphicsDevice, width, height, false, format, DepthFormat.None);
        }

        public void Filter(Texture2D source, RenderTarget2D destination)
        {
            var previousBlendState = GraphicsDevice.BlendState;
            var previousDepthStencilState = GraphicsDevice.DepthStencilState;
            var previousRasterizerState = GraphicsDevice.RasterizerState;
            var previousSamplerState = GraphicsDevice.SamplerStates[0];

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

            Filter(source, backingRenderTarget, GaussianBlurEffectPass.Horizon);
            Filter(backingRenderTarget, destination, GaussianBlurEffectPass.Vertical);

            GraphicsDevice.SetRenderTarget(null);

            // ステートを以前の状態へ戻す。
            GraphicsDevice.BlendState = previousBlendState;
            GraphicsDevice.DepthStencilState = previousDepthStencilState;
            GraphicsDevice.RasterizerState = previousRasterizerState;
            GraphicsDevice.SamplerStates[0] = previousSamplerState;
        }

        void Filter(Texture2D source, RenderTarget2D destination, GaussianBlurEffectPass direction)
        {
            GraphicsDevice.SetRenderTarget(destination);

            gaussianBlurEffect.Pass = direction;
            gaussianBlurEffect.Apply();

            GraphicsDevice.Textures[0] = source;

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, null, null, effect);
            spriteBatch.Draw(source, destination.Bounds, Color.White);
            spriteBatch.End();
        }

        #region IDisposable

        bool disposed;

        ~GaussianBlur()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                backingRenderTarget.Dispose();
            }

            disposed = true;
        }

        #endregion
    }
}
