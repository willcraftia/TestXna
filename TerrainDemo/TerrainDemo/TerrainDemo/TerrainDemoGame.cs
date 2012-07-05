#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

#endregion

namespace TerrainDemo
{
    public class TerrainDemoGame : Game
    {
        const int heightMapSize = 256 + 1;

        const int heightMapCountX = 1;

        const int heightMapCountY = 1;

        GraphicsDeviceManager graphics;

        KeyboardState lastKeyboardState;

        ImprovedPerlinNoise improvedPerlinNoise = new ImprovedPerlinNoise();

        SumFractal sumFractal = new SumFractal();

        HeightMap[,] heightMaps = new HeightMap[heightMapCountX, heightMapCountY];

        Terrain terrain;

        BasicEffect basicEffect;

        FreeView view = new FreeView();

        PerspectiveFov projection = new PerspectiveFov();

        FreeViewInput freeViewInput = new FreeViewInput();

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
                    var w = 1;
                    var h = 1;
                    map.SetBounds(w * i, h * j, w, h);
                    map.Build();

                    heightMaps[i, j] = map;
                }
            }

            terrain = new Terrain(GraphicsDevice);
            terrain.HeightColors.AddColor(-1.0000f, new Color(0, 0, 128));
            terrain.HeightColors.AddColor(-0.2500f, new Color(0, 0, 255));
            terrain.HeightColors.AddColor(0.0000f, new Color(0, 128, 255));
            terrain.HeightColors.AddColor(0.0625f, new Color(240, 240, 64));
            terrain.HeightColors.AddColor(0.1250f, new Color(32, 160, 0));
            terrain.HeightColors.AddColor(0.3750f, new Color(224, 224, 0));
            //terrain.HeightColors.AddColor(0.7500f, new Color(128, 128, 128));
            terrain.HeightColors.AddColor(0.7500f, new Color(64, 64, 64));
            terrain.HeightColors.AddColor(1.0000f, new Color(255, 255, 255));
            terrain.HeightMap = heightMaps[0, 0];
            terrain.Build();

            // カメラ設定。
            //var view = Matrix.CreateLookAt(new Vector3(0.0f, 0.0f, 100.0f), Vector3.Zero, Vector3.Up);
            //var projection = Matrix.CreatePerspectiveFieldOfView(
            //    MathHelper.ToRadians(45.0f),
            //    (float) GraphicsDevice.Viewport.Width / (float) GraphicsDevice.Viewport.Height,
            //    1.0f,
            //    1000.0f);

            // エフェクト設定。
            basicEffect = new BasicEffect(GraphicsDevice);
            basicEffect.VertexColorEnabled = true;
            //basicEffect.TextureEnabled = true;
            //basicEffect.View = view;
            //basicEffect.Projection = projection;

            quadTree = new QuadTree(GraphicsDevice, Vector3.Zero, heightMaps[0, 0], view.Matrix, projection.Matrix, 1);
            quadTree.Effect.Texture = Content.Load<Texture2D>("jigsaw");
        }

        protected override void UnloadContent()
        {
            terrain.Dispose();
            basicEffect.Dispose();
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
            //quadTree.CameraPosition = view.Position;
            quadTree.Update(gameTime);

            Window.Title = string.Format("Triangles Rendered: {0} - Culling Enabled: {1}", quadTree.IndexCount / 3, quadTree.Cull);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            //GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.Clear(Color.Black);

            quadTree.Draw(gameTime);

            //basicEffect.View = view.Matrix;
            //basicEffect.Projection = projection.Matrix;

            //DrawTerrain();

            base.Draw(gameTime);
        }

        void DrawTerrain()
        {
            if (terrain.VertexBuffer == null || terrain.IndexBuffer == null) return;

            //GraphicsDevice.RasterizerState = new RasterizerState
            //{
            //    CullMode = CullMode.None,
            //    FillMode = FillMode.WireFrame
            //};
            GraphicsDevice.SetVertexBuffer(terrain.VertexBuffer);
            GraphicsDevice.Indices = terrain.IndexBuffer;

            int primitiveCount = terrain.IndexBuffer.IndexCount / 3;

            foreach (var pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                GraphicsDevice.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList, 0, 0, terrain.VertexBuffer.VertexCount, 0, primitiveCount);
            }

            GraphicsDevice.Indices = null;
        }
    }
}
