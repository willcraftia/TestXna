#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Framework.Noise
{
    public sealed class NoiseMapBuilder
    {
        SampleSourceDelegate source;

        NoiseMap noiseMap;

        Bounds bounds = Bounds.One;

        bool seamlessEnabled;

        public SampleSourceDelegate Source
        {
            get { return source; }
            set { source = value; }
        }

        public NoiseMap NoiseMap
        {
            get { return noiseMap; }
            set { noiseMap = value; }
        }

        public Bounds Bounds
        {
            get { return bounds; }
            set { bounds = value; }
        }

        public bool SeamlessEnabled
        {
            get { return seamlessEnabled; }
            set { seamlessEnabled = value; }
        }

        public void Build()
        {
            if (noiseMap == null)
                throw new InvalidOperationException("NoiseMap is null.");
            if (bounds.Width <= 0)
                throw new InvalidOperationException(string.Format("Bounds.Width <= 0: {0}", bounds.Width));
            if (bounds.Height <= 0)
                throw new InvalidOperationException(string.Format("Bounds.Height <= 0: {0}", bounds.Height));

            noiseMap.Initialize();

            var w = noiseMap.Width;
            var h = noiseMap.Height;

            if (w == 0 || h == 0)
                return;

            var deltaX = bounds.Width / (float) w;
            var deltaY = bounds.Height / (float) h;

            float y = bounds.Y;
            for (int i = 0; i < h; i++)
            {
                float x = bounds.X;
                for (int j = 0; j < w; j++)
                {
                    float value;

                    if (!seamlessEnabled)
                    {
                        value = Source(x, 0, y);
                    }
                    else
                    {
                        float sw = Source(x, 0, y);
                        float se = Source(x + bounds.Width, 0, y);
                        float nw = Source(x, 0, y + bounds.Height);
                        float ne = Source(x + bounds.Width, 0, y + bounds.Height);
                        float xa = 1 - ((x - bounds.X) / bounds.Width);
                        float ya = 1 - ((y - bounds.Y) / bounds.Height);
                        float y0 = MathHelper.Lerp(sw, se, xa);
                        float y1 = MathHelper.Lerp(nw, ne, xa);
                        value = MathHelper.Lerp(y0, y1, ya);
                    }

                    noiseMap[j, i] = value;

                    x += deltaX;
                }
                y += deltaY;
            }
        }
    }
}
