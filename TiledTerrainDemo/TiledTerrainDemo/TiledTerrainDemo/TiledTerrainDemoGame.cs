#region Using

using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TiledTerrainDemo.Framework;
using TiledTerrainDemo.Framework.Cameras;
using TiledTerrainDemo.Framework.Debug;
using TiledTerrainDemo.Framework.Graphics;
using TiledTerrainDemo.Cameras;
using TiledTerrainDemo.CDLOD;
using TiledTerrainDemo.DemoLandscape;
using TiledTerrainDemo.Landscape;

#endregion

namespace TiledTerrainDemo
{
    public class TiledTerrainDemoGame : Game
    {
        //int heightMapWidth = 256 * 4 + 1;
        //int heightMapHeight = 256 * 4 + 1;
        int heightMapWidth = 256 * 1 + 1;
        int heightMapHeight = 256 * 1 + 1;
        int heightMapOverlapSize = 1;
        float noiseSampleX = 0;
        float noiseSampleY = 0;
        //float noiseSampleWidth = 12;
        //float noiseSampleHeight = 12;
        float noiseSampleWidth = 2;
        float noiseSampleHeight = 2;

        // CDLOD settings for debug.
        int levelCount = Settings.DefaultLevelCount;
        //int leafNodeSize = Settings.DefaultLeafNodeSize;
        int leafNodeSize = Settings.DefaultLeafNodeSize * 2;
        int patchResolution = Settings.DefaultPatchResolution;
        //int patchResolution = Settings.DefaultPatchResolution * 2;
        //float patchScale = Settings.DefaultPatchScale;
        float patchScale = 16 * 2;
        //float heightScale = Settings.DefaultHeightScale;
        float heightScale = Settings.DefaultHeightScale * 4;
        //float heightScale = Settings.DefaultHeightScale * 16;

        int finestNodeSize = DefaultVisibleRanges.DefaultFinestNodeSize;
        float detailBalance = DefaultVisibleRanges.DefaultDetailBalance;
        int loadThreadCount = PartitionLoadQueue.DefaultThreadCount;
        int initialPartitionPoolCapacity = 20;
        int maxPartitionPoolCapacity = 100;

        // View settings for debug.
        //float farPlaneDistance = 150000;
        //float farPlaneDistance = 10000;
        float farPlaneDistance = 300000;
        float fogStart = 20000;
        float fogEnd = 30000;
        //Vector3 fogColor = Vector3.One;
        Vector3 fogColor = Color.CornflowerBlue.ToVector3();
        float moveVelocity = 1000;
        float dashFactor = 2;
        Vector4 backgroundColor = Color.CornflowerBlue.ToVector4();

        GraphicsDeviceManager graphics;

        KeyboardState lastKeyboardState;

        FreeView view = new FreeView();

        PerspectiveFov projection = new PerspectiveFov();

        FreeViewInput viewInput = new FreeViewInput();

        RasterizerState defaultRasterizerState = new RasterizerState();

        PartitionManager partitionManager;

        Settings settings = Settings.Default;

        DemoPartitionContext partitionContext;

        DemoPartitionFactory partitionFactory;

        #region Debug

        string helpMessage =
            "[F1] Help\r\n" +
            "[F2] Node bounding box\r\n" +
            "[F3] White solid\r\n" +
            "[F4] Height color\r\n" +
            "[F5] Wireframe\r\n" +
            "[F6] Light\r\n" +
            "[F7] Fog\r\n" +
            "[w][s][a][d][q][z] Movement\r\n" +
            "[Mouse] Camera orientation\r\n" +
            "[PageUp][PageDown] Move velocity";

        Vector2 helpMessageFontSize;

        Vector2 informationTextFontSize;

        bool helpVisible = true;

        StringBuilder stringBuilder = new StringBuilder();

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

        #endregion

        public TiledTerrainDemoGame()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            // for test settings
            //graphics.PreferredBackBufferWidth = 640;
            //graphics.PreferredBackBufferHeight = 480;
            graphics.PreferMultiSampling = true;

            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            var viewport = GraphicsDevice.Viewport;
            viewInput.InitialMousePositionX = viewport.Width / 2;
            viewInput.InitialMousePositionY = viewport.Height / 2;
            viewInput.FreeView = view;
            viewInput.MoveVelocity = moveVelocity;
            viewInput.DashFactor = dashFactor;

            //view.Position = new Vector3(50, 30, 50);
            view.Position = new Vector3(-150000.0f, 30, -150000.0f);
            view.Yaw(MathHelper.PiOver4 * 5);
            view.Update();

            projection.FarPlaneDistance = farPlaneDistance;
            projection.Update();

            defaultRasterizerState.CullMode = CullMode.CullCounterClockwiseFace;
            defaultRasterizerState.FillMode = FillMode.Solid;

