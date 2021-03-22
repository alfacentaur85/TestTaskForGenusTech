using System;
using CompareTxtFilesLib;

namespace TestCompareTxtFilesLib
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
