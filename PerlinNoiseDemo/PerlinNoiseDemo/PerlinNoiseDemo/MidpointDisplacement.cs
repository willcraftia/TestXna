#region Using

using System;
using Willcraftia.Framework.Noise;

#endregion

namespace PerlinNoiseDemo
{
    public sealed class MidpointDisplacement
    {
        int seed = Environment.TickCount;

        public int Seed
        {
            get { return seed; }
            set { seed = value; }
        }

        NoiseMap noiseMap;

        int boundX;

        int boundY;

        public NoiseMap NoiseMap
        {
            get { return noiseMap; }
            set { noiseMap = value; }
        }

        public int BoundX
        {
            get { return boundX; }
            set { boundX = value; }
        }

        public int BoundY
        {
            get { return boundY; }
            set { boundY = value; }
        }

        public void Build()
        {
            if (noiseMap == null)
                throw new InvalidOperationException("NoiseMap is null.");

            noiseMap.Initialize();

            var w = noiseMap.Width;
            var h = noiseMap.Height;

            if (w == 0 || h == 0)
                return;

            int size = ((w < h) ? w : h) - 1;
            int offset = size / 2;

            noiseMap[   0,    0] = GetPosition(   0 + boundX, 0,    0 + boundY, Seed);
            noiseMap[size,    0] = GetPosition(size + boundX, 0,    0 + boundY, Seed);
            noiseMap[   0, size] = GetPosition(   0 + boundX, 0, size + boundY, Seed);
            noiseMap[size, size] = GetPosition(size + boundX, 0, size + boundY, Seed);

            float modifier = 0.7f;
            float range = 1;

            while (0 < offset)
            {
                bool oddY = false;

                for (int y = 0; y < h; y += offset, oddY = !oddY)
                {
                    bool oddX = false;

                    for (int x = 0; x < w; x += offset, oddX = !oddX)
                    {
                        if (!oddX && !oddY) continue;

                        int x0 = x - offset;
                        int x1 = x + offset;
                        int y0 = y - offset;
                        int y1 = y + offset;

                        float value;

                        if (oddX && oddY)
                        {
                            float v0 = noiseMap[x0, y0];
                            float v1 = noiseMap[x1, y0];
                            float v2 = noiseMap[x0, y1];
                            float v3 = noiseMap[x1, y1];
                            value = (v0 + v1 + v2 + v3) * 0.25f;
                        }
                        else
                        {
                            if (oddX)
                            {
                                float v0 = noiseMap[x0, y];
                                float v1 = noiseMap[x1, y];
                                value = (v0 + v1) * 0.5f;
                            }
                            else
                            {
                                float v0 = noiseMap[x, y0];
                                float v1 = noiseMap[x, y1];
                                value = (v0 + v1) * 0.5f;
                            }
                        }

                        // Add a small error.
                        value += GetPosition(x + boundX, 0, y + boundY, Seed) * range;

                        // Set the value.
                        noiseMap[x, y] = value;
                    }
                }

                range *= modifier;
                offset /= 2;
            }
        }

        float GetPosition(int x, int y, int z, int seed)
        {
            // 1073741824 = 1000000000000000000000000000000 (bit)
            return 1.0f - ((float) GetIntRandom(x, y, z, seed)) / 1073741824.0f;
        }

        int GetIntRandom(int x, int y, int z, int seed)
        {
            // define primes.
            const int primX = 1619;
            const int primY = 31337;
            const int primZ = 6971;
            const int primSeed = 1013;

            int n = (primX * x + primY * y + primZ * z + primSeed * seed);

            n = (n << 13) ^ n;
            // 60493, 19990303, 1376312589 are primes.
            // 0x7fffffff = 2147483647 (decimal) = 1111111111111111111111111111111 (bit).
            return (n * (n * n * 60493 + 19990303) + 1376312589) & 0x7fffffff;
        }
    }
}
