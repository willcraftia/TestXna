#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Terrain
{
    public static class Erosion
    {
        public static void ErodeThermal(IMap<float> map, float talus, int iterations)
        {
            for (int iteration = 0; iteration < iterations; iteration++)
            {
                for (int y = 1; y < map.Height - 2; y++)
                {
                    for (int x = 1; x < map.Width - 2; x++)
                    {
                        var h = map[x, y];
                        var h1 = map[x, y + 1];
                        var h2 = map[x - 1, y];
                        var h3 = map[x + 1, y];
                        var h4 = map[x, y - 1];

                        var d1 = h - h1;
                        var d2 = h - h2;
                        var d3 = h - h3;
                        var d4 = h - h4;

                        var i = 0;
                        var j = 0;

                        float maxD = 0;
                        if (maxD < d1)
                        {
                            maxD = d1;
                            j = 1;
                        }
                        if (maxD < d2)
                        {
                            maxD = d2;
                            i = -1;
                            j = 0;
                        }
                        if (maxD < d3)
                        {
                            maxD = d3;
                            i = 1;
                            j = 0;
                        }
                        if (maxD < d4)
                        {
                            maxD = d4;
                            i = 0;
                            j = -1;
                        }

                        if (maxD < talus) continue;

                        maxD *= 0.5f;
                        map[x, y] -= maxD;
                        map[x + i, y + j] += maxD;
                    }
                }
            }
        }
    }
}
