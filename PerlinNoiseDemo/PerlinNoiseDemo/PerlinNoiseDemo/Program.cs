using System;

namespace PerlinNoiseDemo
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリー ポイントです。
        /// </summary>
        static void Main(string[] args)
        {
            using (PerlinNoiseDemoGame game = new PerlinNoiseDemoGame())
            {
                game.Run();
            }
        }
    }
#endif
}

