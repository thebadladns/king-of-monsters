using System;

namespace kom
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (KoM game = new KoM())
            {
                game.Run();
            }
        }
    }
#endif
}