            var fpsCounter = new FpsCounter(this);
            fpsCounter.Content.RootDirectory = "Content";
            fpsCounter.HorizontalAlignment = DebugHorizontalAlignment.Right;
            fpsCounter.SampleSpan = TimeSpan.FromSeconds(2);
            fpsCounter.Enabled = false;
            fpsCounter.Visible = false;
            Components.Add(fpsCounter);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            settings.LevelCount = levelCount;
            settings.LeafNodeSize = leafNodeSize;
            settings.PatchResolution = patchResolution;
            settings.PatchScale = patchScale;
            settings.HeightScale = heightScale;
            settings.HeightMapWidth = heightMapWidth;
            settings.HeightMapHeight = heightMapHeight;
            settings.HeightMapOverlapSize = heightMapOverlapSize;

            var visibleRanges = new DefaultVisibleRanges(settings);
            visibleRanges.FinestNodeSize = finestNodeSize;
            visibleRanges.DetailBalance = detailBalance;
            visibleRanges.Initialize();

            partitionContext = new DemoPartitionContext(
                GraphicsDevice, Content, settings, visibleRanges,
                noiseSampleX, noiseSampleY, noiseSampleWidth, noiseSampleHeight);
            partitionContext.TerrainRenderer.FogEnabled = true;
            partitionContext.TerrainRenderer.FogStart = fogStart;
            partitionContext.TerrainRenderer.FogEnd = fogEnd;
            partitionContext.TerrainRenderer.FogColor = fogColor;

            partitionFactory = new DemoPartitionFactory(partitionContext);

            var terrainScale = settings.TerrainScale;

            partitionManager = new PartitionManager(partitionFactory.Create, loadThreadCount,
                initialPartitionPoolCapacity, maxPartitionPoolCapacity);
            partitionManager.PartitionWidth = terrainScale.X;
            partitionManager.PartitionHeight = terrainScale.Z;
            partitionManager.ActivationRange = terrainScale.X * 3.0f;
            partitionManager.DeactivationRange = partitionManager.ActivationRange * 1.5f;
            partitionManager.EyePosition = view.Position;

            #region Debug

            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("Fonts/Debug");
            fillTexture = Texture2DHelper.CreateFillTexture(GraphicsDevice);
            helpMessageFontSize = font.MeasureString(helpMessage);

            BuildInformationMessage();
            informationTextFontSize = font.MeasureString(stringBuilder);

