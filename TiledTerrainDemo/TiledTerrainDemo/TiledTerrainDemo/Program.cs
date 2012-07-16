using System;

namespace TiledTerrainDemo
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリー ポイントです。
        /// </summary>
        static void Main(string[] args)
        {
            using (TiledTerrainDemoGame game = new TiledTerrainDemoGame())
            {
                game.Run();
            }
        }
    }
#endif
}

