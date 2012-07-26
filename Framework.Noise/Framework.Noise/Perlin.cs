#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework.Noise
{
    /// <summary>
    /// The class generates improved Perlin noise.
    ///
    /// http://mrl.nyu.edu/~perlin/noise/
    /// </summary>
    public sealed class Perlin : IModule
    {
        const int wrapIndex = 256;

        const int modMask = 255;

        int seed = Environment.TickCount;

        Func<float, float> fadeCurve = NoiseHelper.SCurve3;

        Random random;

        int[] permutation = new int[wrapIndex * 2];

        bool initialized;

        public int Seed
        {
            get { return seed; }
            set { seed = value; }
        }

        /// <summary>
        /// Gets/sets a curve function that fades the defference between
        /// the coordinates of the input value and
        /// the coordinates of the cube's outer-lower-left vertex.
        /// 
        /// e.g.
        /// Low quality: set Noise.PassThrough()
        /// Standard quality: set Noise.SCurve3()
        /// High quality: set Noise.SCurve5()
        /// </summary>
        public Func<float, float> FadeCurve
        {
            get { return fadeCurve; }
            set { fadeCurve = value; }
        }

        public void Reseed()
        {
            random = new Random(seed);
            InitializePermutationTables();

            initialized = true;
        }

        public float Sample(float x, float y, float z)
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
            var u = fadeCurve(rx);
            var v = fadeCurve(ry);
            var w = fadeCurve(rz);

            // Hash coordinates of the 8 cube corners.
            var a = permutation[cx] + cy;
            var aa = permutation[a] + cz;
            var ab = permutation[a + 1] + cz;
            var b = permutation[cx + 1] + cy;
            var ba = permutation[b] + cz;
            var bb = permutation[b + 1] + cz;

            // Gradients of the 8 cube corners.
            var g0 = NoiseHelper.CalculateGradient(permutation[aa], rx, ry, rz);
            var g1 = NoiseHelper.CalculateGradient(permutation[ba], rx - 1, ry, rz);
            var g2 = NoiseHelper.CalculateGradient(permutation[ab], rx, ry - 1, rz);
            var g3 = NoiseHelper.CalculateGradient(permutation[bb], rx - 1, ry - 1, rz);
            var g4 = NoiseHelper.CalculateGradient(permutation[aa + 1], rx, ry, rz - 1);
            var g5 = NoiseHelper.CalculateGradient(permutation[ba + 1], rx - 1, ry, rz - 1);
            var g6 = NoiseHelper.CalculateGradient(permutation[ab + 1], rx, ry - 1, rz - 1);
            var g7 = NoiseHelper.CalculateGradient(permutation[bb + 1], rx - 1, ry - 1, rz - 1);

            // Lerp.
            var l0 = MathHelper.Lerp(g0, g1, u);
            var l1 = MathHelper.Lerp(g2, g3, u);
            var l2 = MathHelper.Lerp(g4, g5, u);
            var l3 = MathHelper.Lerp(g6, g7, u);
            var l4 = MathHelper.Lerp(l0, l1, v);
            var l5 = MathHelper.Lerp(l2, l3, v);
            return MathHelper.Lerp(l4, l5, w);
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
