using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
//third-party lib
using DiffPlex.DiffBuilder.Model;
// NLog
using NLog;


namespace CompareTxtFiles
{
    class CompareClass
    {
        enum MeansOfInputArgs
        {
            ORIGINAL_FILE,
            MODIFIED_FILE
        }

        //NLog class instances
        private static Logger logger = LogManager.GetCurrentClassLogger();

        //third-party class instances
        DiffPlex.Differ differ = new DiffPlex.Differ();
        DiffPlex.Model.DiffResult differResult;

        StreamReader[] _fileStreams = new StreamReader[Enum.GetValues(typeof(MeansOfInputArgs)).Length];

        List<string> _inputArgValues = new List<string>();

        List<string>[] _differences = new List<string>[2] { new List<string>(), new List<string>() };

        //constructor
        public CompareClass(string[] args)
        {
            //check correctness ars and initial of list of input filenames

            //Check count of input args
            if (args.Length >= Enum.GetValues(typeof(MeansOfInputArgs)).Length)
            {
                foreach (string fileName in args)
                {
                    //Check validly extension
                    if (Path.GetExtension(fileName).ToUpper() == ".TXT")
                    {
                        _inputArgValues.Add(fileName);
                    }
                    else
                    {
                        logger.Error($"ERROR: FILE {fileName} IS NOT TXT.  APPLICATION WILL BE TERMINATED");

                        Console.WriteLine($"ERROR: FILE {fileName} IS NOT TXT.  APPLICATION WILL BE TERMINATED");

                        TerminateApp();
                    }
                }
            }
            else
            {
                logger.Error("ERROR: NOT ENOUGHTH INPUT ARGUMENTS.  APPLICATION WILL BE TERMINATED");

                Console.WriteLine("ERROR: NOT ENOUGHTH INPUT ARGUMENTS.  APPLICATION WILL BE TERMINATED");

                TerminateApp();
            }
        }

        void TerminateApp()
        {
            Environment.Exit(-1);
        }

        //Compare By ThirdParty Lib and write to list of defferences
        void ComparingByThirdPartyLib()
        {
            //return to begining of input files
            _fileStreams[(int)MeansOfInputArgs.ORIGINAL_FILE].BaseStream.Position = 0;

            _fileStreams[(int)MeansOfInputArgs.MODIFIED_FILE].BaseStream.Position = 0;

            //
            differResult = differ.CreateLineDiffs(_fileStreams[(int)MeansOfInputArgs.ORIGINAL_FILE].ReadToEnd(),

                  _fileStreams[(int)MeansOfInputArgs.MODIFIED_FILE].ReadToEnd(), false);

            int countDifBlocks = differResult.DiffBlocks.Count;

            for (int i = 0; i < countDifBlocks; i++)
            {
                if (differResult.DiffBlocks[i].DeleteCountA == differResult.DiffBlocks[i].InsertCountB)
                {
                    _differences[1].Add(string.Concat(differResult.DiffBlocks[i].DeleteStartA + 1, " : <modified line value for line ", differResult.DiffBlocks[i].DeleteStartA + 1, ">"));
                }
                else if (differResult.DiffBlocks[i].DeleteCountA > differResult.DiffBlocks[i].InsertCountB)
                {
                    _differences[1].Add(string.Concat("<deleted line(s) from ",

                    differResult.DiffBlocks[i].DeleteStartA + 1, " to ",

                    differResult.DiffBlocks[i].DeleteStartA + differResult.DiffBlocks[i].DeleteCountA, ">"));
                }
                else if (differResult.DiffBlocks[i].DeleteCountA < differResult.DiffBlocks[i].InsertCountB)
                {
                    _differences[1].Add(string.Concat("<added line(s) from ",

                        differResult.DiffBlocks[i].InsertStartB + 1, " to ",

                        differResult.DiffBlocks[i].InsertStartB + differResult.DiffBlocks[i].InsertCountB, ">"));
                }

            }

        }
               
        //Start comparing
        public void StartComparing()
        {
            logger.Debug(string.Concat("STARTING APPLICATION AT ", DateTime.Now));
            OpenInputFilesAndCompare();
            OutputResult("RESULT OF COMPARING BY CUSTOM:", _differences[0]);
            Console.WriteLine();
            OutputResult("RESULT OF COMPARING BY THIRD-PARTY LIB:", _differences[1]);
        }

        //Comparing files by lines and write to list of defferences
        void Comparing()
        {
            var curLine = 1;
            string sOrig, sMod;
            while (
                    ((sOrig = _fileStreams[(int)MeansOfInputArgs.ORIGINAL_FILE].ReadLine()) != null) &&
                    ((sMod = _fileStreams[(int)MeansOfInputArgs.MODIFIED_FILE].ReadLine()) != null)
                    )
            {
                if (String.Compare(sOrig, sMod) != 0)
                {
                    _differences[0].Add(string.Concat(curLine, " : <modified line value for line ", curLine, ">"));
                }
                curLine++;
            }

        }

        //Output result of comparing
        void OutputResult(string head, List<string> differences)
        {
            Console.WriteLine(head);

            if (differences.Count > 0)
            {
                logger.Info(head);

                foreach (string str in differences)
                {
                    logger.Info(str);

                    Console.WriteLine(str);
                }
            }
            else
            {
                logger.Info("NO DIFFERENCES BETWEEN INPUT FILES");

                Console.WriteLine("NO DIFFERENCES BETWEEN INPUT FILES");
            }
        }

        //Open input files and compare
        void OpenInputFilesAndCompare()
        {

            logger.Debug("TRYING TO OPEN INPUT FILES");

            try
            {
                using (_fileStreams[(int)MeansOfInputArgs.ORIGINAL_FILE] = new StreamReader(_inputArgValues[(int)MeansOfInputArgs.ORIGINAL_FILE]))
                {
                    try
                    {
                        using (_fileStreams[(int)MeansOfInputArgs.MODIFIED_FILE] = new StreamReader(_inputArgValues[(int)MeansOfInputArgs.MODIFIED_FILE]))
                        {
                            logger.Info("INPUT FILES ARE OPENED");

                            // comparing
                            Comparing();

                            // Comparing By Third-Party Lib
                            ComparingByThirdPartyLib();
                        }
                    }
                    catch
                    {
                        Console.WriteLine($"CANNOT OPEN  FILE {_inputArgValues[(int)MeansOfInputArgs.MODIFIED_FILE]}");

                        logger.Error($"CANNOT OPEN  FILE {_inputArgValues[(int)MeansOfInputArgs.MODIFIED_FILE]} APPLICATION WILL BE TERMINATED");

                        _fileStreams[(int)MeansOfInputArgs.MODIFIED_FILE].Close();

                        TerminateApp();
                    }
                }
            }
            catch
            {
                Console.WriteLine($"CANNOT OPEN  FILE {_inputArgValues[(int)MeansOfInputArgs.ORIGINAL_FILE]}");

                logger.Error($"CANNOT OPEN  FILE {_inputArgValues[(int)MeansOfInputArgs.ORIGINAL_FILE]} APPLICATION WILL BE TERMINATED");

                _fileStreams[(int)MeansOfInputArgs.ORIGINAL_FILE].Close();

                TerminateApp();
            } 
        }
    }
}
