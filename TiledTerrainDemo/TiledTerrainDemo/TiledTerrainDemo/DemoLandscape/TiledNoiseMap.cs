#region Using

using System;
using TiledTerrainDemo.Noise;

#endregion

namespace TiledTerrainDemo.DemoLandscape
{
    public sealed class TiledNoiseMap
    {
        public const int DefaultOverlapSize = 1;

        NoiseMap backingNoiseMap = new NoiseMap();

        int width;

        int height;

        int overlapSize = DefaultOverlapSize;

        float boundX;

        float boundY;

        float boundWidth;

        float boundHeight;

        public NoiseDelegate Noise
        {
            get { return backingNoiseMap.Noise; }
            set { backingNoiseMap.Noise = value; }
        }

        public int Width
        {
            get { return width; }
            set
            {
                width = value;
                backingNoiseMap.Width = value + 2 * overlapSize;
            }
        }

        public int Height
        {
            get { return height; }
            set
            {
                height = value;
                backingNoiseMap.Height = value + 2 * overlapSize;
            }
        }

        public int OverlapSize
        {
            get { return overlapSize; }
            set { overlapSize = value; }
        }

        // ActualWidth = Width + 2 * OverlapSize
        public int ActualWidth
        {
            get { return backingNoiseMap.Width; }
        }

        // ActualHeight = Height + 2 * OverlapSize;
        public int ActualHeight
        {
            get { return backingNoiseMap.Height; }
        }

        public float[] Values
        {
            get { return backingNoiseMap.Values; }
        }

        public float this[int x, int y]
        {
            get
            {
                x += overlapSize;
                y += overlapSize;
                return backingNoiseMap.Values[x + y * backingNoiseMap.Width];
            }
        }

        public void SetBounds(float x, float y, float w, float h)
        {
            boundX = x;
            boundY = y;
            boundWidth = w;
            boundHeight = h;
        }

        public void Build()
        {
            //var dx = boundWidth * (1 / (float) backingNoiseMap.Width);
            //var dy = boundHeight * (1 / (float) backingNoiseMap.Height);
            //backingNoiseMap.SetBounds(boundX, boundY, boundWidth + dx, boundWidth + dy);

            // overlap the bounds of the next noise map to calculate normals correctly on the edge of a height map.
            var dx = boundWidth / (float) width;
            var dy = boundHeight / (float) height;
            var bx = boundX - dx * overlapSize;
            var by = boundY - dy * overlapSize;
            var bw = boundWidth + dx + (2 * dx * overlapSize);
            var bh = boundWidth + dy + (2 * dy * overlapSize);
            backingNoiseMap.SetBounds(bx, by, bw, bh);

            backingNoiseMap.Build();
        }

        // todo: a temporary implementation
        public void Erode(float talus, int iterations)
        {
            for (int i = 0; i < iterations; i++)
            {
                for (int y = 1 + overlapSize; y < backingNoiseMap.Height - 2 - 2 * overlapSize; y++)
                //for (int y = 1; y < backingNoiseMap.Height - 2; y++)
                {
                    for (int x = 1 + overlapSize; x < backingNoiseMap.Width - 2 - 2 * overlapSize; x++)
                    //for (int x = 1; x < backingNoiseMap.Width - 2; x++)
                    {
                        var h = backingNoiseMap[x, y];
                        var h1 = backingNoiseMap[x, y + 1];
                        var h2 = backingNoiseMap[x - 1, y];
                        var h3 = backingNoiseMap[x + 1, y];
                        var h4 = backingNoiseMap[x, y - 1];

                        var d1 = h - h1;
                        var d2 = h - h2;
                        var d3 = h - h3;
                        var d4 = h - h4;

                        var a = 0;
                        var b = 0;

                        float maxD = 0;
                        if (maxD < d1)
                        {
                            maxD = d1;
                            b = 1;
                        }
                        if (maxD < d2)
                        {
                            maxD = d2;
                            a = -1;
                            b = 0;
                        }
                        if (maxD < d3)
                        {
                            maxD = d3;
                            a = 1;
                            b = 0;
                        }
                        if (maxD < d4)
                        {
                            maxD = d4;
                            a = 0;
                            b = -1;
                        }

                        if (talus < maxD) continue;

                        maxD *= 0.5f;
                        backingNoiseMap[x, y] -= maxD;
                        backingNoiseMap[x + a, y + b] += maxD;
                    }
                }
            }
        }
    }
}
