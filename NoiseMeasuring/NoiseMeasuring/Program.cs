#region Using

using System;
using System.Diagnostics;

#endregion

namespace NoiseMeasuring
{
    class Program
    {
        static void Main(string[] args)
        {
            const int loopCount = 512 * 512 * 6 * 3;
            var stopwatch = new Stopwatch();

            var perlin = new PerlinNoise();
            perlin.Reseed();

            var improvedPerlin = new ImprovedPerlinNoise();
            improvedPerlin.Reseed();

            var simplex = new SimplexNoise();
            simplex.Reseed();

            Console.WriteLine("Loop = {0}", loopCount);

            Console.WriteLine("[Perlin noise]");

            stopwatch.Start();
            for (int i = 0; i < loopCount; i++)
            {
                perlin.GetValue(0, 0, 0);
            }
            stopwatch.Stop();

            Console.WriteLine("Elapsed = {0}", stopwatch.Elapsed);

            stopwatch.Reset();

            Console.WriteLine("[Improved Perlin noise]");

            stopwatch.Start();
            for (int i = 0; i < loopCount; i++)
            {
                improvedPerlin.GetValue(0, 0, 0);
            }
            stopwatch.Stop();

            Console.WriteLine("Elapsed = {0}", stopwatch.Elapsed);

            stopwatch.Reset();

            Console.WriteLine("[Simplex noise]");

            stopwatch.Start();
            for (int i = 0; i < loopCount; i++)
            {
                simplex.GetValue(0, 0, 0);
            }
            stopwatch.Stop();

            Console.WriteLine("Elapsed = {0}", stopwatch.Elapsed);


            Console.WriteLine("Press any key to exit.");
            Console.ReadLine();
        }
    }
}
