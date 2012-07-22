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

        INoiseModule finalNoise;

        NoiseMap[,] noiseMaps = new NoiseMap[heightMapCountX, heightMapCountY];

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

            perlinFractal.Noise = perlin.GetValue;

            // Musgrave
            sumFractal.Noise = perlin.GetValue;
            multifractal.Noise = perlin.GetValue;
            heterofractal.Noise = perlin.GetValue;
            hybridMultifractal.Noise = perlin.GetValue;
            ridgedMultifractal.Noise = perlin.GetValue;
            sinFractal.Noise = perlin.GetValue;

            billow.Noise = perlin.GetValue;
            turbulence.Noise = perlin.GetValue;

            finalNoise = sumFractal;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            for (int i = 0; i < heightMapCountX; i++)
            {
                for (int j = 0; j < heightMapCountY; j++)
                {
                    var map = new NoiseMap();

                    map.Noise = finalNoise.GetValue;
                    map.Width = heightMapSize;
                    map.Height = heightMapSize;
                    map.Bounds = new Bounds { X = i, Y = j, Width = 1, Height = 1 };
                    map.Build();

                    noiseMaps[i, j] = map;
                }
            }

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
                    image.Build(heightMapSize, noiseMaps[i, j].Values);

                    heightMapImages[i, j] = image;

                    var position = new Vector3
                    {
                        X = (heightMapMeshSize + heightMapMeshGap) * i,
                        Y = (heightMapMeshSize + heightMapMeshGap) * (noiseMaps.GetLength(1) - 1 - j),
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
