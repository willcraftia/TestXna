#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TerrainDemo.Framework.Cameras;
using TerrainDemo.Framework.Debug;
using TerrainDemo.Framework.Graphics;
using TerrainDemo.Cameras;
using TerrainDemo.CDLOD;
using TerrainDemo.Noise;

#endregion

namespace TerrainDemo
{
    public class TerrainDemoGame : Game
    {
        const int noiseMapWidth = 512 * 1 + 1;

        const int noiseMapHeight = 512 * 1 + 1;

        const float noiseSampleWidth = 10;
        
        const float noiseSampleHeight = 10;

        GraphicsDeviceManager graphics;

        KeyboardState lastKeyboardState;

        ImprovedPerlinNoise improvedPerlinNoise = new ImprovedPerlinNoise();

        SumFractal sumFractal = new SumFractal();

        FreeView view = new FreeView();

        PerspectiveFov projection = new PerspectiveFov();

        FreeViewInput freeViewInput = new FreeViewInput();

        RasterizerState defaultRasterizerState = new RasterizerState();

        CDLODTerrain terrain;

        string helpMessage =
            "[F1] Help\r\n" +
            "[F2] Node bounding box\r\n" +
            "[F3] White solid\r\n" +
            "[F4] Height color\r\n" +
            "[F5] Wireframe\r\n" +
            "[F6] Light\r\n" +
            "\r\n" +
            "[w][s][a][d][q][z] Movement\r\n" +
            "[Mouse] Camera orientation";

        Vector2 helpMessageFontSize;

        bool helpVisible;

        /// <summary>
        /// SpriteBatch。
        /// </summary>
        SpriteBatch spriteBatch;

        /// <summary>
        /// SpriteFont。
        /// </summary>
        SpriteFont font;

        /// <summary>
        /// 塗り潰し用テクスチャ。
        /// </summary>
        Texture2D fillTexture;

        public TerrainDemoGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            //IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            improvedPerlinNoise.Seed = 300;
            sumFractal.Noise3 = improvedPerlinNoise.Noise;

            var viewport = GraphicsDevice.Viewport;
            freeViewInput.InitialMousePositionX = viewport.Width / 2;
            freeViewInput.InitialMousePositionY = viewport.Height / 2;
            freeViewInput.FreeView = view;

            view.Position = new Vector3(50, 30, 50);
            view.Yaw(MathHelper.PiOver4 * 5);
            projection.FarPlaneDistance = 10000;

            defaultRasterizerState.CullMode = CullMode.CullCounterClockwiseFace;
            defaultRasterizerState.FillMode = FillMode.Solid;

            //IsFixedTimeStep = false;

            var fpsCounter = new FpsCounter(this);
            fpsCounter.Content.RootDirectory = "Content";
            fpsCounter.HorizontalAlignment = DebugHorizontalAlignment.Right;
            fpsCounter.SampleSpan = TimeSpan.FromSeconds(2);
            Components.Add(fpsCounter);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // ノイズから height map を生成。
            var noiseMap = new NoiseMap();
            noiseMap.GetValue2 = (x, y) => { return sumFractal.GetValue(x, 0, y); };
            noiseMap.Width = noiseMapWidth;
            noiseMap.Height = noiseMapHeight;
            noiseMap.SetBounds(0, 0, noiseSampleWidth, noiseSampleHeight);
            noiseMap.Build();

            var heightMap = new DefaultHeightMapSource(noiseMap);

            terrain = new CDLODTerrain(GraphicsDevice, Content);
            terrain.Initialize(heightMap);

            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("Fonts/Debug");
            fillTexture = Texture2DHelper.CreateFillTexture(GraphicsDevice);
            helpMessageFontSize = font.MeasureString(helpMessage);
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Escape)) Exit();

            freeViewInput.Update(gameTime);

            if (keyboardState.IsKeyUp(Keys.F1) && lastKeyboardState.IsKeyDown(Keys.F1))
                helpVisible = !helpVisible;

            if (keyboardState.IsKeyUp(Keys.F2) && lastKeyboardState.IsKeyDown(Keys.F2))
                terrain.NodeBoundingBoxVisible = !terrain.NodeBoundingBoxVisible;

            if (keyboardState.IsKeyUp(Keys.F3) && lastKeyboardState.IsKeyDown(Keys.F3))
                terrain.WhiteSolidVisible = !terrain.WhiteSolidVisible;

            if (keyboardState.IsKeyUp(Keys.F4) && lastKeyboardState.IsKeyDown(Keys.F4))
                terrain.HeightColorVisible = !terrain.HeightColorVisible;

            if (keyboardState.IsKeyUp(Keys.F5) && lastKeyboardState.IsKeyDown(Keys.F5))
                terrain.WireframeVisible = !terrain.WireframeVisible;

            if (keyboardState.IsKeyUp(Keys.F6) && lastKeyboardState.IsKeyDown(Keys.F6))
                terrain.LightEnabled = !terrain.LightEnabled;

            lastKeyboardState = keyboardState;

            view.Update();
            projection.Update();

            terrain.View = view.Matrix;
            terrain.Projection = projection.Matrix;
            terrain.Update(gameTime);

            Window.Title = string.Format("Selected nodes: {0}", terrain.SelectedNodeCount);
            //Window.Title = string.Format("Triangles Rendered: {0} - Culling Enabled: {1}", quadTree.IndexCount / 3, quadTree.Cull);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.RasterizerState = defaultRasterizerState;

            terrain.Draw(gameTime);

            if (helpVisible)
                DrawHelp();

            base.Draw(gameTime);
        }

        void DrawHelp()
        {
            spriteBatch.Begin();

            // 背景領域を計算します。
            var layout = new DebugLayout();
            layout.ContainerBounds = GraphicsDevice.Viewport.TitleSafeArea;
            layout.Width = (int) helpMessageFontSize.X + 4;
            layout.Height = (int) helpMessageFontSize.Y + 2;
            layout.HorizontalMargin = 8;
            layout.VerticalMargin = 8;
            layout.HorizontalAlignment = DebugHorizontalAlignment.Left;
            layout.VerticalAlignment = DebugVerticalAlignment.Top;
            layout.Arrange();

            spriteBatch.Draw(fillTexture, layout.ArrangedBounds, Color.Black * 0.5f);
            
            // 文字列表示領域を計算します。
            layout.ContainerBounds = layout.ArrangedBounds;
            layout.Width = (int) helpMessageFontSize.X;
            layout.Height = (int) helpMessageFontSize.Y;
            layout.HorizontalMargin = 2;
            layout.VerticalMargin = 0;
            layout.HorizontalAlignment = DebugHorizontalAlignment.Center;
            layout.VerticalAlignment = DebugVerticalAlignment.Center;
            layout.Arrange();

            spriteBatch.DrawString(font, helpMessage, new Vector2(layout.ArrangedBounds.X, layout.ArrangedBounds.Y), Color.Yellow);

            spriteBatch.End();
        }
    }
}
