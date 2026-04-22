using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Json;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;
using System.Runtime.Serialization;

namespace Results_Comparer
{
    internal class Compare_Program
    {

        // Globals
        public static string x64Path;
        public static string x86Path;

        public static bool usingDebugFiles = false;

        [DataContract]
        private class Result
        {
            [DataMember]
            public string TestName { get; set; }
            [DataMember]
            public decimal TestResultValue { get; set; }
            [DataMember]
            public string TestResultUnit { get; set; }
            [DataMember]
            public int RunNumber { get; set; }
        }

        [DataContract]
        private class AllResultsInfo
        {
            [DataMember]
            public List<Result> Results { get; set; }
            [DataMember]
            public bool OptimizationsDisabled { get; set; }
            [DataMember]
            public bool DebugMode { get; set; }
        }

        private class TestResult
        {
            public string TestName { get; set; }
            public decimal AverageResult { get; set; }
            public string ResultUnit { get; set; }
        }


        // ----------------- Main -----------------

        static void Main(string[] args)
        {
            // Show info about the program
            Console.WriteLine("----------- Test-Architecture-Speed Results Comparison Tool ----------");
            Console.WriteLine("Compare results from two json files created by Test-Architecture-Speed\n\n");

            var filePairs = DetermineFilePairs();

            if (filePairs == null || filePairs.Count == 0)
            {
                Console.WriteLine("Press any key to exit.");
                Console.ReadLine();
                return;
            }

            for (int pairIndex = 0; pairIndex < filePairs.Count; pairIndex++)
            {
                (x64Path, x86Path) = filePairs[pairIndex];

                if (filePairs.Count > 1)
                {
                    Console.WriteLine("\n\n\n=============================================================================================================");
                    Console.WriteLine($"  Comparing pair {pairIndex + 1} of {filePairs.Count}:");
                    Console.WriteLine($"  x64: {Path.GetFileName(x64Path)}");
                    Console.WriteLine($"  x86: {Path.GetFileName(x86Path)}");
                    Console.WriteLine("-------------------------------------");
                }

                // Deserialize the json files back into objects using the DataContractJsonSerializer
                AllResultsInfo x64ResultsInfo;
                AllResultsInfo x86ResultsInfo;

                using (FileStream fs = new FileStream(x64Path, FileMode.Open))
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(AllResultsInfo));
                    x64ResultsInfo = (AllResultsInfo)serializer.ReadObject(fs);
                }

