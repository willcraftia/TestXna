#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework.Noise
{
    public sealed class NoiseMapBuilder
    {
        SampleSourceDelegate source;

        IMap<float> destination;

        Bounds bounds = Bounds.One;

        bool seamlessEnabled;

        public SampleSourceDelegate Source
        {
            get { return source; }
            set { source = value; }
        }

        public IMap<float> Destination
        {
            get { return destination; }
            set { destination = value; }
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
            if (destination == null)
                throw new InvalidOperationException("NoiseMap is null.");
            if (bounds.Width <= 0)
                throw new InvalidOperationException(string.Format("Bounds.Width <= 0: {0}", bounds.Width));
            if (bounds.Height <= 0)
                throw new InvalidOperationException(string.Format("Bounds.Height <= 0: {0}", bounds.Height));

            var w = destination.Width;
            var h = destination.Height;

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
                        value = source(x, 0, y);
                    }
                    else
                    {
                        float sw = source(x, 0, y);
                        float se = source(x + bounds.Width, 0, y);
                        float nw = source(x, 0, y + bounds.Height);
                        float ne = source(x + bounds.Width, 0, y + bounds.Height);
                        float xa = 1 - ((x - bounds.X) / bounds.Width);
                        float ya = 1 - ((y - bounds.Y) / bounds.Height);
                        float y0 = MathHelper.Lerp(sw, se, xa);
                        float y1 = MathHelper.Lerp(nw, ne, xa);
                        value = MathHelper.Lerp(y0, y1, ya);
                    }

                    destination[j, i] = value;

                    x += deltaX;
                }
                y += deltaY;
            }
        }
    }
}
