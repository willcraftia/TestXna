#region Using

using System;
using Willcraftia.Framework.Noise;

#endregion

namespace PerlinNoiseDemo
{
    public sealed class MidpointDisplacement
    {
        public int Seed = 300;

        public int Width;

        public int Height;

        public float[] Values;

        public void Build()
        {
            // width & height such as (256 + 1)x(256 + 1)

            Values = new float[Width * Height];

            int size = ((Width < Height) ? Width : Height) - 1;
            int offset = size / 2;

            Values[   0 +    0 * Width] = GetPosition(   0, 0,    0, Seed);
            Values[size +    0 * Width] = GetPosition(size, 0,    0, Seed);
            Values[   0 + size * Width] = GetPosition(   0, 0, size, Seed);
            Values[size + size * Width] = GetPosition(size, 0, size, Seed);

            float modifier = 0.7f;
            float range = 1;

            while (0 < offset)
            {
                bool oddY = false;

                for (int y = 0; y < Height; y += offset, oddY = !oddY)
                {
                    bool oddX = false;

                    for (int x = 0; x < Width; x += offset, oddX = !oddX)
                    {
                        if (!oddX && !oddY) continue;

                        int x0 = x - offset;
                        int x1 = x + offset;
                        int y0 = y - offset;
                        int y1 = y + offset;

                        float value;

                        if (oddX && oddY)
                        {
                            float v0 = Values[x0 + y0 * Width];
                            float v1 = Values[x1 + y0 * Width];
                            float v2 = Values[x0 + y1 * Width];
                            float v3 = Values[x1 + y1 * Width];
                            value = (v0 + v1 + v2 + v3) * 0.25f;
                        }
                        else
                        {
                            if (oddX)
                            {
                                float v0 = Values[x0 + y * Width];
                                float v1 = Values[x1 + y * Width];
                                value = (v0 + v1) * 0.5f;
                            }
                            else
                            {
                                float v0 = Values[x + y0 * Width];
                                float v1 = Values[x + y1 * Width];
                                value = (v0 + v1) * 0.5f;
                            }
                        }

                        // Add a small error.
                        value += GetPosition(x, y, x, Seed) * range;

                        // Set the value.
                        Values[x + y * Width] = value;
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