            #endregion
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (!IsActive) return;

            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Escape)) Exit();

            viewInput.Update(gameTime);

            if (keyboardState.IsKeyDown(Keys.PageUp))
                viewInput.MoveVelocity += 10;
            if (keyboardState.IsKeyDown(Keys.PageDown))
            {
                viewInput.MoveVelocity -= 10;
                if (viewInput.MoveVelocity < 10) viewInput.MoveVelocity = 10;
            }

            #region Debug

            if (keyboardState.IsKeyUp(Keys.F1) && lastKeyboardState.IsKeyDown(Keys.F1))
                helpVisible = !helpVisible;

            if (keyboardState.IsKeyUp(Keys.F2) && lastKeyboardState.IsKeyDown(Keys.F2))
                partitionContext.TerrainRenderer.NodeBoundingBoxVisible = !partitionContext.TerrainRenderer.NodeBoundingBoxVisible;

            if (keyboardState.IsKeyUp(Keys.F3) && lastKeyboardState.IsKeyDown(Keys.F3))
                partitionContext.TerrainRenderer.WhiteSolidVisible = !partitionContext.TerrainRenderer.WhiteSolidVisible;

            if (keyboardState.IsKeyUp(Keys.F4) && lastKeyboardState.IsKeyDown(Keys.F4))
                partitionContext.TerrainRenderer.HeightColorVisible = !partitionContext.TerrainRenderer.HeightColorVisible;

            if (keyboardState.IsKeyUp(Keys.F5) && lastKeyboardState.IsKeyDown(Keys.F5))
                partitionContext.TerrainRenderer.WireframeVisible = !partitionContext.TerrainRenderer.WireframeVisible;

            if (keyboardState.IsKeyUp(Keys.F6) && lastKeyboardState.IsKeyDown(Keys.F6))
                partitionContext.TerrainRenderer.LightEnabled = !partitionContext.TerrainRenderer.LightEnabled;

            if (keyboardState.IsKeyUp(Keys.F7) && lastKeyboardState.IsKeyDown(Keys.F7))
                partitionContext.TerrainRenderer.FogEnabled = !partitionContext.TerrainRenderer.FogEnabled;

            #endregion

            lastKeyboardState = keyboardState;

            view.Update();
            projection.Update();

            partitionManager.EyePosition = view.Position;
            partitionManager.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, backgroundColor, 1, 0);
            GraphicsDevice.RasterizerState = defaultRasterizerState;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            partitionContext.Selection.View = view.Matrix;
            partitionContext.Selection.Projection = projection.Matrix;
            partitionContext.Selection.Prepare();

            #region Debug

            partitionContext.TotalSelectedNodeCount = 0;
            partitionContext.DrawPartitionCount = 0;

            #endregion

            partitionManager.Draw(gameTime);

            #region Debug

            if (helpVisible) DrawHelp();

            #endregion

            base.Draw(gameTime);
        }

        void BuildInformationMessage()
        {
            var sb = stringBuilder;

            sb.Length = 0;
            sb.Append("Screen: ");
            sb.AppendNumber(graphics.PreferredBackBufferWidth).Append('x');
            sb.AppendNumber(graphics.PreferredBackBufferHeight).AppendLine();
            sb.Append("Height map: ");
            sb.AppendNumber(heightMapWidth).Append('x').Append(heightMapHeight).Append(" ");
            sb.Append("(overlap ");
            sb.AppendNumber(settings.HeightMapOverlapSize).Append(")").AppendLine();
            sb.Append("Level count: ");
            sb.AppendNumber(settings.LevelCount).Append(", ");
            sb.Append("Leaf node size: ");
            sb.AppendNumber(settings.LeafNodeSize).AppendLine();
            sb.Append("Patch resolution: ");
            sb.AppendNumber(settings.PatchResolution).Append(", ");
            sb.Append("Patch Scale: ");
            sb.AppendNumber(settings.PatchScale).AppendLine();
            sb.Append("Top node size: ");
            sb.AppendNumber(settings.TopNodeSize).AppendLine();
            sb.Append("Far plane distance: ");
            sb.AppendNumber(farPlaneDistance).AppendLine();
            sb.AppendLine();
            sb.Append("Quads: ");
            sb.AppendNumber(partitionContext.TotalSelectedNodeCount).AppendLine();
            sb.Append("Partitions: ");
            sb.AppendNumber(partitionContext.DrawPartitionCount).Append("/");
            sb.AppendNumber(partitionManager.ActivePartitionCount).Append(" ");
            sb.Append("W(");
            sb.AppendNumber(partitionManager.WaitLoadPartitionCount).Append(") ");
            sb.Append("L(");
            sb.AppendNumber(partitionManager.LoadingParitionCount).Append(") ");
            sb.Append("N(");
            sb.AppendNumber(partitionManager.NonePartitionCount).Append(")").AppendLine();
            sb.Append("Thread pool: ");
            sb.AppendNumber(partitionManager.FreePartitionLoadingThreadCount).Append("/");
            sb.AppendNumber(partitionManager.PartitionLoadingThreadCount).AppendLine();
            sb.Append("Partition pool: ");
            sb.AppendNumber(partitionManager.FreePartitionObjectCount).Append("/");
            sb.AppendNumber(partitionManager.TotalPartitionObjectCount).Append("(");
            sb.AppendNumber(partitionManager.MaxPartitionObjectCount).Append(")").AppendLine();
            sb.Append("Move velocity: ");
            sb.AppendNumber(viewInput.MoveVelocity).AppendLine();
            sb.Append("Eye position: ");
            sb.AppendNumber(view.Position.X).Append(", ");
            sb.AppendNumber(view.Position.Y).Append(", ");
            sb.AppendNumber(view.Position.Z);
        }

        void DrawHelp()
        {
            spriteBatch.Begin();

            var layout = new DebugLayout();

            int informationWidth = 380;

            // calculate the background area for information.
            layout.ContainerBounds = GraphicsDevice.Viewport.TitleSafeArea;
            layout.Width = informationWidth + 4;
            layout.Height = (int) informationTextFontSize.Y + 2;
            layout.HorizontalMargin = 8;
            layout.VerticalMargin = 8;
            layout.HorizontalAlignment = DebugHorizontalAlignment.Left;
            layout.VerticalAlignment = DebugVerticalAlignment.Top;
            layout.Arrange();
            // draw the rectangle.
            spriteBatch.Draw(fillTexture, layout.ArrangedBounds, Color.Black * 0.5f);

            // calculate the text area for help messages.
            layout.ContainerBounds = layout.ArrangedBounds;
            layout.Width = informationWidth;
            layout.Height = (int) informationTextFontSize.Y;
            layout.HorizontalMargin = 2;
            layout.VerticalMargin = 0;
            layout.HorizontalAlignment = DebugHorizontalAlignment.Center;
            layout.VerticalAlignment = DebugVerticalAlignment.Center;
            layout.Arrange();
            // draw the text.
            BuildInformationMessage();
            spriteBatch.DrawString(font, stringBuilder, new Vector2(layout.ArrangedBounds.X, layout.ArrangedBounds.Y), Color.Yellow);

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
