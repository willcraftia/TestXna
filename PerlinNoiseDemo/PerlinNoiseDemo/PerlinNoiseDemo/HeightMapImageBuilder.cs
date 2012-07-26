#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace PerlinNoiseDemo
{
    /// <summary>
    /// The class generates a height map texture.
    /// </summary>
    public sealed class HeightMapImageBuilder
    {
        GradientColorCollection gradientColors = new GradientColorCollection();

        float[,] source;

        Texture2D destination;

        Color[] buffer;

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

        public GradientColorCollection GradientColors
        {
            get { return gradientColors; }
        }

        public float[,] Source
        {
            get { return source; }
            set { source = value; }
        }

        public Texture2D Destination
        {
            get { return destination; }
            set { destination = value; }
        }

        public bool LightingEnabled { get; set; }

        /// <summary>
        /// Gets/sets the angle (radian) of the light on x-z plane.
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
        /// Gets/sets the angle (radian) of the light on x-y plane.
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

        public float LightContrast
        {
            get { return lightContrast; }
            set { lightContrast = value; }
        }

        public float LightBrightness
        {
            get { return lightBrightness; }
            set { lightBrightness = value; }
        }

        public Vector4 LightColor
        {
            get { return lightColor; }
            set { lightColor = value; }
        }

        public HeightMapImageBuilder()
        {
            LightAzimuth = MathHelper.PiOver4;
            LightElevation = MathHelper.PiOver4;
        }

        public void Build()
        {
            if (source == null)
                throw new InvalidOperationException("Source is null.");
            if (destination == null)
                throw new InvalidOperationException("Destination is null.");
            if (source.GetLength(0) != destination.Width)
                throw new InvalidOperationException("Source.GetLength(0) != Destination.Width");
            if (source.GetLength(1) != destination.Height)
                throw new InvalidOperationException("Source.GetLength(1) != Destination.Height");

            var w = source.GetLength(0);
            var h = source.GetLength(1);
            var size = w * h;

            if (buffer == null || buffer.Length != size)
                buffer = new Color[size];

            Array.Clear(buffer, 0, buffer.Length);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Vector4 c;
                    gradientColors.Get(source[x, y], out c);

                    if (LightingEnabled)
                        Light(x, y, ref c);

                    buffer[x + y * w] = new Color(c.X, c.Y, c.Z, c.W);
                }
            }

            destination.SetData(buffer);
        }

        void Light(int x, int y, ref Vector4 color)
        {
            int offsetL;
            int offsetR;

            if (x == 0)
            {
                offsetL = 0;
                offsetR = 1;
            }
            else if (x == source.GetLength(0) - 1)
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
            else if (y == source.GetLength(1) - 1)
            {
                offsetD = -1;
                offsetU = 0;
            }
            else
            {
                offsetD = -1;
                offsetU = 1;
            }

            var center = source[x, y];
            var left = source[x + offsetL, y];
            var right = source[x + offsetR, y];
            var down = source[x, y + offsetD];
            var up = source[x, y + offsetU];

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

        float CalculateLightIntensity(float center, float left, float right, float down, float up)
        {
            float io = (float) (sqrt2 * sinElevation * 0.5f);
            float ix = (1.0f - io) * lightContrast * sqrt2 * cosElevation * cosAzimuth;
            float iy = (1.0f - io) * lightContrast * sqrt2 * cosElevation * sinAzimuth;
            float intensity = ix * (left - right) + iy * (down + up) + io;
            if (intensity < 0) intensity = 0;
            return intensity;
        }
    }
}
