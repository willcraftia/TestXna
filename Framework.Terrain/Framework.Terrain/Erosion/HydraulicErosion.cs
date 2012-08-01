#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Terrain.Erosion
{
    public sealed class HydraulicErosion
    {
        public const float DefaultSolubility = 0.05f;

        public const float DefaultEvaporation = 0.9f;

        public const float DefaultSedimentCapacity = 0.05f;

        public const int DefaultIterationCount = 50;

        int width;

        int height;

        IMap<float> heightMap;

        IMap<float> rainMap;

        float solubility = DefaultSolubility;

        float evaporation = DefaultEvaporation;

        float sedimentCapacity = DefaultSedimentCapacity;

        int iterationCount = DefaultIterationCount;

        Map<float> waterMap;

        Map<float> sedimentMap;

        Map<float> deltaWaterMap;

        Map<float> deltaSedimentMap;

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

        public float SedimentCapacity
        {
            get { return sedimentCapacity; }
            set { sedimentCapacity = value; }
        }

        public int IterationCount
        {
            get { return iterationCount; }
            set { iterationCount = value; }
        }

        public HydraulicErosion(int width, int height)
        {
            this.width = width;
            this.height = height;

            waterMap = new Map<float>(width, height);
            sedimentMap = new Map<float>(width, height);
            deltaWaterMap = new Map<float>(width, height);
            deltaSedimentMap = new Map<float>(width, height);
        }

        public void Build()
        {
            waterMap.Clear();
            sedimentMap.Clear();

            for (int i = 0; i < iterationCount; i++)
                Erode();

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    heightMap[x, y] += Math.Max(0, sedimentMap[x, y]);
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
            {
                for (int x = 0; x < width; x++)
                {
                    var sediment = solubility * waterMap[x, y];
                    heightMap[x, y] -= sediment;
                    sedimentMap[x, y] += sediment;
                }
            }
        }

        void MoveWater()
        {
            deltaWaterMap.Clear();
            deltaSedimentMap.Clear();
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

                    float dTotal = 0;
                    float aTotal = 0;
                    int cellCount = 0;
                    if (0 < d1)
                    {
                        dTotal += d1;
                        aTotal += a1;
                        cellCount++;
                    }
                    if (0 < d2)
                    {
                        dTotal += d2;
                        aTotal += a2;
                        cellCount++;
                    }
                    if (0 < d3)
                    {
                        dTotal += d3;
                        aTotal += a3;
                        cellCount++;
                    }
                    if (0 < d4)
                    {
                        dTotal += d4;
                        aTotal += a4;
                        cellCount++;
                    }

                    if (cellCount == 0) continue;

                    float w = waterMap[x, y];
                    float wTotal = Math.Min(w, a - aTotal / cellCount);
                    if (wTotal <= 0) continue;

                    float unitW = wTotal / dTotal;
                    float unitM = sedimentMap[x, y] / w;

                    if (0 < d1) MoveWater(x, y, x, y + 1, d1, unitW, unitM);
                    if (0 < d2) MoveWater(x, y, x - 1, y, d2, unitW, unitM);
                    if (0 < d3) MoveWater(x, y, x + 1, y, d3, unitW, unitM);
                    if (0 < d4) MoveWater(x, y, x, y - 1, d4, unitW, unitM);
                }
            }
        }

        void MoveWater(int x, int y, int neighborX, int neighborY, float d, float unitW, float unitM)
        {
            var dw = d * unitW;
            var dm = dw * unitM;

            deltaWaterMap[x, y] -= dw;
            deltaWaterMap[neighborX, neighborY] += dw;

            deltaSedimentMap[x, y] -= dm;
            deltaSedimentMap[neighborX, neighborY] += dm;
        }

        void Evarporate()
        {
            // update water/sediment maps.
            waterMap.Add(deltaWaterMap);
            sedimentMap.Add(deltaSedimentMap);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // evarporate.
                    waterMap[x, y] *= (1 - evaporation);

                    // decide dm, the amount of the sediment depositing.
                    var mMax = sedimentCapacity * waterMap[x, y];
                    var dm = Math.Max(0, sedimentMap[x, y] - mMax);
                    sedimentMap[x, y] -= dm;
                    heightMap[x, y] += dm;
                }
            }
        }
    }
}
