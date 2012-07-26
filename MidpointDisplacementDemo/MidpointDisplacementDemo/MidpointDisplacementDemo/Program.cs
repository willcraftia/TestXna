using System;

namespace MidpointDisplacementDemo
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリー ポイントです。
        /// </summary>
        static void Main(string[] args)
        {
            using (MidpointDisplacementDemoGame game = new MidpointDisplacementDemoGame())
            {
                game.Run();
            }
        }
    }
#endif
}

