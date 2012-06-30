#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace PerlinNoiseDemo
{
    public sealed class ImprovedPerlinNoise
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
            InitializeLookupTables();

            initialized = true;
        }

        public float Noise3(float x, float y, float z)
        {
            if (!initialized) Reseed();

            // 後続の計算で使用する floor の事前計算。
            float fx = (float) Math.Floor(x);
            float fy = (float) Math.Floor(y);
            float fz = (float) Math.Floor(z);

            // キューブの座標。
            int cx = (int) fx & modMask;
            int cy = (int) fy & modMask;
            int cz = (int) fz & modMask;

            // キューブ内での x,y,z の相対座標。
            var rx = x - fx;
            var ry = y - fy;
            var rz = z - fz;

            // x,y,z についてのフェード カーブの値。
            var u = CalculateFadeCurve(rx);
            var v = CalculateFadeCurve(ry);
            var w = CalculateFadeCurve(rz);

            // キューブの 8 点に対するハッシュ値を決定するための下地。
            var a = permutation[cx] + cy;
            var aa = permutation[a] + cz;
            var ab = permutation[a + 1] + cz;
            var b = permutation[cx + 1] + cy;
            var ba = permutation[b] + cz;
            var bb = permutation[b + 1] + cz;

            // キューブの 8 点に対する gradient。
            var g0 = CalculateGradient(permutation[aa], rx, ry, rz);
            var g1 = CalculateGradient(permutation[ba], rx - 1, ry, rz);
            var g2 = CalculateGradient(permutation[ab], rx, ry - 1, rz);
            var g3 = CalculateGradient(permutation[bb], rx - 1, ry - 1, rz);
            var g4 = CalculateGradient(permutation[aa + 1], rx, ry, rz - 1);
            var g5 = CalculateGradient(permutation[ba + 1], rx - 1, ry, rz - 1);
            var g6 = CalculateGradient(permutation[ab + 1], rx, ry - 1, rz - 1);
            var g7 = CalculateGradient(permutation[bb + 1], rx - 1, ry - 1, rz - 1);

            // 補間。
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
            int h = hash & 15;
            float u = h < 8 ? x : y;
            float v = h < 4 ? y : h == 12 || h == 14 ? x : z;
            return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
        }

        void InitializeLookupTables()
        {
            for (int i = 0; i < wrapIndex; i++)
            {
                permutation[i] = i;
            }

            // permutation をシャッフル。
            for (int i = 0; i < wrapIndex; i++)
            {
                var j = random.Next() & modMask;
                var tmp = permutation[i];
                permutation[i] = permutation[j];
                permutation[j] = tmp;
            }

            // 配列の残り半分に値を複製。
            for (int i = 0; i < wrapIndex; i++)
            {
                permutation[wrapIndex + i] = permutation[i];
            }
        }
    }
}
