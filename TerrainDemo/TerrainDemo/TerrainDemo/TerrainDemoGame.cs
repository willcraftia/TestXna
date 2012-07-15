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
        // noise parameters for debug.
        int noiseSeed = 300;
        // A map can not be over 4096x4096 in HiDef profile.
        int noiseMapWidth = 256 * 1 + 1;
        int noiseMapHeight = 256 * 1 + 1;
        //int noiseMapWidth = 256 * 8 + 1;
        //int noiseMapHeight = 256 * 8 + 1;
        float noiseSampleX = 0;
        float noiseSampleY = 0;
        float noiseSampleWidth = 12;
        float noiseSampleHeight = 12;
        //float noiseSampleWidth = 6;
        //float noiseSampleHeight = 6;

        // CDLOD settings for debug.
        int levelCount = Settings.DefaultLevelCount;
        //int levelCount = 10;
        int leafNodeSize = Settings.DefaultLeafNodeSize;
        //int leafNodeSize = Settings.DefaultLeafNodeSize * 2 * 2;
        float patchScale = Settings.DefaultPatchScale;
        //float patchScale = Settings.DefaultPatchScale * 2 * 2;
        float heightScale = Settings.DefaultHeightScale * 0.1f;
        //float heightScale = Settings.DefaultHeightScale;
        //float heightScale = Settings.DefaultHeightScale * 3;

        // View settings for debug.
        float farPlaneDistance = 100000;

        GraphicsDeviceManager graphics;

        KeyboardState lastKeyboardState;

        ImprovedPerlinNoise noise = new ImprovedPerlinNoise();

        SumFractal sumFractal = new SumFractal();

        FreeView view = new FreeView();

        PerspectiveFov projection = new PerspectiveFov();

        FreeViewInput viewInput = new FreeViewInput();

        RasterizerState defaultRasterizerState = new RasterizerState();

        Settings settings = Settings.Default;

        DefaultVisibleRanges visibleRanges;

        Terrain terrain;

        TerrainRenderer renderer;

        Selection selection;

        string helpMessage =
            "[F1] Help\r\n" +
            "[F2] Node bounding box\r\n" +
            "[F3] White solid\r\n" +
            "[F4] Height color\r\n" +
            "[F5] Wireframe\r\n" +
            "[F6] Light\r\n" +
            "[w][s][a][d][q][z] Movement\r\n" +
            "[Mouse] Camera orientation";

        Vector2 helpMessageFontSize;

        Vector2 selectedNodesTextFontSize;

        bool helpVisible;

        /// <summary>
        /// SpriteBatch.
        /// </summary>
        SpriteBatch spriteBatch;

        /// <summary>
        /// SpriteFont.
        /// </summary>
        SpriteFont font;

        /// <summary>
        /// The texture to fill a regionÅB
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
            noise.Seed = noiseSeed;
            sumFractal.Noise3 = noise.Noise;

            var viewport = GraphicsDevice.Viewport;
            viewInput.InitialMousePositionX = viewport.Width / 2;
            viewInput.InitialMousePositionY = viewport.Height / 2;
            viewInput.FreeView = view;

            view.Position = new Vector3(50, 30, 50);
            view.Yaw(MathHelper.PiOver4 * 5);
            projection.FarPlaneDistance = farPlaneDistance;

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
            // create a height map.
            var noiseMap = new NoiseMap();
            noiseMap.GetValue2 = (x, y) => { return sumFractal.GetValue(x, 0, y); };
            noiseMap.Width = noiseMapWidth;
            noiseMap.Height = noiseMapHeight;
            noiseMap.SetBounds(noiseSampleX, noiseSampleY, noiseSampleWidth, noiseSampleHeight);
            noiseMap.Build();

            var heightMap = new DefaultHeightMapSource(noiseMap.Values, noiseMap.Width, noiseMap.Height);

            settings.LevelCount = levelCount;
            settings.LeafNodeSize = leafNodeSize;
            settings.PatchScale = patchScale;
            settings.HeightScale = heightScale;

            visibleRanges = new DefaultVisibleRanges(settings);
            //visibleRanges.MostDetailRange = DefaultVisibleRanges.DefaultMostDetailRange;
            visibleRanges.FinestNodeSize = 3;
            visibleRanges.DetailBalance = 2;
            visibleRanges.Initialize();

            terrain = new Terrain(GraphicsDevice, settings);
            terrain.Initialize(heightMap);
            renderer = new TerrainRenderer(GraphicsDevice, Content, settings);
            renderer.InitializeMorphConsts(visibleRanges);
            selection = new Selection(settings, visibleRanges);

            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("Fonts/Debug");
            fillTexture = Texture2DHelper.CreateFillTexture(GraphicsDevice);
            helpMessageFontSize = font.MeasureString(helpMessage);
            selectedNodesTextFontSize = font.MeasureString("Quads: XXXXX");
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Escape)) Exit();

            viewInput.Update(gameTime);

            if (keyboardState.IsKeyUp(Keys.F1) && lastKeyboardState.IsKeyDown(Keys.F1))
                helpVisible = !helpVisible;

            if (keyboardState.IsKeyUp(Keys.F2) && lastKeyboardState.IsKeyDown(Keys.F2))
                renderer.NodeBoundingBoxVisible = !renderer.NodeBoundingBoxVisible;

            if (keyboardState.IsKeyUp(Keys.F3) && lastKeyboardState.IsKeyDown(Keys.F3))
                renderer.WhiteSolidVisible = !renderer.WhiteSolidVisible;

            if (keyboardState.IsKeyUp(Keys.F4) && lastKeyboardState.IsKeyDown(Keys.F4))
                renderer.HeightColorVisible = !renderer.HeightColorVisible;

            if (keyboardState.IsKeyUp(Keys.F5) && lastKeyboardState.IsKeyDown(Keys.F5))
                renderer.WireframeVisible = !renderer.WireframeVisible;

            if (keyboardState.IsKeyUp(Keys.F6) && lastKeyboardState.IsKeyDown(Keys.F6))
                renderer.LightEnabled = !renderer.LightEnabled;

            lastKeyboardState = keyboardState;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            view.Update();
            projection.Update();

            // prepare selection's state.
            selection.View = view.Matrix;
            selection.Projection = projection.Matrix;
            selection.Prepare();

            // select.
            terrain.Select(selection);

            //Window.Title = string.Format("Selected nodes: {0}", selection.SelectedNodeCount);

            GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.RasterizerState = defaultRasterizerState;

            // draw
            renderer.Draw(gameTime, selection);

            if (helpVisible)
                DrawHelp();

            base.Draw(gameTime);
        }

        void DrawHelp()
        {
            spriteBatch.Begin();

            var layout = new DebugLayout();

            var selectedNodesText = string.Format("Quads: {0}", selection.SelectedNodeCount);

            // calculate the background area for information.
            layout.ContainerBounds = GraphicsDevice.Viewport.TitleSafeArea;
            layout.Width = (int) selectedNodesTextFontSize.X + 4;
            layout.Height = (int) selectedNodesTextFontSize.Y + 2;
            layout.HorizontalMargin = 8;
            layout.VerticalMargin = 8;
            layout.HorizontalAlignment = DebugHorizontalAlignment.Left;
            layout.VerticalAlignment = DebugVerticalAlignment.Top;
            layout.Arrange();
            // draw the rectangle.
            spriteBatch.Draw(fillTexture, layout.ArrangedBounds, Color.Black * 0.5f);

            // calculate the text area for help messages.
            layout.ContainerBounds = layout.ArrangedBounds;
            layout.Width = (int) selectedNodesTextFontSize.X;
            layout.Height = (int) selectedNodesTextFontSize.Y;
            layout.HorizontalMargin = 2;
            layout.VerticalMargin = 0;
            layout.HorizontalAlignment = DebugHorizontalAlignment.Center;
            layout.VerticalAlignment = DebugVerticalAlignment.Center;
            layout.Arrange();
            // draw the text.
            spriteBatch.DrawString(font, selectedNodesText, new Vector2(layout.ArrangedBounds.X, layout.ArrangedBounds.Y), Color.Yellow);

            // calculate the background area for help messages.
            layout.ContainerBounds = GraphicsDevice.Viewport.TitleSafeArea;
            layout.Width = (int) helpMessageFontSize.X + 4;
            layout.Height = (int) helpMessageFontSize.Y + 2;
            layout.HorizontalMargin = 8;
            layout.VerticalMargin = 8;
            layout.HorizontalAlignment = DebugHorizontalAlignment.Left;
            layout.VerticalAlignment = DebugVerticalAlignment.Bottom;
            layout.Arrange();
            // draw the rectangle.
            spriteBatch.Draw(fillTexture, layout.ArrangedBounds, Color.Black * 0.5f);

            // calculate the text area for help messages.
            layout.ContainerBounds = layout.ArrangedBounds;
            layout.Width = (int) helpMessageFontSize.X;
            layout.Height = (int) helpMessageFontSize.Y;
            layout.HorizontalMargin = 2;
            layout.VerticalMargin = 0;
            layout.HorizontalAlignment = DebugHorizontalAlignment.Center;
            layout.VerticalAlignment = DebugVerticalAlignment.Center;
            layout.Arrange();
            // draw the text.
            spriteBatch.DrawString(font, helpMessage, new Vector2(layout.ArrangedBounds.X, layout.ArrangedBounds.Y), Color.Yellow);

            spriteBatch.End();
        }
    }
}
