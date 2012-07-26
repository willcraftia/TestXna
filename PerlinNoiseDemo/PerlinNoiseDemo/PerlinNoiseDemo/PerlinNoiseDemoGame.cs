#region Using

using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Willcraftia.Framework.Noise;
using Willcraftia.Framework.Terrain;
using Willcraftia.Framework.Terrain.Helpers;

#endregion

namespace PerlinNoiseDemo
{
    public class PerlinNoiseDemoGame : Game
    {
        public const int Size = 256 + 1;

        public const float MeshSize = 1;

        public const float MeshGap = 0.02f;

        public const int CountX = 5;

        public const int CountY = 5;

        public const int NoiseSeed = 300;

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

        HeightMap heightMap = new HeightMap(Size, Size);

        NoiseMapBuilder noiseMapBuilder = new NoiseMapBuilder();

        HeightMapImageBuilder imageBuilder = new HeightMapImageBuilder();

        Texture2D[,] textures = new Texture2D[CountX, CountY];

        QuadMesh mesh;

        BasicEffect effect;

        Matrix[,] transforms = new Matrix[CountX, CountY];

        public PerlinNoiseDemoGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            classicPerlin.Seed = NoiseSeed;
            perlin.Seed = NoiseSeed;
            simplex.Seed = NoiseSeed;
            voronoi.Seed = NoiseSeed;

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

            finalModule = sumFractal;

            noiseMapBuilder.Source = finalModule.Sample;
            //noiseMapBuilder.SeamlessEnabled = true;

            // height map
            imageBuilder.GradientColors.Add(-1.0000f, 0, 0, 128);
            imageBuilder.GradientColors.Add(-0.2500f, 0, 0, 255);
            imageBuilder.GradientColors.Add(0.0000f, 0, 128, 255);
            imageBuilder.GradientColors.Add(0.0625f, 240, 240, 64);
            imageBuilder.GradientColors.Add(0.1250f, 32, 160, 0);
            imageBuilder.GradientColors.Add(0.3750f, 224, 224, 0);
            imageBuilder.GradientColors.Add(0.7500f, 64, 64, 64);
            imageBuilder.GradientColors.Add(1.0000f, 255, 255, 255);
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

            base.Initialize();
        }

        protected override void LoadContent()
        {
            var centerPosition = new Vector3
            {
                X = (MeshSize + MeshGap) * CountX * 0.5f,
                Y = (MeshSize + MeshGap) * CountY * 0.5f,
                Z = 0
            };

            for (int i = 0; i < CountX; i++)
            {
                for (int j = 0; j < CountY; j++)
                {
                    noiseMapBuilder.Destination = heightMap;
                    noiseMapBuilder.Bounds = new Bounds { X = i, Y = j, Width = 1, Height = 1 };
                    noiseMapBuilder.Build();

                    Console.WriteLine("[{0}, {1}]", heightMap.Min(), heightMap.Max());

                    textures[i, j] = new Texture2D(GraphicsDevice, Size, Size, false, SurfaceFormat.Color);
                    imageBuilder.Source = heightMap;
                    imageBuilder.Destination = textures[i, j];
                    imageBuilder.Build();

                    var position = new Vector3
                    {
                        X = (MeshSize + MeshGap) * i,
                        Y = (MeshSize + MeshGap) * (CountY - 1 - j),
                        Z = 0
                    };
                    position -= centerPosition;
                    Matrix.CreateTranslation(ref position, out transforms[i, j]);

                    //var fileName = string.Format("HeightMap_{0}{1}.png", i, j);
                    //using (var stream = new FileStream(fileName, FileMode.Create))
                    //{
                    //    imageBuilder.Destination.SaveAsPng(stream, heightMap.Width, heightMap.Height);
                    //}
                }
            }

            mesh = new QuadMesh(GraphicsDevice, MeshSize);

            var view = Matrix.CreateLookAt(new Vector3(0.0f, 0.0f, 7.0f), Vector3.Zero, Vector3.Up);
            var projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45.0f),
                (float) GraphicsDevice.Viewport.Width / (float) GraphicsDevice.Viewport.Height,
                1.0f,
                100.0f);

            effect = new BasicEffect(GraphicsDevice);
            effect.TextureEnabled = true;
            effect.View = view;
            effect.Projection = projection;
        }

        protected override void UnloadContent()
        {
            for (int i = 0; i < CountX; i++)
                for (int j = 0; j < CountY; j++)
                    textures[i, j].Dispose();

            mesh.Dispose();
            effect.Dispose();
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.SetVertexBuffer(mesh.VertexBuffer);
            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            for (int i = 0; i < CountX; i++)
                for (int j = 0; j < CountY; j++)
                    DrawHeightMapMesh(i, j);

            base.Draw(gameTime);
        }

        protected void DrawHeightMapMesh(int x, int y)
        {
            effect.World = transforms[x, y];
            effect.Texture = textures[x, y];

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            }
        }
    }
}
