#region Using

using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

#endregion

namespace PerlinNoiseDemo
{
    public class PerlinNoiseDemoGame : Game
    {
        GraphicsDeviceManager graphics;

        PerlinNoise perlinNoise = new PerlinNoise();

        ImprovedPerlinNoise improvedPerlinNoise = new ImprovedPerlinNoise();

        Turbulence turbulence = new Turbulence();

        HeightMap heightMap = new HeightMap();

        HeightMapImage heightMapImage;

        VertexBuffer heightMapMesh;

        BasicEffect basicEffect;

        public PerlinNoiseDemoGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            perlinNoise.Seed = 300;
            improvedPerlinNoise.Seed = 300;

            turbulence.Noise1 = perlinNoise.Noise1;
            turbulence.Noise2 = perlinNoise.Noise2;
            //turbulence.Noise3 = perlinNoise.Noise3;
            turbulence.Noise3 = improvedPerlinNoise.Noise3;

            // ※persistence = 1 に近い値だとノイズ値を足した時に [-1, 1] を越える値が多くなる。
            //turbulence.Persistence = 1;
            //turbulence.Persistence = 0.9f;
            turbulence.Persistence = (float) (1 / Math.Sqrt(2));
            //turbulence.Persistence = 60;
            turbulence.OctaveCount = 10;

            base.Initialize();
        }

        float GetValue2(float x, float y)
        {
            return turbulence.GetValue3(x, y, 0);
        }

        protected override void LoadContent()
        {
            // ノイズから height map を生成。
            heightMap.GetValue2 = GetValue2;
            heightMap.Size = 256 + 1;
            heightMap.Build();

            // normalize.
            //float minHeight = float.MaxValue;
            //float maxHeight = float.MinValue;
            //for (int i = 0; i < heights.Length; i++)
            //{
            //    var h = heights[i];
            //    minHeight = MathHelper.Min(minHeight, h);
            //    maxHeight = MathHelper.Max(maxHeight, h);
            //}
            //var factor = 1 / (maxHeight - minHeight);
            //for (int i = 0; i < heights.Length; i++)
            //{
            //    heights[i] = factor * (heights[i] - minHeight);
            //}

            // 色付けされた height map を生成。
            heightMapImage = new HeightMapImage(GraphicsDevice);
            heightMapImage.HeightColors.AddColor(-1.0000f, new Color(0, 0, 128));
            heightMapImage.HeightColors.AddColor(-0.2500f, new Color(0, 0, 255));
            heightMapImage.HeightColors.AddColor(0.0000f, new Color(0, 128, 255));
            heightMapImage.HeightColors.AddColor(0.0625f, new Color(240, 240, 64));
            heightMapImage.HeightColors.AddColor(0.1250f, new Color(32, 160, 0));
            heightMapImage.HeightColors.AddColor(0.3750f, new Color(224, 224, 0));
            //heightMapImage.HeightColors.AddColor(0.7500f, new Color(128, 128, 128));
            heightMapImage.HeightColors.AddColor(0.7500f, new Color(64, 64, 64));
            heightMapImage.HeightColors.AddColor(1.0000f, new Color(255, 255, 255));
            heightMapImage.LightingEnabled = true;
            heightMapImage.LightContrast = 3;
            heightMapImage.Build(heightMap.Size, heightMap.Heights);

            // 色付けされた height map を画像として実行ディレクトリに保存 (上書き保存)。
            using (var stream = new FileStream("ColoredHeightMap.png", FileMode.Create))
            {
                heightMapImage.ColoredHeightMap.SaveAsPng(stream, heightMap.Size, heightMap.Size);
            }

            // height map を貼り付ける正方形を作成。
            heightMapMesh = new VertexBuffer(GraphicsDevice, typeof(VertexPositionTexture), 4, BufferUsage.WriteOnly);
            var vertices = new VertexPositionTexture[4];
            vertices[0] = new VertexPositionTexture(new Vector3(-1.0f, 1.0f, 0.0f), new Vector2(0, 0));
            vertices[1] = new VertexPositionTexture(new Vector3(1.0f, 1.0f, 0.0f), new Vector2(1, 0));
            vertices[2] = new VertexPositionTexture(new Vector3(-1.0f, -1.0f, 0.0f), new Vector2(0, 1));
            vertices[3] = new VertexPositionTexture(new Vector3(1.0f, -1.0f, 0.0f), new Vector2(1, 1));
            heightMapMesh.SetData(vertices);

            // カメラ設定。
            var view = Matrix.CreateLookAt(new Vector3(0.0f, 0.0f, 3.0f), Vector3.Zero, Vector3.Up);
            var projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45.0f),
                (float) GraphicsDevice.Viewport.Width / (float) GraphicsDevice.Viewport.Height,
                1.0f,
                100.0f);

            // エフェクト設定。
            basicEffect = new BasicEffect(GraphicsDevice);
            basicEffect.TextureEnabled = true;
            basicEffect.View = view;
            basicEffect.Projection = projection;
            basicEffect.Texture = heightMapImage.ColoredHeightMap;
        }

        protected override void UnloadContent()
        {
            heightMapImage.Dispose();
            heightMapMesh.Dispose();
            basicEffect.Dispose();
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.SetVertexBuffer(heightMapMesh);
            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            foreach (var pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            }

            base.Draw(gameTime);
        }
    }
}