                using (FileStream fs = new FileStream(x86Path, FileMode.Open))
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(AllResultsInfo));
                    x86ResultsInfo = (AllResultsInfo)serializer.ReadObject(fs);
                }

                usingDebugFiles = x64ResultsInfo.DebugMode || x86ResultsInfo.DebugMode;

                bool optimizations_off = x64ResultsInfo.OptimizationsDisabled || x86ResultsInfo.OptimizationsDisabled;

                // Compare the results
                CompareResults(x64Results: x64ResultsInfo.Results, x86Results: x86ResultsInfo.Results, optimizations_off: optimizations_off, debug_results: usingDebugFiles);
            }

            Console.WriteLine("\n\nPress any key to exit");
            Console.ReadLine();

        }

        // -------------------------- Scaffolding / Results files detection functions --------------------------

        // Determine root path with the results files. Tries to see if the program is running from the solution directory, otherwise prompts user for folder
        static List<(string x64PathStr, string x86PathStr)> DetermineFilePairs()
        {
            string rootPath;
            bool isUserEnteredPath = false;
            List<(string, string)> pairs;

            // First look in the current directory
            pairs = FindFilePairsInDirectory(Directory.GetCurrentDirectory(), searchSubDirectories: false);

            if (pairs.Count == 0)
            {
                var pathResult = DetermineProjectRootPath();
                if (pathResult == null)
                    return null;

                rootPath = pathResult.Value.rootPath;
                isUserEnteredPath = pathResult.Value.userEnteredPath;

                pairs = FindFilePairsInDirectory(rootPath, searchSubDirectories: !isUserEnteredPath);
            }

            if (pairs.Count == 0)
            {
                if (isUserEnteredPath)
                {
                    Console.WriteLine("Results files not found in the specified directory.");
                    return null;
                }

                string userPath = PromptPath();
                if (userPath == null)
                    return null;

                pairs = FindFilePairsInDirectory(userPath, searchSubDirectories: false);

                if (pairs.Count == 0)
                {
                    Console.WriteLine("Results files not found in the specified directory.");
                    return null;
                }
            }

            return pairs;
        }

        // Finds all matched x64/x86 result file pairs in a directory, grouped by their language/framework suffix.
        // Files are named Results64bit[_suffix].json and Results32bit[_suffix].json where suffix identifies the language.
        static List<(string x64File, string x86File)> FindFilePairsInDirectory(string rootPath, bool searchSubDirectories)
        {
            SearchOption option = searchSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            string[] x64Files = Directory.GetFiles(rootPath, "Results64bit*.json", option);
            string[] x86Files = Directory.GetFiles(rootPath, "Results32bit*.json", option);

            // Build a lookup from suffix -> x86 file path
            // Suffix is everything after "Results32bit" and before ".json" (e.g. "_c++", "_DotNet48", "")
            var x86BySuffix = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (string file in x86Files)
            {
                string name = Path.GetFileNameWithoutExtension(file); // e.g. "Results32bit_c++"
                string suffix = name.Substring("Results32bit".Length);  // e.g. "_c++" or ""
                x86BySuffix[suffix] = file;
            }

            var pairs = new List<(string, string)>();
            foreach (string x64File in x64Files)
            {
                string name = Path.GetFileNameWithoutExtension(x64File);
                string suffix = name.Substring("Results64bit".Length);

                if (x86BySuffix.TryGetValue(suffix, out string x86File))
                {
                    pairs.Add((x64File, x86File));
                }
            }

            return pairs;
        }

        // Used to determine if the program is running from the solution directory / repository folder structure
        static (string rootPath, bool userEnteredPath)? DetermineProjectRootPath()
        {
            string projectRootPath;
            bool userEnteredPath = false;

            string currentDirectory = Directory.GetCurrentDirectory();
            // If "Results-Comparer" is in the path, then we are probably in the solution directory
            if (currentDirectory.Contains("Test-Architecture-Speed\\Results-Comparer"))
            {
                // Set the project root path to the solution directory (Test-Architecture-Speed)
                projectRootPath = currentDirectory.Substring(0, currentDirectory.IndexOf("Results-Comparer") - 1);

            }
            // Otherwise if "Results-Comparer" is at least in the path, use that as the root path
            else if (currentDirectory.Contains("Results-Comparer"))
            {
                projectRootPath = currentDirectory.Substring(0, currentDirectory.IndexOf("Results-Comparer") - 1);
            }
            else if (currentDirectory.Contains("Test-Architecture-Speed"))
            {
                projectRootPath = currentDirectory.Substring(0, currentDirectory.IndexOf("Test-Architecture-Speed") - 1);
            }
            else
            {
                // Prompt the user for the path to the results files
                projectRootPath = PromptPath();

                if (projectRootPath == null)
                    return null;

                userEnteredPath = true;
            }

            return (projectRootPath, userEnteredPath);
        }

        static string PromptPath()
        {
            Console.WriteLine("Enter the folder path containing the json results files.");
            Console.Write("Enter path: ");
            string input = Console.ReadLine();
            input = input.Trim('"');
            if (!Directory.Exists(input))
            {
                Console.WriteLine("Invalid Path - Path not found.");
                return null;
            }
            else
            {
                return input;
            }
        }

        // ------------------------------------ Actual comparison logic ------------------------------------
        static void CompareResults(List<Result> x64Results, List<Result> x86Results, bool optimizations_off, bool debug_results)
        {
            // Local function to get average results grouped by TestName
            List<TestResult> GetAverageResultsList(List<Result> results)
            {
                return results
                    .GroupBy(r => new { r.TestName, r.TestResultUnit })
                    .Select(g => new TestResult
                    {
                        TestName = g.Key.TestName,
                        AverageResult = g.Average(r => r.TestResultValue),
                        ResultUnit = g.Key.TestResultUnit
                    })
                    .ToList();
            }
            // ------------------ End of local function ------------------

            List<TestResult> x64TestResults = GetAverageResultsList(x64Results);
            List<TestResult> x86TestResults = GetAverageResultsList(x86Results);

            // Print file locations
            //Console.WriteLine("x64 results file: {0}", x64Path);
            //Console.WriteLine("x86 results file: {0}", x86Path);

            if (debug_results)
            {
                Console.WriteLine("\nWARNING: Using results from a debug session or debug version of binaries - results may not be realistic.");
            }
            if (optimizations_off)
            {
                Console.WriteLine("WARNING: Results were obtained with optimizations disabled - results may not be realistic.");
            }

            Console.WriteLine();

            // Print table header with centered text
            Console.WriteLine("{0} | {1} | {2} | {3} | {4}",
                "Test Name".PadLeft((30 + 9) / 2).PadRight(30),
                "x64 Avg".PadLeft(8 + (16 - 8) / 2).PadRight(16),
                "x86 Avg".PadLeft(8 + (16 - 8) / 2).PadRight(16),
                "Winner".PadLeft(7 + (15 - 7) / 2).PadRight(15),
                "Difference".PadLeft(10 + (20 - 10) / 2).PadRight(20));

            Console.WriteLine("{0} | {1} | {2} | {3} | {4} | {5}",
                "".PadRight(30),
                "".PadRight(16),
                "".PadRight(16),
                "x64".PadLeft(3 + (6 - 3) / 2).PadRight(6),           // Center in 6 chars
                "x86".PadLeft(3 + (6 - 3) / 2).PadRight(6),           // Center in 6 chars
                "".PadRight(20));

            // Separator line
            Console.WriteLine("{0,-30} | {1,16} | {2,16} | {3,6} | {4,6} | {5,20}",
                new string('-', 30),
                new string('-', 16),
                new string('-', 16),
                new string('-', 6),
                new string('-', 6),
                new string('-', 20));

            // Compare the results
            for (int i = 0; i < x64TestResults.Count; i++)
            {
                TestResult x64TestResult = x64TestResults[i];
                TestResult x86TestResult = x86TestResults[i];

                if (x64TestResult.TestName != x86TestResult.TestName)
                {
                    Console.WriteLine("Test names do not match.");
                    return;
                }

                string x64Faster = "";
                string x86Faster = "";
                string difference = "";

                if (x64TestResult.AverageResult < x86TestResult.AverageResult)
                {
                    string diff = $"{Math.Round((x86TestResult.AverageResult - x64TestResult.AverageResult) / x86TestResult.AverageResult * 100, 2)}%";
                    x64Faster = "√";
                    difference = diff;
                }
                else if (x64TestResult.AverageResult > x86TestResult.AverageResult)
                {
                    string diff = $"{Math.Round((x64TestResult.AverageResult - x86TestResult.AverageResult) / x64TestResult.AverageResult * 100, 2)}%";
                    x86Faster = "√";
                    difference = diff;
                }
                else
                {
                    difference = "0%";
                }

                Console.WriteLine("{0,-30} | {1,10:F2} {2,-5} | {3,10:F2} {4,-5} | {5,6} | {6,6} | {7,-20}",
                    x64TestResult.TestName,
                    x64TestResult.AverageResult, x64TestResult.ResultUnit,
                    x86TestResult.AverageResult, x86TestResult.ResultUnit,
                    x64Faster.PadLeft(3).PadRight(6),
                    x86Faster.PadLeft(3).PadRight(6),
                    difference);
            }

        } // End of CompareResults

    } // ------------------ End of class Compare_Program ------------------

} // ------------------ End of namespace Results_Comparer ------------------
