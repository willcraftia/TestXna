#region Using

using System;
using System.Diagnostics;

#endregion

namespace FloorMeasuring
{
    class Program
    {
        static void Main(string[] args)
        {
            var stopwatch = new Stopwatch();

            int loopCount = 100000;

            Console.WriteLine("Loop = {0}", loopCount);

            Console.WriteLine("[Math.Floor]", loopCount);

            stopwatch.Start();
            for (int i = 0; i < loopCount; i++)
            {
                var v = (int) Math.Floor(0d);
            }
            stopwatch.Stop();

            Console.WriteLine("Elapsed = {0}", stopwatch.Elapsed);

            stopwatch.Reset();

            Console.WriteLine("[Custom Floor]", loopCount);

            stopwatch.Start();
            for (int i = 0; i < loopCount; i++)
            {
                var v = DemoHelper.Floor(0f);
            }
            stopwatch.Stop();

            Console.WriteLine("Elapsed = {0}", stopwatch.Elapsed);


            Console.WriteLine("Press any key to exit.");
            Console.ReadLine();
        }

        static int CustomFloor(double v)
        {
            return 0 < v ? (int) v : (int) v - 1;
        }
    }
}
