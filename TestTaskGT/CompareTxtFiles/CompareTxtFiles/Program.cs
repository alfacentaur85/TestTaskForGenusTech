using System;
// NLog
using NLog;

namespace CompareTxtFiles
{
    class Program
    {
        static void Main(string[] args)
        {
            
            CompareClass compare = new CompareClass(args);
            compare.StartComparing();

        }
    }
}
