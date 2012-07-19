#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace TiledTerrainDemo.Noise
{
    /// <summary>
    /// The class generates Perlin noise.
    /// </summary>
    public sealed class PerlinNoise
    {
        const int wrapIndex = 256;

        const int modMask = 255;

        const int largePower2 = 4096;

        int seed = Environment.TickCount;

        Random random;

        int[] permutation = new int[wrapIndex * 2 + 2];

        Vector3[] gradients = new Vector3[wrapIndex * 2 + 2];

        bool initialized;

        public int Seed
        {
            get { return seed; }
            set { seed = value; }
        }

        public void Reseed()
        {
            random = new Random(seed);
            InitializeLookupTables();

            initialized = true;
        }

        public float GetValue(float x, float y, float z)
        {
            if (!initialized) Reseed();

            float t;

            t = x + largePower2;
            var gridPointL = ((int) t) & modMask;
            var gridPointR = (gridPointL + 1) & modMask;
            var distanceFromL = t - (int) t;
            var distanceFromR = distanceFromL - 1;

            t = y + largePower2;
            var gridPointD = ((int) t) & modMask;
            var gridPointU = (gridPointD + 1) & modMask;
            var distanceFromD = t - (int) t;
            var distanceFromU = distanceFromD - 1;

            t = z + largePower2;
            var gridPointB = ((int) t) & modMask;
            var gridPointF = (gridPointB + 1) & modMask;
            var distanceFromB = t - (int) t;
            var distanceFromF = distanceFromB - 1;

            var indexL = permutation[gridPointL];
            var indexR = permutation[gridPointR];

            var indexLD = permutation[indexL + gridPointD];
            var indexRD = permutation[indexR + gridPointD];
            var indexLU = permutation[indexL + gridPointU];
            var indexRU = permutation[indexR + gridPointU];

            var sx = NoiseHelper.CubicSCurve(distanceFromL);
            var sy = NoiseHelper.CubicSCurve(distanceFromD);
            var sz = NoiseHelper.CubicSCurve(distanceFromB);

            Vector3 q;
            float u;
            float v;
            float a;
            float b;
            float c;
            float d;

            q = gradients[indexLD + gridPointB];
            u = Vector3.Dot(new Vector3(distanceFromL, distanceFromD, distanceFromB), q);
            q = gradients[indexRD + gridPointB];
            v = Vector3.Dot(new Vector3(distanceFromR, distanceFromD, distanceFromB), q);
            a = MathHelper.Lerp(u, v, sx);

            q = gradients[indexLU + gridPointB];
            u = Vector3.Dot(new Vector3(distanceFromL, distanceFromU, distanceFromB), q);
            q = gradients[indexRU + gridPointB];
            v = Vector3.Dot(new Vector3(distanceFromR, distanceFromU, distanceFromB), q);
            b = MathHelper.Lerp(u, v, sx);

            c = MathHelper.Lerp(a, b, sy);

            q = gradients[indexLD + gridPointF];
            u = Vector3.Dot(new Vector3(distanceFromL, distanceFromD, distanceFromF), q);
            q = gradients[indexRD + gridPointF];
            v = Vector3.Dot(new Vector3(distanceFromR, distanceFromD, distanceFromF), q);
            a = MathHelper.Lerp(u, v, sx);

            q = gradients[indexLU + gridPointF];
            u = Vector3.Dot(new Vector3(distanceFromL, distanceFromU, distanceFromF), q);
            q = gradients[indexRU + gridPointF];
            v = Vector3.Dot(new Vector3(distanceFromR, distanceFromU, distanceFromF), q);
            b = MathHelper.Lerp(u, v, sx);

            d = MathHelper.Lerp(a, b, sy);

            return MathHelper.Lerp(c, d, sz);
        }

        float GenerateGradientValue()
        {
            // a random value [-1, 1].
            return (float) ((random.Next() % (wrapIndex + wrapIndex)) - wrapIndex) / wrapIndex;
        }

        void InitializeLookupTables()
        {
            for (int i = 0; i < wrapIndex; i++)
            {
                permutation[i] = i;

                var value = new Vector3(GenerateGradientValue(), GenerateGradientValue(), GenerateGradientValue());
                value.Normalize();
                gradients[i] = value;
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
            for (int i = 0; i < wrapIndex + 2; i++)
            {
                var index = wrapIndex + i;

                permutation[index] = permutation[i];

                gradients[index] = gradients[i];
            }
        }
    }
}
