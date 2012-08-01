#region Using

using System;
using System.Diagnostics;

#endregion

namespace Willcraftia.Xna.Framework.Terrain.Erosion
{
    public sealed class MusgraveHydraulicErosion
    {
        public const float DefaultSedimentCapacity = 0.1f;

        public const float DefaultDeposition = 0.1f;

        public const float DefaultSoilSoftness = 0.3f;

        public const int DefaultIterationCount = 50;

        int width;

        int height;

        IMap<float> heightMap;

        IMap<float> rainMap;

        float sedimentCapacity = DefaultSedimentCapacity;

        float deposition = DefaultDeposition;

        float soilSoftness = DefaultSoilSoftness;

        int iterationCount = DefaultIterationCount;

        Map<float> waterMap;

        Map<float> sedimentMap;

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

        public float SedimentCapacity
        {
            get { return sedimentCapacity; }
            set { sedimentCapacity = value; }
        }

        public float Deposition
        {
            get { return deposition; }
            set { deposition = value; }
        }

        public float SoilSoftness
        {
            get { return soilSoftness; }
            set { soilSoftness = value; }
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

            InitializeWorkingMaps();

            for (int i = 0; i < iterationCount; i++)
                Erode();
        }

        void InitializeWorkingMaps()
        {
            if (waterMap == null || waterMap.Width != width || waterMap.Height != height)
            {
                waterMap = new Map<float>(width, height);
                sedimentMap = new Map<float>(width, height);
            }
            else
            {
                waterMap.Clear();
                sedimentMap.Clear();
            }
        }

        void Erode()
        {
            waterMap.Add(rainMap);

            for (int y = 1; y < height - 2; y++)
            {
                for (int x = 1; x < width - 2; x++)
                {
                    // altitude.
                    var a = heightMap[x, y] + waterMap[x, y];
                    var a1 = heightMap[x, y + 1] + waterMap[x, y + 1];
                    var a2 = heightMap[x - 1, y] + waterMap[x - 1, y];
                    var a3 = heightMap[x + 1, y] + waterMap[x + 1, y];
                    var a4 = heightMap[x, y - 1] + waterMap[x, y - 1];

                    // difference.
                    var d1 = a - a1;
                    var d2 = a - a2;
                    var d3 = a - a3;
                    var d4 = a - a4;

                    float totalDiff = 0;
                    float totalAltitude = 0;
                    float cellCount = 0;
                    if (0 < d1)
                    {
                        totalDiff += d1;
                        totalAltitude += a1;
                        cellCount++;
                    }
                    if (0 < d2)
                    {
                        totalDiff += d2;
                        totalAltitude += a2;
                        cellCount++;
                    }
                    if (0 < d3)
                    {
                        totalDiff += d3;
                        totalAltitude += a3;
                        cellCount++;
                    }
                    if (0 < d4)
                    {
                        totalDiff += d4;
                        totalAltitude += a4;
                        cellCount++;
                    }

                    if (cellCount == 0) continue;

                    var average = totalAltitude / cellCount;
                    float targetWater = Math.Min(waterMap[x, y], a - average);
                    float unitWater = targetWater / totalDiff;
                    float unitSediment = sedimentMap[x, y] / totalDiff;

                    // move the water and sediment.
                    if (0 < d1) MoveWater(x, y, x, y + 1, d1 * unitWater, d1 * unitSediment);
                    if (0 < d2) MoveWater(x, y, x - 1, y, d2 * unitWater, d2 * unitSediment);
                    if (0 < d3) MoveWater(x, y, x + 1, y, d3 * unitWater, d3 * unitSediment);
                    if (0 < d4) MoveWater(x, y, x, y - 1, d4 * unitWater, d4 * unitSediment);

                    if (sedimentMap[x, y] < 0) sedimentMap[x, y] = 0;
                }
            }
        }

        void MoveWater(int x, int y, int neighborX, int neighborY, float dw, float s)
        {
            if (dw <= 0)
            {
                var depo = deposition * s;
                heightMap[x, y] += depo;
                sedimentMap[x, y] -= depo;
            }
            else
            {
                // move water.
                waterMap[x, y] -= dw;
                waterMap[neighborX, neighborY] += dw;

                // calculate the sediment capacity of dw.
                var c = dw * sedimentCapacity;

                if (c <= s)
                {
                    var depo = deposition * (s - c);
                    sedimentMap[neighborX, neighborY] += c;
                    heightMap[x, y] += depo;
                    sedimentMap[x, y] -= c + depo;
                }
                else
                {
                    var soil = soilSoftness * (c - s);
                    sedimentMap[neighborX, neighborY] += s + soil;
                    heightMap[x, y] -= soil;
                    sedimentMap[x, y] -= s;
                }
            }
        }
    }
}
