#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TerrainDemo.Framework.Cameras;
using TerrainDemo.Framework.Debug;
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

        bool isWireframe;

        RasterizerState defaultRasterizerState = new RasterizerState();

        RasterizerState wireframeRasterizerState = new RasterizerState();

        CDLODTerrain terrain;

        public TerrainDemoGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            IsMouseVisible = true;
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
            projection.FarPlaneDistance = 10000;

            defaultRasterizerState.CullMode = CullMode.CullCounterClockwiseFace;
            defaultRasterizerState.FillMode = FillMode.Solid;
            wireframeRasterizerState.CullMode = CullMode.CullCounterClockwiseFace;
            wireframeRasterizerState.FillMode = FillMode.WireFrame;

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
            // ƒmƒCƒY‚©‚ç height map ‚ð¶¬B
            var noiseMap = new NoiseMap();
            noiseMap.GetValue2 = (x, y) => { return sumFractal.GetValue(x, 0, y); };
            noiseMap.Width = noiseMapWidth;
            noiseMap.Height = noiseMapHeight;
            noiseMap.SetBounds(0, 0, noiseSampleWidth, noiseSampleHeight);
            noiseMap.Build();

            var heightMap = new DefaultHeightMapSource(noiseMap);

            terrain = new CDLODTerrain(GraphicsDevice, Content);
            terrain.Initialize(heightMap);
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
            {
                //GraphicsDevice.RasterizerState = isWireframe ? defaultRasterizerState : wireframeRasterizerState;
                isWireframe = !isWireframe;
            }

            //if (keyboardState.IsKeyUp(Keys.F2) && lastKeyboardState.IsKeyDown(Keys.F2))
            //    quadTree.Cull = !quadTree.Cull;

            lastKeyboardState = keyboardState;

            view.Update();
            projection.Update();

            terrain.View = view.Matrix;
            terrain.Projection = projection.Matrix;
            terrain.Update(gameTime);

            //quadTree.View = view.Matrix;
            //quadTree.Projection = projection.Matrix;
            //quadTree.Update(gameTime);

            Window.Title = string.Format("Selected nodes: {0}", terrain.SelectedNodeCount);
            //Window.Title = string.Format("Triangles Rendered: {0} - Culling Enabled: {1}", quadTree.IndexCount / 3, quadTree.Cull);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            //GraphicsDevice.Clear(Color.Black);

            GraphicsDevice.RasterizerState = isWireframe ? wireframeRasterizerState : defaultRasterizerState;

            //quadTree.Draw(gameTime);
            terrain.Draw(gameTime);

            base.Draw(gameTime);
        }
    }
}
