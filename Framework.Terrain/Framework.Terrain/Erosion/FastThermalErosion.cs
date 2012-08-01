#region Using

using System;
using System.Diagnostics;

#endregion

namespace Willcraftia.Xna.Framework.Terrain.Erosion
{
    public sealed class FastThermalErosion
    {
        public const float DefaultTalus = 0.05f;

        public const int DefaultIterationCount = 50;

        IMap<float> heightMap;

        float talus = DefaultTalus;

        int iterationCount = DefaultIterationCount;

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

            for (int i = 0; i < iterationCount; i++)
                Erode();
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

                    var offsetX = 0;
                    var offsetY = 0;

                    float dMax = 0;
                    if (dMax < d1)
                    {
                        dMax = d1;
                        offsetY = 1;
                    }
                    if (dMax < d2)
                    {
                        dMax = d2;
                        offsetX = -1;
                        offsetY = 0;
                    }
                    if (dMax < d3)
                    {
                        dMax = d3;
                        offsetX = 1;
                        offsetY = 0;
                    }
                    if (dMax < d4)
                    {
                        dMax = d4;
                        offsetX = 0;
                        offsetY = -1;
                    }

                    if (dMax < talus) continue;

                    dMax *= 0.5f;
                    heightMap[x, y] -= dMax;
                    heightMap[x + offsetX, y + offsetY] += dMax;
                }
            }
        }
    }
}
