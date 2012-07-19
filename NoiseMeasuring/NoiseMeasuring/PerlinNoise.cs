#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace NoiseMeasuring
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

        float[] gradients1 = new float[wrapIndex * 2 + 2];

        Vector2[] gradients2 = new Vector2[wrapIndex * 2 + 2];

        Vector3[] gradients3 = new Vector3[wrapIndex * 2 + 2];

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

            q = gradients3[indexLD + gridPointB];
            u = Vector3.Dot(new Vector3(distanceFromL, distanceFromD, distanceFromB), q);
            q = gradients3[indexRD + gridPointB];
            v = Vector3.Dot(new Vector3(distanceFromR, distanceFromD, distanceFromB), q);
            a = MathHelper.Lerp(u, v, sx);

            q = gradients3[indexLU + gridPointB];
            u = Vector3.Dot(new Vector3(distanceFromL, distanceFromU, distanceFromB), q);
            q = gradients3[indexRU + gridPointB];
            v = Vector3.Dot(new Vector3(distanceFromR, distanceFromU, distanceFromB), q);
            b = MathHelper.Lerp(u, v, sx);

            c = MathHelper.Lerp(a, b, sy);

            q = gradients3[indexLD + gridPointF];
            u = Vector3.Dot(new Vector3(distanceFromL, distanceFromD, distanceFromF), q);
            q = gradients3[indexRD + gridPointF];
            v = Vector3.Dot(new Vector3(distanceFromR, distanceFromD, distanceFromF), q);
            a = MathHelper.Lerp(u, v, sx);

            q = gradients3[indexLU + gridPointF];
            u = Vector3.Dot(new Vector3(distanceFromL, distanceFromU, distanceFromF), q);
            q = gradients3[indexRU + gridPointF];
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

                gradients1[i] = GenerateGradientValue();

                var value2d = new Vector2(GenerateGradientValue(), GenerateGradientValue());
                value2d.Normalize();
                gradients2[i] = value2d;

                var value3d = new Vector3(GenerateGradientValue(), GenerateGradientValue(), GenerateGradientValue());
                value3d.Normalize();
                gradients3[i] = value3d;
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

                gradients1[index] = gradients1[i];
                gradients2[index] = gradients2[i];
                gradients3[index] = gradients3[i];
            }
        }
    }
}
