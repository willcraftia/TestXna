#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Terrain.Erosion
{
    public sealed class HydraulicErosion
    {
        int width;

        int height;

        IMap<float> heightMap;

        IMap<float> rainMap;

        float solubility = 0.01f;

        float evaporation = 0.5f;

        float sedimentCapacity = 0.01f;

        int iterationCount = 10;

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

            for (int i = 0; i < iterationCount; i++)
                Erode();
        }

        void Erode()
        {
            // step 1
            waterMap.Add(rainMap);

            // step 2
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var sediment = solubility * waterMap[x, y];
                    heightMap[x, y] -= sediment;
                    sedimentMap[x, y] += sediment;
                }
            }

            // step 3
            deltaWaterMap.Clear();
            deltaSedimentMap.Clear();
            for (int y = 1; y < height - 2; y++)
            {
                for (int x = 1; x < width - 2; x++)
                {
                    float w = waterMap[x, y];
                    if (w <= 0) continue;

                    //var a = heightMap[x, y] + waterMap[x, y];
                    //var a1 = heightMap[x, y + 1] + waterMap[x, y + 1];
                    //var a2 = heightMap[x - 1, y] + waterMap[x - 1, y];
                    //var a3 = heightMap[x + 1, y] + waterMap[x + 1, y];
                    //var a4 = heightMap[x, y - 1] + waterMap[x, y - 1];
                    var a = heightMap[x, y] + waterMap[x, y] + sedimentMap[x, y];
                    var a1 = heightMap[x, y + 1] + waterMap[x, y + 1] + sedimentMap[x, y + 1];
                    var a2 = heightMap[x - 1, y] + waterMap[x - 1, y] + sedimentMap[x - 1, y];
                    var a3 = heightMap[x + 1, y] + waterMap[x + 1, y] + sedimentMap[x + 1, y];
                    var a4 = heightMap[x, y - 1] + waterMap[x, y - 1] + sedimentMap[x, y - 1];

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

                    float aAverage = (cellCount != 0) ? aTotal / cellCount : 0;
                    float wTotal = (cellCount != 0) ? Math.Min(w, a - aAverage) : 0;
                    if (wTotal <= 0) continue;

                    deltaWaterMap[x, y] -= wTotal;

                    float m = sedimentMap[x, y];

                    float mTotal = 0;
                    if (0 < d1)
                    {
                        var dw = wTotal * d1 / dTotal;
                        deltaWaterMap[x, y + 1] += dw;
                        var dm = m * dw / w;
                        mTotal += dm;
                        deltaSedimentMap[x, y + 1] += dm;
                    }
                    if (0 < d2)
                    {
                        var dw = wTotal * d2 / dTotal;
                        deltaWaterMap[x - 1, y] += dw;
                        var dm = m * dw / w;
                        mTotal += dm;
                        deltaSedimentMap[x - 1, y] += dm;
                    }
                    if (0 < d3)
                    {
                        var dw = wTotal * d3 / dTotal;
                        deltaWaterMap[x + 1, y] += dw;
                        var dm = m * dw / w;
                        mTotal += dm;
                        deltaSedimentMap[x + 1, y] += dm;
                    }
                    if (0 < d4)
                    {
                        var dw = wTotal * d4 / dTotal;
                        deltaWaterMap[x, y - 1] += dw;
                        var dm = m * dw / w;
                        mTotal += dm;
                        deltaSedimentMap[x, y - 1] += dm;
                    }

                    deltaSedimentMap[x, y] -= mTotal;
                }
            }

            // step 4
            waterMap.Add(deltaWaterMap);
            sedimentMap.Add(deltaSedimentMap);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    waterMap[x, y] *= (1 - evaporation);

                    var mMax = sedimentCapacity * waterMap[x, y];
                    var deltaM = Math.Max(0, sedimentMap[x, y] - mMax);
                    sedimentMap[x, y] -= deltaM;
                    heightMap[x, y] += deltaM;
                }
            }
        }
    }
}
