#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace TiledTerrainDemo.Noise
{
    /// <summary>
    /// The class generates improved Perlin noise.
    ///
    /// http://mrl.nyu.edu/~perlin/noise/
    /// </summary>
    public sealed class Perlin
    {
        const int wrapIndex = 256;

        const int modMask = 255;

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

            int fx = NoiseHelper.Floor(x);
            int fy = NoiseHelper.Floor(y);
            int fz = NoiseHelper.Floor(z);

            // Find unit cube that contains point.
            int cx = fx & modMask;
            int cy = fy & modMask;
            int cz = fz & modMask;

            // Find relative x, y, z of point in cube.
            var rx = x - fx;
            var ry = y - fy;
            var rz = z - fz;

            // complute fade curves for each of x, y, z.
            var u = CalculateFadeCurve(rx);
            var v = CalculateFadeCurve(ry);
            var w = CalculateFadeCurve(rz);

            // Hash coordinates of the 8 cube corners.
            var a = permutation[cx] + cy;
            var aa = permutation[a] + cz;
            var ab = permutation[a + 1] + cz;
            var b = permutation[cx + 1] + cy;
            var ba = permutation[b] + cz;
            var bb = permutation[b + 1] + cz;

            // Gradients of the 8 cube corners.
            var g0 = CalculateGradient(permutation[aa], rx, ry, rz);
            var g1 = CalculateGradient(permutation[ba], rx - 1, ry, rz);
            var g2 = CalculateGradient(permutation[ab], rx, ry - 1, rz);
            var g3 = CalculateGradient(permutation[bb], rx - 1, ry - 1, rz);
            var g4 = CalculateGradient(permutation[aa + 1], rx, ry, rz - 1);
            var g5 = CalculateGradient(permutation[ba + 1], rx - 1, ry, rz - 1);
            var g6 = CalculateGradient(permutation[ab + 1], rx, ry - 1, rz - 1);
            var g7 = CalculateGradient(permutation[bb + 1], rx - 1, ry - 1, rz - 1);

            // Lerp.
            var l0 = MathHelper.Lerp(g0, g1, u);
            var l1 = MathHelper.Lerp(g2, g3, u);
            var l2 = MathHelper.Lerp(g4, g5, u);
            var l3 = MathHelper.Lerp(g6, g7, u);
            var l4 = MathHelper.Lerp(l0, l1, v);
            var l5 = MathHelper.Lerp(l2, l3, v);
            return MathHelper.Lerp(l4, l5, w);
        }

        float CalculateFadeCurve(float t)
        {
            return t * t * t * (t * (t * 6 - 15) + 10);
        }

        float CalculateGradient(int hash, float x, float y, float z)
        {
            // convert LO 4 bits of hash code into 12 gradient directions.
            int h = hash & 15;
            float u = h < 8 ? x : y;
            float v = h < 4 ? y : h == 12 || h == 14 ? x : z;
            return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
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
