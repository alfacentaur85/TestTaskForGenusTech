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

        List<string> _differences = new List<string>();

        //Compare By ThirdParty Lib and write to list of defferences
        void ComparingByThirdPartyLib()
        {
            
            if (OpenInputFiles())
            {
                using (_fileStreams[(int)MeansOfInputArgs.ORIGINAL_FILE])
                {
                    using (_fileStreams[(int)MeansOfInputArgs.MODIFIED_FILE])
                    {
                        _differences.Clear();

                        differResult = differ.CreateLineDiffs(_fileStreams[(int)MeansOfInputArgs.ORIGINAL_FILE].ReadToEnd(),
                            _fileStreams[(int)MeansOfInputArgs.MODIFIED_FILE].ReadToEnd(), false);

                        for (int i = 0; i < differResult.DiffBlocks.Count; i++)
                        {
                            if (differResult.DiffBlocks[i].DeleteCountA == differResult.DiffBlocks[i].InsertCountB)
                            {
                                _differences.Add(string.Concat(differResult.DiffBlocks[i].DeleteStartA + 1, " : <modified line value for line ", differResult.DiffBlocks[i].DeleteStartA + 1, ">"));
                            }
                            else if (differResult.DiffBlocks[i].DeleteCountA > differResult.DiffBlocks[i].InsertCountB)
                            {
                                _differences.Add(string.Concat("<deleted line(s) from ",
                                       differResult.DiffBlocks[i].DeleteStartA + 1, " to " ,
                                       differResult.DiffBlocks[i].DeleteStartA + differResult.DiffBlocks[i].DeleteCountA, ">"));
                            }
                            else if (differResult.DiffBlocks[i].DeleteCountA < differResult.DiffBlocks[i].InsertCountB)
                            {
                                _differences.Add(string.Concat("<added line(s) from ",
                                       differResult.DiffBlocks[i].InsertStartB + 1, " to ",
                                       differResult.DiffBlocks[i].InsertStartB + differResult.DiffBlocks[i].InsertCountB, ">"));
                            }

                        }
                                              
                    }
                }
            }
        }

        public CompareClass(string[] args)
        {

            //Check count of input args
            if (args.Length >= Enum.GetValues(typeof(MeansOfInputArgs)).Length)
            {
                foreach (string FileName in args)
                {
                    _inputArgValues.Add(FileName);
                }
            }
            else
            {
                logger.Error("NOT ENOUGHTH INPUT ARGUMENTS.  APPLICATION WILL BE TERMINATED");

                Console.WriteLine("ERROR: NOT ENOUGHTH INPUT ARGUMENTS.  APPLICATION WILL BE TERMINATED");
                Environment.Exit(-1);
            }
        }

        //Start comparing
        public void StartComparing()
        {
            logger.Debug(string.Concat("STARTING APPLICATION AT ", DateTime.Now));
            Comparing();
            OutputResult("RESULT OF COMPARING BY CUSTOM:");
            Console.WriteLine();
            ComparingByThirdPartyLib();
            OutputResult("RESULT OF COMPARING BY THIRD-PARTY LIB:");
        }

        //Comparing files by lines and write to list of defferences
        void Comparing()
        {

            var curLine = 1;

            if (OpenInputFiles())
            {
                using (_fileStreams[(int)MeansOfInputArgs.ORIGINAL_FILE])
                {
                    using (_fileStreams[(int)MeansOfInputArgs.MODIFIED_FILE])
                    {
                        _differences.Clear();

                        while (!_fileStreams[(int)MeansOfInputArgs.ORIGINAL_FILE].EndOfStream && !_fileStreams[(int)MeansOfInputArgs.MODIFIED_FILE].EndOfStream)
                        {
                            if (String.Compare(_fileStreams[(int)MeansOfInputArgs.ORIGINAL_FILE].ReadLine(), _fileStreams[(int)MeansOfInputArgs.MODIFIED_FILE].ReadLine()) != 0)
                            {
                                _differences.Add(string.Concat(curLine, " : <modified line value for line ", curLine, ">"));
                            }
                            curLine++;
                        }
                    }
                }
            }
            else
            {
                Environment.Exit(-1);
            }
        }

        //Output result of comparing
        void OutputResult(string head)
        {
            Console.WriteLine(head);
            if (_differences.Count > 0)
            {
                logger.Info(head);

                foreach (string str in _differences)
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

        //Open input files
        bool OpenInputFiles()
        {
            try
            {
                logger.Debug("TRYING TO OPEN INPUT FILES");

                _fileStreams[(int)MeansOfInputArgs.ORIGINAL_FILE] = new StreamReader(_inputArgValues[(int)MeansOfInputArgs.ORIGINAL_FILE]);

                _fileStreams[(int)MeansOfInputArgs.MODIFIED_FILE] = new StreamReader(_inputArgValues[(int)MeansOfInputArgs.MODIFIED_FILE]);

                logger.Info("INPUT FILES ARE OPENED");

                return true;
            }
            catch
            {
                Console.WriteLine("CANNOT OPEN ONE OR SOME FILES. APPLICATION WILL BE TERMINATED");

                logger.Error("CANNOT OPEN ONE OR SOME FILES. APPLICATION WILL BE TERMINATED");

                return false;
            }
        }
    }
}
