#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace PerlinNoiseDemo
{
    /// <summary>
    /// Height map をテクスチャとして生成するためのクラスです。
    /// </summary>
    public sealed class HeightMapImage : IDisposable
    {
        GraphicsDevice graphicsDevice;

        HeightColorCollection heightColors = new HeightColorCollection();

        float sqrt2 = (float) Math.Sqrt(2);

        float lightAzimuth;
        
        float lightElevation;
        
        float lightContrast = 1;

        float lightBrightness = 1;

        Vector4 lightColor = Vector4.One;

        float cosAzimuth;
        
        float sinAzimuth;
        
        float cosElevation;
        
        float sinElevation;
        
        /// <summary>
        /// HeightColorCollection を取得します。
        /// </summary>
        public HeightColorCollection HeightColors
        {
            get { return heightColors; }
        }

        /// <summary>
        /// 高度に応じて色付けされた height map をテクスチャとして取得します。
        /// </summary>
        public Texture2D ColoredHeightMap { get; private set; }

        /// <summary>
        /// ライティングが有効であるかどうかを示す値を取得または設定します。
        /// </summary>
        /// <value>
        /// true (ライティングが有効)、false (それ以外の場合)。
        /// </value>
        public bool LightingEnabled { get; set; }

        /// <summary>
        /// X-Z 平面についてのライトの角度 (ラジアン) を取得または設定します。
        /// </summary>
        public float LightAzimuth
        {
            get { return lightAzimuth; }
            set
            {
                if (lightAzimuth == value) return;

                lightAzimuth = value;
                cosAzimuth = (float) Math.Cos(lightAzimuth);
                sinAzimuth = (float) Math.Sin(lightAzimuth);
            }
        }

        /// <summary>
        /// X-Y 平面についてのライトの角度 (ラジアン) を取得または設定します。
        /// </summary>
        public float LightElevation
        {
            get { return lightElevation; }
            set
            {
                if (lightElevation == value) return;

                lightElevation = value;
                cosElevation = (float) Math.Cos(lightElevation);
                sinElevation = (float) Math.Sin(lightElevation);
            }
        }

        /// <summary>
        /// ライト色のコントラストの度合いを取得または設定します。
        /// </summary>
        public float LightContrast
        {
            get { return lightContrast; }
            set { lightContrast = value; }
        }

        /// <summary>
        /// ライト色の明るさの度合いを取得または設定します。
        /// </summary>
        public float LightBrightness
        {
            get { return lightBrightness; }
            set { lightBrightness = value; }
        }

        /// <summary>
        /// ライト色を取得または設定します。
        /// </summary>
        public Vector4 LightColor
        {
            get { return lightColor; }
            set { lightColor = value; }
        }

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        /// <param name="graphicsDevice"></param>
        public HeightMapImage(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;

            LightAzimuth = MathHelper.PiOver4;
            LightElevation = MathHelper.PiOver4;
        }

        /// <summary>
        /// テクスチャを生成します。
        /// </summary>
        /// <param name="size"></param>
        /// <param name="heights"></param>
        public void Build(int size, float[] heights)
        {
            if (heights == null) throw new InvalidOperationException("'heights' is null.");
            if (size <= 0) throw new InvalidOperationException("'size' must be a positive value.");
            if (heights.Length != size * size) throw new InvalidOperationException("The length of 'heights' is invalid.");

            if (ColoredHeightMap != null &&
                (ColoredHeightMap.Width != size || ColoredHeightMap.Height != size))
            {
                ColoredHeightMap.Dispose();
                ColoredHeightMap = null;
            }

            if (ColoredHeightMap == null)
            {
                ColoredHeightMap = new Texture2D(graphicsDevice, size, size, false, SurfaceFormat.Color);
            }

            var coloredHeights = new Color[heights.Length];
            for (int i = 0; i < heights.Length; i++)
            {
                Vector4 c;

                heightColors.GetColor(heights[i], out c);

                if (LightingEnabled) Light(size, heights, i, ref c);

                coloredHeights[i] = new Color(c.X, c.Y, c.Z, c.W);
            }

            ColoredHeightMap.SetData(coloredHeights);
        }

        /// <summary>
        /// ライティングによる色を算出します。
        /// </summary>
        /// <param name="size"></param>
        /// <param name="heights"></param>
        /// <param name="index"></param>
        /// <param name="color"></param>
        void Light(int size, float[] heights, int index, ref Vector4 color)
        {
            var x = index % size;
            var y = index / size;

            int offsetL;
            int offsetR;

            if (x == 0)
            {
                offsetL = 0;
                offsetR = 1;
            }
            else if (x == size - 1)
            {
                offsetL = -1;
                offsetR = 0;
            }
            else
            {
                offsetL = -1;
                offsetR = 1;
            }

            int offsetD;
            int offsetU;

            if (y == 0)
            {
                offsetD = 0;
                offsetU = 1;
            }
            else if (y == size - 1)
            {
                offsetD = -1;
                offsetU = 0;
            }
            else
            {
                offsetD = -1;
                offsetU = 1;
            }

            offsetD *= size;
            offsetU *= size;

            var center = heights[index];
            var left = heights[index + offsetL];
            var right = heights[index + offsetR];
            var down = heights[index + offsetD];
            var up = heights[index + offsetU];

            var intensity = CalculateLightIntensity(center, left, right, down, up);
            intensity *= lightBrightness;

            var currentLightColor = lightColor * intensity;

            var r = color.X * currentLightColor.X;
            var g = color.Y * currentLightColor.Y;
            var b = color.Z * currentLightColor.Z;

            color.X = MathHelper.Clamp(r, 0, 1);
            color.Y = MathHelper.Clamp(g, 0, 1);
            color.Z = MathHelper.Clamp(b, 0, 1);
        }

        /// <summary>
        /// ライトの強さを算出します。
        /// </summary>
        /// <param name="center"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="down"></param>
        /// <param name="up"></param>
        /// <returns></returns>
        float CalculateLightIntensity(float center, float left, float right, float down, float up)
        {
            float io = (float) (sqrt2 * sinElevation * 0.5f);
            float ix = (1.0f - io) * lightContrast * sqrt2 * cosElevation * cosAzimuth;
            float iy = (1.0f - io) * lightContrast * sqrt2 * cosElevation * sinAzimuth;
            float intensity = ix * (left - right) + iy * (down + up) + io;
            if (intensity < 0) intensity = 0;
            return intensity;
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed;

        ~HeightMapImage()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                if (ColoredHeightMap != null) ColoredHeightMap.Dispose();
            }

            disposed = true;
        }

        #endregion
    }
}
