using System;
// NLog
using NLog;

namespace CompareTxtFiles
{

    class Program
    {
        //NLog class instances
        private static Logger logger = LogManager.GetCurrentClassLogger();
        static void Main(string[] args)
        {
            try
            {
                CompareClass compare = new CompareClass(args);

                compare.StartComparing();
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR: {e.Message}");

                logger.Error($"ERROR: {e.Message}");

            }
                   
        }
    }
}
