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

        const float meshSize = 1;

        const float meshGap = 0.02f;

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

        HeightMapSource heightMap = new HeightMapSource(heightMapSize, heightMapSize);

        NoiseMapBuilder noiseMapBuilder = new NoiseMapBuilder();

        MidpointDisplacement md = new MidpointDisplacement();

        HeightMapImageBuilder heightMapImageBuilder = new HeightMapImageBuilder();

        Texture2D[,] textures = new Texture2D[heightMapCountX, heightMapCountY];

        QuadMesh quadMesh;

        BasicEffect basicEffect;

        Matrix[,] transforms = new Matrix[heightMapCountX, heightMapCountY];

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
            //noiseMapBuilder.SeamlessEnabled = true;

            md.Seed = 300;

            // height map
            heightMapImageBuilder.GradientColors.Add(-1.0000f, 0, 0, 128);
            heightMapImageBuilder.GradientColors.Add(-0.2500f, 0, 0, 255);
            heightMapImageBuilder.GradientColors.Add(0.0000f, 0, 128, 255);
            heightMapImageBuilder.GradientColors.Add(0.0625f, 240, 240, 64);
            heightMapImageBuilder.GradientColors.Add(0.1250f, 32, 160, 0);
            heightMapImageBuilder.GradientColors.Add(0.3750f, 224, 224, 0);
            heightMapImageBuilder.GradientColors.Add(0.7500f, 64, 64, 64);
            heightMapImageBuilder.GradientColors.Add(1.0000f, 255, 255, 255);
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
                X = (meshSize + meshGap) * heightMapCountX * 0.5f,
                Y = (meshSize + meshGap) * heightMapCountY * 0.5f,
                Z = 0
            };

            for (int i = 0; i < heightMapCountX; i++)
            {
                for (int j = 0; j < heightMapCountY; j++)
                {
                    // Noise.
                    //noiseMapBuilder.Destination = heightMap;
                    //noiseMapBuilder.Bounds = new Bounds { X = i, Y = j, Width = 1, Height = 1 };
                    //noiseMapBuilder.Build();

                    // Midpoint displacemenet.
                    md.Destination = heightMap;
                    md.BoundX = i * (heightMap.Width - 1);
                    md.BoundY = j * (heightMap.Height - 1);
                    md.Build();

                    Console.WriteLine("[{0}, {1}]", heightMap.Min(), heightMap.Max());

                    textures[i, j] = new Texture2D(GraphicsDevice, heightMap.Width, heightMap.Height, false, SurfaceFormat.Color);
                    heightMapImageBuilder.Source = heightMap;
                    heightMapImageBuilder.Destination = textures[i, j];
                    heightMapImageBuilder.Build();

                    var position = new Vector3
                    {
                        X = (meshSize + meshGap) * i,
                        Y = (meshSize + meshGap) * (heightMapCountY - 1 - j),
                        Z = 0
                    };
                    position -= centerPosition;
                    Matrix.CreateTranslation(ref position, out transforms[i, j]);

                    var fileName = string.Format("HeightMap_{0}{1}.png", i, j);
                    using (var stream = new FileStream(fileName, FileMode.Create))
                    {
                        heightMapImageBuilder.Destination.SaveAsPng(stream, heightMap.Width, heightMap.Height);
                    }
                }
            }

            quadMesh = new QuadMesh(GraphicsDevice, meshSize);

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
                for (int j = 0; j < heightMapCountY; j++)
                    textures[i, j].Dispose();

            quadMesh.Dispose();
            basicEffect.Dispose();
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
            GraphicsDevice.SetVertexBuffer(quadMesh.VertexBuffer);
            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            for (int i = 0; i < heightMapCountX; i++)
                for (int j = 0; j < heightMapCountY; j++)
                    DrawHeightMapMesh(i, j);

            base.Draw(gameTime);
        }

        protected void DrawHeightMapMesh(int x, int y)
        {
            basicEffect.World = transforms[x, y];
            basicEffect.Texture = textures[x, y];

            foreach (var pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            }
        }
    }
}
