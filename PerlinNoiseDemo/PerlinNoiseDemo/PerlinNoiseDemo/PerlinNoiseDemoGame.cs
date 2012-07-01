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
        const int heightMapSize = 256 + 1;

        const float heightMapMeshSize = 1;

        const float heightMapMeshGap = 0.02f;

        const int heightMapCountX = 5;

        const int heightMapCountY = 5;

        GraphicsDeviceManager graphics;

        PerlinNoise perlinNoise = new PerlinNoise();

        ImprovedPerlinNoise improvedPerlinNoise = new ImprovedPerlinNoise();

        PerlinFractal perlinFractal = new PerlinFractal();

        SumFractal sumFractal = new SumFractal();

        Turbulence turbulence = new Turbulence();

        Multifractal multifractal = new Multifractal();

        Heterofractal heterofractal = new Heterofractal();

        HybridMultifractal hybridMultifractal = new HybridMultifractal();

        RidgedMultifractal ridgedMultifractal = new RidgedMultifractal();

        SinFractal sinFractal = new SinFractal();

        HeightMap[,] heightMaps = new HeightMap[heightMapCountX, heightMapCountY];

        HeightMapImage[,] heightMapImages = new HeightMapImage[heightMapCountX, heightMapCountY];

        HeightMapMesh heightMapMesh;

        BasicEffect basicEffect;

        Matrix[,] heightMapMeshTransforms = new Matrix[heightMapCountX, heightMapCountY];

        public PerlinNoiseDemoGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            perlinNoise.Seed = 300;
            improvedPerlinNoise.Seed = 300;

            perlinFractal.Noise1 = perlinNoise.Noise;
            perlinFractal.Noise2 = perlinNoise.Noise;
            perlinFractal.Noise3 = improvedPerlinNoise.Noise;

            sumFractal.Noise3 = improvedPerlinNoise.Noise;
            turbulence.Noise3 = improvedPerlinNoise.Noise;
            multifractal.Noise3 = improvedPerlinNoise.Noise;
            heterofractal.Noise3 = improvedPerlinNoise.Noise;
            hybridMultifractal.Noise3 = improvedPerlinNoise.Noise;
            ridgedMultifractal.Noise3 = improvedPerlinNoise.Noise;
            sinFractal.Noise3 = improvedPerlinNoise.Noise;

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

            var centerPosition = new Vector3
            {
                X = (heightMapMeshSize + heightMapMeshGap) * heightMapCountX * 0.5f,
                Y = (heightMapMeshSize + heightMapMeshGap) * heightMapCountY * 0.5f,
                Z = 0
            };

            for (int i = 0; i < heightMapCountX; i++)
            {
                for (int j = 0; j < heightMapCountY; j++)
                {
                    // 色付けされた height map を生成。
                    var image = new HeightMapImage(GraphicsDevice);
                    image.HeightColors.AddColor(-1.0000f, new Color(0, 0, 128));
                    image.HeightColors.AddColor(-0.2500f, new Color(0, 0, 255));
                    image.HeightColors.AddColor(0.0000f, new Color(0, 128, 255));
                    image.HeightColors.AddColor(0.0625f, new Color(240, 240, 64));
                    image.HeightColors.AddColor(0.1250f, new Color(32, 160, 0));
                    image.HeightColors.AddColor(0.3750f, new Color(224, 224, 0));
                    //image.HeightColors.AddColor(0.7500f, new Color(128, 128, 128));
                    image.HeightColors.AddColor(0.7500f, new Color(64, 64, 64));
                    image.HeightColors.AddColor(1.0000f, new Color(255, 255, 255));
                    image.LightingEnabled = true;
                    image.LightContrast = 3;
                    image.Build(heightMapSize, heightMaps[i, j].Heights);

                    heightMapImages[i, j] = image;

                    // 各 height map のメッシュの座標を調整。
                    var position = new Vector3
                    {
                        X = (heightMapMeshSize + heightMapMeshGap) * i,
                        Y = (heightMapMeshSize + heightMapMeshGap) * (heightMaps.GetLength(1) - 1 - j),
                        Z = 0
                    };
                    position -= centerPosition;
                    Matrix.CreateTranslation(ref position, out heightMapMeshTransforms[i, j]);

                    // 色付けされた height map を画像として実行ディレクトリに保存 (上書き保存)。
                    var fileName = string.Format("ColoredHeightMap_{0}{1}.png", heightMapCountX, heightMapCountY);
                    using (var stream = new FileStream(fileName, FileMode.Create))
                    {
                        image.ColoredHeightMap.SaveAsPng(stream, heightMapSize, heightMapSize);
                    }
                }
            }

            // height map を貼り付ける正方形を作成。
            heightMapMesh = new HeightMapMesh(GraphicsDevice, heightMapMeshSize);

            // カメラ設定。
            var view = Matrix.CreateLookAt(new Vector3(0.0f, 0.0f, 7.0f), Vector3.Zero, Vector3.Up);
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
        }

        protected override void UnloadContent()
        {
            for (int i = 0; i < heightMapCountX; i++)
            {
                for (int j = 0; j < heightMapCountY; j++)
                {
                    heightMapImages[i, j].Dispose();
                }
            }
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
            GraphicsDevice.SetVertexBuffer(heightMapMesh.VertexBuffer);
            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            for (int i = 0; i < heightMapCountX; i++)
            {
                for (int j = 0; j < heightMapCountY; j++)
                {
                    DrawHeightMapMesh(i, j);
                }
            }

            base.Draw(gameTime);
        }

        protected void DrawHeightMapMesh(int x, int y)
        {
            basicEffect.World = heightMapMeshTransforms[x, y];
            basicEffect.Texture = heightMapImages[x, y].ColoredHeightMap;

            foreach (var pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            }
        }
    }
}
