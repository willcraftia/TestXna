using System;

namespace TerrainDemo
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリー ポイントです。
        /// </summary>
        static void Main(string[] args)
        {
            using (TerrainDemoGame game = new TerrainDemoGame())
            {
                game.Run();
            }
        }
    }
#endif
}

