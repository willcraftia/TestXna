#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace TiledTerrainDemo.Noise
{
    public sealed class SimpleVoronoi
    {
        #region Result

        struct Result
        {
            public float Distance0;
            public float Distance1;
            public float Distance2;
            public float Distance3;
        }

        #endregion

        const int wrapIndex = 256;

        const int modMask = 255;

        int seed = Environment.TickCount;

        Random random;

        int[] permutation = new int[wrapIndex * 2];

        float[] coordinates = new float[wrapIndex * 3];

        VoronoiType voronoiType = VoronoiType.First;

        MetricsDelegate metrics = Mertics.Real;

        bool initialized;

        public int Seed
        {
            get { return seed; }
            set { seed = value; }
        }

        public void Reseed()
        {
            random = new Random(seed);
            Initialize();

            initialized = true;
        }

        public VoronoiType VoronoiType
        {
            get { return voronoiType; }
            set { voronoiType = value; }
        }

        public MetricsDelegate Metrics
        {
            get { return metrics; }
            set { metrics = value; }
        }

        public float GetValue(float x, float y, float z)
        {
            return GetValue(x, y, z, voronoiType);
        }

        float GetValue(float x, float y, float z, VoronoiType type)
        {
            Result result;

            switch (type)
            {
                case VoronoiType.First:
                    Calculate(x, y, z, out result);
                    return result.Distance0;
                case VoronoiType.Second:
                    Calculate(x, y, z, out result);
                    return result.Distance1;
                case VoronoiType.Third:
                    Calculate(x, y, z, out result);
                    return result.Distance2;
                case VoronoiType.Fourth:
                    Calculate(x, y, z, out result);
                    return result.Distance3;
                case VoronoiType.Difference21:
                    Calculate(x, y, z, out result);
                    return result.Distance1 - result.Distance0;
                case VoronoiType.Difference32:
                    Calculate(x, y, z, out result);
                    return result.Distance2 - result.Distance1;
                case VoronoiType.Crackle:
                    {
                        var d = 10 * GetValue(x, y, z, VoronoiType.Difference21);
                        return (1 < d) ? 1 : d;
                    }
            }

            throw new InvalidOperationException("An unknown VoronoiType is specified.");
        }

        void Calculate(float x, float y, float z, out Result result)
        {
            if (!initialized) Reseed();

            result = new Result
            {
                Distance0 = 1e10f,
                Distance1 = 1e10f,
                Distance2 = 1e10f,
                Distance3 = 1e10f
            };

            int xi = NoiseHelper.Floor(x);
            int yi = NoiseHelper.Floor(y);
            int zi = NoiseHelper.Floor(z);

            for (int xx = xi - 1; xx <= xi + 1; xx++)
            {
                for (int yy = yi - 1; yy <= yi + 1; yy++)
                {
                    for (int zz = zi - 1; zz <= zi + 1; zz++)
                    {
                        int ci = CoordIndex(xx, yy, zz);
                        
                        var p0 = coordinates[ci];
                        var p1 = coordinates[ci + 1];
                        var p2 = coordinates[ci + 2];

                        var xd = x - (p0 + xx);
                        var yd = y - (p1 + yy);
                        var zd = z - (p2 + zz);
                        
                        var d = metrics(xd, yd, zd);
                        
                        if (d < result.Distance0)
                        {
                            result.Distance3 = result.Distance2;
                            result.Distance2 = result.Distance1;
                            result.Distance1 = result.Distance0;
                            result.Distance0 = d;
                        }
                        else if (d < result.Distance1)
                        {
                            result.Distance3 = result.Distance2;
                            result.Distance2 = result.Distance1;
                            result.Distance1 = d;
                        }
                        else if (d < result.Distance2)
                        {
                            result.Distance3 = result.Distance2;
                            result.Distance2 = d;
                        }
                        else if (d < result.Distance3)
                        {
                            result.Distance3 = d;
                        }
                    }
                }
            }
        }

        int CoordIndex(int x, int y, int z)
        {
            return 3 * permutation[(permutation[(permutation[z & modMask] + y) & modMask] + x) & modMask];
        }

        void Initialize()
        {
            for (int i = 0; i < wrapIndex; i++)
            {
                permutation[i] = i;
            }

            for (int i = 0; i < coordinates.Length; i++)
            {
                coordinates[i] = (float) random.NextDouble();
            }

            // Shuffle.
            for (int i = 0; i < wrapIndex; i++)
            {
                var j = random.Next() & modMask;
                var tmp = permutation[i];
                permutation[i] = permutation[j];
                permutation[j] = tmp;
            }

            // Clone.
            for (int i = 0; i < wrapIndex; i++)
            {
                permutation[wrapIndex + i] = permutation[i];
            }
        }
    }
}
