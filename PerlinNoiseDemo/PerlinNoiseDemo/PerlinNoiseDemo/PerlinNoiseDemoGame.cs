#region Using

using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Willcraftia.Framework.Noise;

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

        const int noiseSeed = 300;

        GraphicsDeviceManager graphics;

        #region Noise Generators

        ClassicPerlin classicPerlin = new ClassicPerlin();

        Perlin perlin = new Perlin();

        Simplex simplex = new Simplex();

        Voronoi voronoi = new Voronoi();

        #endregion

        PerlinFractal perlinFractal = new PerlinFractal();

        #region Musgrave Fractal Algorithms

        SumFractal sumFractal = new SumFractal();

        Multifractal multifractal = new Multifractal();

        Heterofractal heterofractal = new Heterofractal();

        HybridMultifractal hybridMultifractal = new HybridMultifractal();

        RidgedMultifractal ridgedMultifractal = new RidgedMultifractal();

        SinFractal sinFractal = new SinFractal();

        #endregion

        Billow billow = new Billow();

        Turbulence turbulence = new Turbulence();

        #region Procedural texture

        Granite granite = new Granite();

        #endregion

        IModule finalModule;

        NoiseMapBuilder noiseMapBuilder = new NoiseMapBuilder();

        HeightMapImage[,] heightMapImages = new HeightMapImage[heightMapCountX, heightMapCountY];

        QuadMesh quadMesh;

        BasicEffect basicEffect;

        Matrix[,] heightMapMeshTransforms = new Matrix[heightMapCountX, heightMapCountY];

        public PerlinNoiseDemoGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            classicPerlin.Seed = noiseSeed;
            perlin.Seed = noiseSeed;
            simplex.Seed = noiseSeed;
            voronoi.Seed = noiseSeed;

            perlinFractal.Source = perlin.Sample;

            // Musgrave
            sumFractal.Source = perlin.Sample;
            multifractal.Source = perlin.Sample;
            heterofractal.Source = perlin.Sample;
            hybridMultifractal.Source = perlin.Sample;
            ridgedMultifractal.Source = perlin.Sample;
            sinFractal.Source = perlin.Sample;

            billow.Source = perlin.Sample;
            turbulence.Source = perlin.Sample;

            // Procedural texture
            granite.Initialize();

            finalModule = perlin;

            noiseMapBuilder.Source = finalModule.Sample;

            base.Initialize();
        }

        MidpointDisplacement md = new MidpointDisplacement();

        protected override void LoadContent()
        {
            var centerPosition = new Vector3
            {
                X = (heightMapMeshSize + heightMapMeshGap) * heightMapCountX * 0.5f,
                Y = (heightMapMeshSize + heightMapMeshGap) * heightMapCountY * 0.5f,
                Z = 0
            };

            //md.Width = heightMapSize;
            //md.Height = heightMapSize;
            //md.Build();

            var map = new NoiseMap();

            for (int i = 0; i < heightMapCountX; i++)
            {
                for (int j = 0; j < heightMapCountY; j++)
                {
                    map.Width = heightMapSize;
                    map.Height = heightMapSize;

                    noiseMapBuilder.NoiseMap = map;
                    noiseMapBuilder.Bounds = new Bounds { X = i, Y = j, Width = 1, Height = 1 };
                    //noiseMapBuilder.SeamlessEnabled = true;
                    noiseMapBuilder.Build();

                    float minV = float.MaxValue;
                    float maxV = float.MinValue;

                    map.ForEach((v) =>
                    {
                        minV = MathHelper.Min(minV, v);
                        maxV = MathHelper.Min(maxV, v);
                    });

                    Console.WriteLine("[{0}, {1}]", minV, maxV);

                    var image = new HeightMapImage(GraphicsDevice);
                    // height map
                    image.GradientColors.Add(-1.0000f, 0, 0, 128);
                    image.GradientColors.Add(-0.2500f, 0, 0, 255);
                    image.GradientColors.Add(0.0000f, 0, 128, 255);
                    image.GradientColors.Add(0.0625f, 240, 240, 64);
                    image.GradientColors.Add(0.1250f, 32, 160, 0);
                    image.GradientColors.Add(0.3750f, 224, 224, 0);
                    image.GradientColors.Add(0.7500f, 64, 64, 64);
                    image.GradientColors.Add(1.0000f, 255, 255, 255);
                    //image.LightingEnabled = true;
                    //image.LightContrast = 3;

                    // granite
                    //image.GradientColors.Add(-1.0000f, 0, 0, 0);
                    //image.GradientColors.Add(-0.9375f, 0, 0, 0);
                    //image.GradientColors.Add(-0.8750f, 216, 216, 242);
                    //image.GradientColors.Add(0.0000f, 191, 191, 191);
                    //image.GradientColors.Add(0.5000f, 210, 116, 125);
                    //image.GradientColors.Add(0.7500f, 210, 113, 98);
                    //image.GradientColors.Add(1.0000f, 255, 176, 192);

                    image.Build(heightMapSize, map.Values);
                    //image.Build(heightMapSize, md.Values);
                    heightMapImages[i, j] = image;

                    var position = new Vector3
                    {
                        X = (heightMapMeshSize + heightMapMeshGap) * i,
                        Y = (heightMapMeshSize + heightMapMeshGap) * (heightMapCountY - 1 - j),
                        Z = 0
                    };
                    position -= centerPosition;
                    Matrix.CreateTranslation(ref position, out heightMapMeshTransforms[i, j]);

                    var fileName = string.Format("ColoredHeightMap_{0}{1}.png", i, j);
                    using (var stream = new FileStream(fileName, FileMode.Create))
                    {
                        image.ColoredHeightMap.SaveAsPng(stream, heightMapSize, heightMapSize);
                    }
                }
            }

            quadMesh = new QuadMesh(GraphicsDevice, heightMapMeshSize);

            var view = Matrix.CreateLookAt(new Vector3(0.0f, 0.0f, 7.0f), Vector3.Zero, Vector3.Up);
            var projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45.0f),
                (float) GraphicsDevice.Viewport.Width / (float) GraphicsDevice.Viewport.Height,
                1.0f,
                100.0f);

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
            quadMesh.Dispose();
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
            GraphicsDevice.SetVertexBuffer(quadMesh.VertexBuffer);
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
