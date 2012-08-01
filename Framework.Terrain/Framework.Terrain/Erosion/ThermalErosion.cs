#region Using

using System;
using System.Diagnostics;

#endregion

namespace Willcraftia.Xna.Framework.Terrain.Erosion
{
    public sealed class ThermalErosion
    {
        public const float DefaultTalus = 0.05f;

        public const int DefaultIterationCount = 50;

        IMap<float> heightMap;

        float talus = DefaultTalus;

        int iterationCount = DefaultIterationCount;

        int width;

        int height;

        Map<float> differenceMap;

        public IMap<float> HeightMap
        {
            get { return heightMap; }
            set { heightMap = value; }
        }

        public float Talus
        {
            get { return talus; }
            set { talus = value; }
        }

        public int IterationCount
        {
            get { return iterationCount; }
            set { iterationCount = value; }
        }

        public void Build()
        {
            Debug.Assert(heightMap != null);

            width = heightMap.Width;
            height = heightMap.Height;

            InitializeWorkingMap();

            for (int i = 0; i < iterationCount; i++)
                Erode();
        }

        void InitializeWorkingMap()
        {
            if (differenceMap == null || differenceMap.Width != width || differenceMap.Height != height)
            {
                differenceMap = new Map<float>(width, height);
            }
            else
            {
                differenceMap.Clear();
            }
        }

        void Erode()
        {
            for (int y = 1; y < heightMap.Height - 2; y++)
            {
                for (int x = 1; x < heightMap.Width - 2; x++)
                {
                    var h = heightMap[x, y];
                    var h1 = heightMap[x, y + 1];
                    var h2 = heightMap[x - 1, y];
                    var h3 = heightMap[x + 1, y];
                    var h4 = heightMap[x, y - 1];

                    var d1 = h - h1;
                    var d2 = h - h2;
                    var d3 = h - h3;
                    var d4 = h - h4;

                    float dMax = 0;
                    float dTotal = 0;
                    if (0 < d1)
                    {
                        dTotal += d1;
                        if (dMax < d1) dMax = d1;
                    }
                    if (0 < d2)
                    {
                        dTotal += d2;
                        if (dMax < d2) dMax = d2;
                    }
                    if (0 < d3)
                    {
                        dTotal += d3;
                        if (dMax < d3) dMax = d3;
                    }
                    if (0 < d4)
                    {
                        dTotal += d4;
                        if (dMax < d4) dMax = d4;
                    }

                    if (dTotal == 0) continue;
                    if (dMax < talus) continue;

                    dMax *= 0.5f;
                    var unitD = dMax / dTotal;
                    differenceMap[x, y] -= dMax;
                    if (0 < d1) differenceMap[x, y + 1] += d1 * unitD;
                    if (0 < d2) differenceMap[x - 1, y] += d2 * unitD;
                    if (0 < d3) differenceMap[x + 1, y] += d3 * unitD;
                    if (0 < d4) differenceMap[x, y - 1] += d4 * unitD;
                }
            }

            heightMap.Add(differenceMap);
            differenceMap.Clear();
        }
    }
}
