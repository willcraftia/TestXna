#region Using

using System;
using System.Diagnostics;

#endregion

namespace Willcraftia.Xna.Framework.Terrain.Erosion
{
    public sealed class FastHydraulicErosion
    {
        public const float DefaultSolubility = 0.05f;

        public const float DefaultEvaporation = 0.9f;

        public const int DefaultIterationCount = 50;

        int width;

        int height;

        IMap<float> heightMap;

        IMap<float> rainMap;

        float solubility = DefaultSolubility;

        float evaporation = DefaultEvaporation;

        int iterationCount = DefaultIterationCount;

        Map<float> waterMap;

        public IMap<float> HeightMap
        {
            get { return heightMap; }
            set { heightMap = value; }
        }

        public IMap<float> RainMap
        {
            get { return rainMap; }
            set { rainMap = value; }
        }

        public float Solubility
        {
            get { return solubility; }
            set { solubility = value; }
        }

        public float Evaporation
        {
            get { return evaporation; }
            set { evaporation = value; }
        }

        public int IterationCount
        {
            get { return iterationCount; }
            set { iterationCount = value; }
        }

        public void Build()
        {
            Debug.Assert(heightMap != null);
            Debug.Assert(rainMap != null);
            Debug.Assert(heightMap.Width == rainMap.Width && heightMap.Height == rainMap.Height);
            Debug.Assert(0 <= rainMap.Min());

            width = heightMap.Width;
            height = heightMap.Height;

            InitializeWorkingMap();

            for (int i = 0; i < iterationCount; i++)
                Erode();

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    heightMap[x, y] += waterMap[x, y] * solubility;
        }

        void InitializeWorkingMap()
        {
            if (waterMap == null || waterMap.Width != width || waterMap.Height != height)
            {
                waterMap = new Map<float>(width, height);
            }
            else
            {
                waterMap.Clear();
            }
        }

        void Erode()
        {
            // step 1
            Rain();
            // step 2
            ConvertToSediment();
            // step 3
            MoveWater();
            // step 4
            Evarporate();
        }

        void Rain()
        {
            waterMap.Add(rainMap);
        }

        void ConvertToSediment()
        {
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    heightMap[x, y] -= solubility * waterMap[x, y];
        }

        void MoveWater()
        {
            for (int y = 1; y < height - 2; y++)
            {
                for (int x = 1; x < width - 2; x++)
                {
                    var a = heightMap[x, y] + waterMap[x, y];
                    var a1 = heightMap[x, y + 1] + waterMap[x, y + 1];
                    var a2 = heightMap[x - 1, y] + waterMap[x - 1, y];
                    var a3 = heightMap[x + 1, y] + waterMap[x + 1, y];
                    var a4 = heightMap[x, y - 1] + waterMap[x, y - 1];

                    var d1 = a - a1;
                    var d2 = a - a2;
                    var d3 = a - a3;
                    var d4 = a - a4;

                    float dMax = 0;
                    int offsetX = 0;
                    int offsetY = 0;
                    if (dMax < d1)
                    {
                        dMax = d1;
                        offsetX = 0;
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

                    if (dMax <= 0) continue;

                    if (waterMap[x, y] < dMax)
                    {
                        waterMap[x + offsetX, y + offsetY] += waterMap[x, y];
                        waterMap[x, y] = 0;
                    }
                    else
                    {
                        waterMap[x + offsetX, y + offsetY] += dMax * 0.5f;
                        waterMap[x, y] = dMax * 0.5f;
                    }
                }
            }
        }

        void Evarporate()
        {
            var sedimentCapacity = solubility;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var waterLost = waterMap[x, y] * evaporation;

                    waterMap[x, y] -= waterLost;
                    heightMap[x, y] += waterLost * sedimentCapacity;
                }
            }
        }
    }
}
