#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Terrain.Helpers
{
    public sealed class HeightMapImageBuilder
    {
        GradientColorCollection gradientColors = new GradientColorCollection();

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

        Color[] buffer;

        public GradientColorCollection GradientColors
        {
            get { return gradientColors; }
        }

        public IMap Source { get; set; }

        public Texture2D Destination { get; set; }

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
            if (Source == null)
                throw new InvalidOperationException("Source is null.");
            if (Destination == null)
                throw new InvalidOperationException("Destination is null.");
            if (Source.Width != Destination.Width)
                throw new InvalidOperationException("Source.Width != Destination.Width");
            if (Source.Height != Destination.Height)
                throw new InvalidOperationException("Source.Width != Destination.Width");

            int w = Source.Width;
            int h = Source.Height;
            int size = w * h;

            if (buffer == null || buffer.Length != size)
                buffer = new Color[size];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Vector4 c;

                    gradientColors.Get(Source[x, y], out c);

                    if (LightingEnabled)
                        Light(x, y, ref c);

                    buffer[x + y * w] = new Color(c.X, c.Y, c.Z, c.W);
                }
            }

            Destination.SetData(buffer);
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
            else if (x == Source.Width - 1)
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
            else if (y == Source.Height - 1)
            {
                offsetD = -1;
                offsetU = 0;
            }
            else
            {
                offsetD = -1;
                offsetU = 1;
            }

            var center = Source[x, y];
            var left = Source[x + offsetL, y];
            var right = Source[x + offsetR, y];
            var down = Source[x, y + offsetD];
            var up = Source[x, y + offsetU];

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
