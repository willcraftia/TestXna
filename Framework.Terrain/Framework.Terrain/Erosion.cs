#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Terrain
{
    public static class Erosion
    {
        public static void Erode(IMap map, float talus, int iterations)
        {
            for (int i = 0; i < iterations; i++)
            {
                for (int y = 2; y < map.Height - 4; y++)
                {
                    for (int x = 2; x < map.Width - 4; x++)
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

                        var a = 0;
                        var b = 0;

                        float maxD = 0;
                        if (maxD < d1)
                        {
                            maxD = d1;
                            b = 1;
                        }
                        if (maxD < d2)
                        {
                            maxD = d2;
                            a = -1;
                            b = 0;
                        }
                        if (maxD < d3)
                        {
                            maxD = d3;
                            a = 1;
                            b = 0;
                        }
                        if (maxD < d4)
                        {
                            maxD = d4;
                            a = 0;
                            b = -1;
                        }

                        if (talus < maxD) continue;

                        maxD *= 0.5f;
                        map[x, y] -= maxD;
                        map[x + a, y + b] += maxD;
                    }
                }
            }
        }
    }
}
