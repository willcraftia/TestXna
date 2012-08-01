#region Using

using System;
using System.Diagnostics;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Diagnostics
{
    /// <summary>
    /// FPS を計測して画面に表示するクラスです。
    /// </summary>
    public sealed class FpsCounter : DrawableGameComponent
    {
        /// <summary>
        /// Stopwatch。
        /// </summary>
        Stopwatch stopwatch = Stopwatch.StartNew();

        /// <summary>
        /// 現在の FPS。
        /// </summary>
        float fps;

        /// <summary>
        /// サンプリングしたフレーム数。
        /// </summary>
        int sampleFrames;

        /// <summary>
        /// SpriteFont。
        /// </summary>
        SpriteFont font;

        /// <summary>
        /// フォント サイズ。
        /// </summary>
        Vector2 fontSize;

        /// <summary>
        /// 塗り潰し用テクスチャ。
        /// </summary>
        Texture2D fillTexture;

        /// <summary>
        /// SpriteBatch。
        /// </summary>
        SpriteBatch spriteBatch;

        /// <summary>
        /// 表示文字列を格納する StringBuilder。
        /// </summary>
        StringBuilder stringBuilder = new StringBuilder("FPS: 00.00000".Length);

        /// <summary>
        /// 背景色を取得または設定します。
        /// </summary>
        public Color BackgroundColor { get; set; }

        /// <summary>
        /// 前景色を取得または設定します。
        /// </summary>
        public Color ForegroundColor { get; set; }

        /// <summary>
        /// Running Slowly が発生した場合の前景色を取得または設定します。
        /// </summary>
        public Color RunningSlowlyForegroundColor { get; set; }

        /// <summary>
        /// サンプル期間を取得または設定します。
        /// </summary>
        public TimeSpan SampleSpan { get; set; }

        /// <summary>
        /// カウンタの水平方向についての配置方法を取得または設定します。
        /// </summary>
        public DebugHorizontalAlignment HorizontalAlignment { get; set; }

        /// <summary>
        /// カウンタの垂直方向についての配置方法を取得または設定します。
        /// </summary>
        public DebugVerticalAlignment VerticalAlignment { get; set; }

        /// <summary>
        /// SpriteFont をロードするための ContentManager を取得します。
        /// </summary>
        public ContentManager Content { get; private set; }

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        /// <param name="game">Game。</param>
        public FpsCounter(Game game)
            : base(game)
        {
            BackgroundColor = Color.Black * 0.5f;
            ForegroundColor = Color.White;
            RunningSlowlyForegroundColor = Color.Red;
            SampleSpan = TimeSpan.FromSeconds(1);
            HorizontalAlignment = DebugHorizontalAlignment.Left;
            VerticalAlignment = DebugVerticalAlignment.Top;

            Content = new ContentManager(Game.Services);

            // 表示文字列を構築します (初期表示用)。
            stringBuilder.Append("FPS:");
        }

        public override void Update(GameTime gameTime)
        {
            if (SampleSpan < stopwatch.Elapsed)
            {
                // サンプル期間を超えたならば FPS を計算します。
                fps = (float) sampleFrames / (float) stopwatch.Elapsed.TotalSeconds;

                // Stopwatch をリセットして新たに開始します。
                stopwatch.Reset();
                stopwatch.Start();

                // サンプル数をリセットします。
                sampleFrames = 0;

                // 表示文字列を構築します。
                stringBuilder.Length = 0;
                stringBuilder.Append("FPS: ");
                stringBuilder.Append(fps);
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            // Draw 呼び出しをサンプリングします。
            sampleFrames++;

            spriteBatch.Begin();

            // 背景領域を計算します。
            var layout = new DebugLayout();
            layout.ContainerBounds = GraphicsDevice.Viewport.TitleSafeArea;
            layout.Width = (int) fontSize.X + 4;
            layout.Height = (int) fontSize.Y + 2;
            layout.HorizontalMargin = 8;
            layout.VerticalMargin = 8;
            layout.HorizontalAlignment = HorizontalAlignment;
            layout.VerticalAlignment = VerticalAlignment;
            layout.Arrange();

            spriteBatch.Draw(fillTexture, layout.ArrangedBounds, BackgroundColor);

            // 文字列表示領域を計算します。
            layout.ContainerBounds = layout.ArrangedBounds;
            layout.Width = (int) fontSize.X;
            layout.Height = (int) fontSize.Y;
            layout.HorizontalMargin = 2;
            layout.VerticalMargin = 0;
            layout.HorizontalAlignment = DebugHorizontalAlignment.Center;
            layout.VerticalAlignment = DebugVerticalAlignment.Center;
            layout.Arrange();

            // 文字色を決定します。
            var color = !gameTime.IsRunningSlowly ? ForegroundColor : RunningSlowlyForegroundColor;

            spriteBatch.DrawString(font, stringBuilder, new Vector2(layout.ArrangedBounds.X, layout.ArrangedBounds.Y), color);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("Fonts/Debug");
            fontSize = font.MeasureString("FPS: 00.00000");
            fillTexture = Texture2DHelper.CreateFillTexture(GraphicsDevice);

            base.LoadContent();
        }

        protected override void UnloadContent()
        {
            if (spriteBatch != null) spriteBatch.Dispose();
            if (fillTexture != null) fillTexture.Dispose();
            Content.Unload();

            base.UnloadContent();
        }
    }
}
