#region Using

using System;

#endregion

namespace NoiseMeasuring
{
    /// <summary>
    /// The class generates Simplex noise.
    /// 
    /// http://staffwww.itn.liu.se/~stegu/simplexnoise/simplexnoise.pdf
    /// (including the original source code)
    /// </summary>
    public sealed class SimplexNoise
    {
        const int wrapIndex = 256;

        const int modMask = 255;

        static int[,] grad3 =
        {
            {1,1,0}, {-1,1,0}, {1,-1,0}, {-1,-1,0},
            {1,0,1}, {-1,0,1}, {1,0,-1}, {-1,0,-1},
            {0,1,1}, {0,-1,1}, {0,1,-1}, {0,-1,-1}
        };

        int seed = Environment.TickCount;

        Random random;

        int[] permutation = new int[wrapIndex * 2];

        bool initialized;

        public int Seed
        {
            get { return seed; }
            set { seed = value; }
        }

        public void Reseed()
        {
            random = new Random(seed);
            InitializePermutationTables();

            initialized = true;
        }

        public float GetValue(float x, float y, float z)
        {
            if (!initialized) Reseed();

            // Skew the input space to determine which simplex cell we're in.
            // Very nice and simple skew factor for 3D.
            const float F3 = 1.0f / 3.0f;
            float s = (x + y + z) * F3;
            int i = Floor(x + s);
            int j = Floor(y + s);
            int k = Floor(z + s);

            // Very nice and simple unskew factor, too.
            const float G3 = 1.0f / 6.0f;
            float t = (i + j + k) * G3;
            // Unskew the cell origin back to (x, y, z) space.
            float X0 = i - t;
            float Y0 = j - t;
            float Z0 = k - t;
            // The x, y, z distances from the cell origin.
            float x0 = x - X0;
            float y0 = y - Y0;
            float z0 = z - Z0;

            // For the 3D case, the simplex shape is a slightly irregular tetrahedron.
            // Determine which simplex we are in.

            // Offsets for second corner of simplex in (i, j, k) coords.
            int i1, j1, k1;
            // Offsets for third corner of simplex in (i, j, k) coords.
            int i2, j2, k2;

            if (y0 <= x0)
            {
                if (z0 <= y0)
                {
                    // X Y Z order.
                    i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 1; k2 = 0;
                }
                else if (z0 <= x0)
                {
                    // X Z Y order
                    i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 0; k2 = 1;
                }
                else
                {
                    // Z X Y order
                    i1 = 0; j1 = 0; k1 = 1; i2 = 1; j2 = 0; k2 = 1;
                }
            }
            else
            {
                if (y0 < z0)
                {
                    // Z Y X order
                    i1 = 0; j1 = 0; k1 = 1; i2 = 0; j2 = 1; k2 = 1;
                }
                else if (x0 < z0)
                {
                    // Y Z X order
                    i1 = 0; j1 = 1; k1 = 0; i2 = 0; j2 = 1; k2 = 1;
                }
                else
                {
                    // Y X Z order
                    i1 = 0; j1 = 1; k1 = 0; i2 = 1; j2 = 1; k2 = 0;
                }
            }

            // A step of (1, 0, 0) in (i, j, k) means a step of (1 - c, -c, -c) in (x, y, z),
            // a step of (0, 1, 0) in (i, j, k) means a step of (-c, 1 - c, -c) in (x, y, z), and
            // a step of (0, 0, 1) in (i, j, k) means a step of (-c, -c, 1 - c) in (x, y, z), where
            // c = 1 / 6.

            // Offsets for second corner in (x, y, z) coords.
            float x1 = x0 - i1 + G3;
            float y1 = y0 - j1 + G3;
            float z1 = z0 - k1 + G3;
            // Offsets for third corner in (x, y, z) coords.
            float x2 = x0 - i2 + 2 * G3;
            float y2 = y0 - i2 + 2 * G3;
            float z2 = z0 - k2 + 2 * G3;
            // Offsets for last corner in (x, y, z) coords.
            float x3 = x0 - 1 + 3 * G3;
            float y3 = y0 - 1 + 3 * G3;
            float z3 = z0 - 1 + 3 * G3;

            // Work out the hashed gradient indices of the four simplex corners.
            int ii = i & modMask;
            int jj = j & modMask;
            int kk = k & modMask;
            int gi0 = permutation[ii + permutation[jj + permutation[kk]]] % 12;
            int gi1 = permutation[ii + i1 + permutation[jj + j1 + permutation[kk + k1]]] % 12;
            int gi2 = permutation[ii + i2 + permutation[jj + j2 + permutation[kk + k2]]] % 12;
            int gi3 = permutation[ii + 1 + permutation[jj + 1 + permutation[kk + 1]]] % 12;

            // Noise contributions from the four corners.
            float n0, n1, n2, n3;

            // Caluculate the contribution from the four corners.
            float t0 = 0.6f - x0 * x0 - y0 * y0 - z0 * z0;
            if (t0 < 0)
            {
                n0 = 0;
            }
            else
            {
                t0 *= t0;
                n0 = t0 * t0 * Gradient3Dot(gi0, x0, y0, z0);
            }

            float t1 = 0.6f - x1 * x1 - y1 * y1 - z1 * z1;
            if (t1 < 0)
            {
                n1 = 0;
            }
            else
            {
                t1 *= t1;
                n1 = t1 * t1 * Gradient3Dot(gi1, x1, y1, z1);
            }

            float t2 = 0.6f - x2 * x2 - y2 * y2 - z2 * z2;
            if (t2 < 0)
            {
                n2 = 0;
            }
            else
            {
                t2 *= t2;
                n2 = t2 * t2 * Gradient3Dot(gi2, x2, y2, z2);
            }

            float t3 = 0.6f - x3 * x3 - y3 * y3 - z3 * z3;
            if (t3 < 0)
            {
                n3 = 0;
            }
            else
            {
                t3 *= t3;
                n3 = t3 * t3 * Gradient3Dot(gi3, x3, y3, z3);
            }
            // Add contributions from each corner to get the final noise value.
            // The result is scaled to stay just inside [-1,1].
            return 32.0f * (n0 + n1 + n2 + n3);
        }

        static int Floor(float v)
        {
            // Faster than using (int) Math.Floor(x).
            return 0 < v ? (int) v : (int) v - 1;
        }

        static float Gradient3Dot(int gradIndex, float x, float y, float z)
        {
            return grad3[gradIndex, 0] * x + grad3[gradIndex, 1] * y + grad3[gradIndex, 2] * z;
        }

        void InitializePermutationTables()
        {
            for (int i = 0; i < wrapIndex; i++)
            {
                permutation[i] = i;
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
