#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TerrainDemo.Framework.Cameras;
using TerrainDemo.Cameras;
using TerrainDemo.LOD;
using TerrainDemo.Noise;

#endregion

namespace TerrainDemo
{
    public class TerrainDemoGame : Game
    {
        const int heightMapSize = 1024 + 1;

        const int heightMapCountX = 1;

        const int heightMapCountY = 1;

        const float noiseSampleWidth = 10;
        
        const float noiseSampleHeight = 10;

        GraphicsDeviceManager graphics;

        KeyboardState lastKeyboardState;

        ImprovedPerlinNoise improvedPerlinNoise = new ImprovedPerlinNoise();

        SumFractal sumFractal = new SumFractal();

        HeightMap[,] heightMaps = new HeightMap[heightMapCountX, heightMapCountY];

        FreeView view = new FreeView();

        PerspectiveFov projection = new PerspectiveFov();

        FreeViewInput freeViewInput = new FreeViewInput();

        Vector3 terrainScale = new Vector3(1, 50, 1);

        QuadTree quadTree;

        bool isWireframe;

        RasterizerState defaultRasterizerState = new RasterizerState();

        RasterizerState wireframeRasterizerState = new RasterizerState();

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

            defaultRasterizerState.CullMode = CullMode.CullCounterClockwiseFace;
            defaultRasterizerState.FillMode = FillMode.Solid;
            wireframeRasterizerState.CullMode = CullMode.CullCounterClockwiseFace;
            wireframeRasterizerState.FillMode = FillMode.WireFrame;

            IsFixedTimeStep = false;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // ノイズから height map を生成。
            for (int i = 0; i < heightMapCountX; i++)
            {
                for (int j = 0; j < heightMapCountY; j++)
                {
                    var map = new HeightMap();
                    map.GetValue2 = (x, y) => { return sumFractal.GetValue(x, 0, y); };
                    map.Size = heightMapSize;
                    map.SetBounds(noiseSampleWidth * i, noiseSampleHeight * j, noiseSampleWidth, noiseSampleHeight);
                    map.Build();

                    heightMaps[i, j] = map;
                }
            }

            quadTree = new QuadTree(GraphicsDevice, Vector3.Zero, heightMaps[0, 0], view.Matrix, projection.Matrix, terrainScale);
            quadTree.Effect.Texture = Content.Load<Texture2D>("jigsaw");
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
                GraphicsDevice.RasterizerState = isWireframe ? defaultRasterizerState : wireframeRasterizerState;
                isWireframe = !isWireframe;
            }

            if (keyboardState.IsKeyUp(Keys.F2) && lastKeyboardState.IsKeyDown(Keys.F2))
                quadTree.Cull = !quadTree.Cull;

            lastKeyboardState = keyboardState;

            view.Update();
            projection.Update();

            quadTree.View = view.Matrix;
            quadTree.Projection = projection.Matrix;
            quadTree.Update(gameTime);

            Window.Title = string.Format("Triangles Rendered: {0} - Culling Enabled: {1}", quadTree.IndexCount / 3, quadTree.Cull);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            //GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.Clear(Color.Black);

            quadTree.Draw(gameTime);

            base.Draw(gameTime);
        }
    }
}
