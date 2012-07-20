#region Using

using System;

#endregion

namespace TiledTerrainDemo.Noise
{
    public sealed class Voronoi
    {
        public const float DefaultDisplacement = 1;

        public const float DefaultFrequency = 1;

        int seed = Environment.TickCount;

        float displacement = DefaultDisplacement;

        float frequency = DefaultFrequency;

        bool distanceEnabled;

        public int Seed
        {
            get { return seed; }
            set { seed = value; }
        }

        public float Displacement
        {
            get { return displacement; }
            set { displacement = value; }
        }

        public float Frequency
        {
            get { return frequency; }
            set { frequency = value; }
        }

        public bool DistanceEnabled
        {
            get { return distanceEnabled; }
            set { distanceEnabled = value; }
        }

        public float GetValue(float x, float y, float z)
        {
            x *= frequency;
            y *= frequency;
            z *= frequency;

            int xi = NoiseHelper.Floor(x);
            int yi = NoiseHelper.Floor(y);
            int zi = NoiseHelper.Floor(z);

            float minD = float.MaxValue;
            float xc = 0;
            float yc = 0;
            float zc = 0;

            // Inside each unit cube, there is a seed point at a random position.
            // Go through each of the nearby cubes until we find a cube with a seed point
            // that is closest to the specified position.
            for (int zz = zi - 2; zz <= zi + 2; zz++)
            {
                for (int yy = yi - 2; yy <= yi + 2; yy++)
                {
                    for (int xx = xi - 2; xx <= xi + 2; xx++)
                    {
                        // Calculate the position and distance to the seed point
                        // inside of this unit cube.
                        float xp = xx + GetValueNoise(xx, yy, zz, seed);
                        float yp = yy + GetValueNoise(xx, yy, zz, seed + 1);
                        float zp = zz + GetValueNoise(xx, yy, zz, seed + 2);
                        float xd = xp - x;
                        float yd = yp - y;
                        float zd = zp - z;
                        float d = xd * xd + yd * yd + zd * zd;

                        if (d < minD)
                        {
                            minD = d;
                            xc = xp;
                            yc = yp;
                            zc = zp;
                        }
                    }
                }
            }

            float value = 0;
            if (distanceEnabled)
                value = (float) (Math.Sqrt(minD) * Math.Sqrt(3) - 1.0f);

            int xci = NoiseHelper.Floor(xc);
            int yci = NoiseHelper.Floor(yc);
            int zci = NoiseHelper.Floor(zc);
            return value + displacement * GetValueNoise(xci, yci, zci, 0);
        }

        float GetValueNoise(int x, int y, int z, int seed)
        {
            // 1073741824 = 1000000000000000000000000000000 (bit)
            return 1.0f - ((float) GetIntValueNoise(x, y, z, seed)) / 1073741824.0f;
        }

        int GetIntValueNoise(int x, int y, int z, int seed)
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
