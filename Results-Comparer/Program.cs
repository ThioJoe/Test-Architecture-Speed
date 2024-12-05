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
    internal class Program
    {

        public static string x64FileName;
        public static string x86FileName;
        public static string x64Path;
        public static string x86Path;

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

        private class TestResult
        {
            public string TestName { get; set; }
            public decimal AverageResult { get; set; }
            public string ResultUnit { get; set; }
        }


        static void Main(string[] args)
        {
            // File names
            x64FileName = "Results64bit.json";
            x86FileName = "Results32bit.json";

            if (Debugger.IsAttached)
            {
                //x64FileName = "Results64bit_debug.json";
                //x86FileName = "Results32bit_debug.json";
            }

            // Root path with the results files
            string rootPath = "D:\\Users\\Joe\\Documents\\Development\\Test-Architecture-Speed";

            // Check hard coded paths first
            x64Path = FindFile(rootPath, x64FileName);
            x86Path = FindFile(rootPath, x86FileName);

            if (x64Path == null || x86Path == null)
            {
                string userPath = PromptPath();
                x64Path = FindFile(userPath, x64FileName);
                x86Path = FindFile(userPath, x86FileName);

                if (x64Path == null || x86Path == null)
                {
                    Console.WriteLine("Results files not found.");
                    return;
                }
            }

            // Deserialize using the DataContractJsonSerializer
            List<Result> x64Results;
            List<Result> x86Results;

            using (FileStream fs = new FileStream(x64Path, FileMode.Open))
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<Result>));
                x64Results = (List<Result>)serializer.ReadObject(fs);
            }

            using (FileStream fs = new FileStream(x86Path, FileMode.Open))
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<Result>));
                x86Results = (List<Result>)serializer.ReadObject(fs);
            }

            // Compare the results
            CompareResults(x64Results, x86Results);

            Console.WriteLine("\n\nPress any key to exit");
            Console.ReadLine();

        }

        static void CompareResults(List<Result> x64Results, List<Result> x86Results)
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

            List<TestResult> x64TestResults = GetAverageResultsList(x64Results);
            List<TestResult> x86TestResults = GetAverageResultsList(x86Results);

            // Print file locations
            Console.WriteLine("x64 results file: {0}", x64Path);
            Console.WriteLine("x86 results file: {0}", x86Path);
            Console.WriteLine("\n\n");

            // Print table header
            Console.WriteLine("{0,-30} | {1,16} | {2,16} | {3,15} | {4,20}", "Test Name", "x64 Avg", "x86 Avg", "Winner", "Difference");
            Console.WriteLine("{0,-30} | {1,16} | {2,16} | {3,6} | {4,6} | {5,20}", "", "", "", "x64", "x86", "");
            Console.WriteLine("{0,-30} | {1,16} | {2,16} | {3,6} | {4,6} | {5,20}", new string('-', 30), new string('-', 16), new string('-', 16), new string('-', 6), new string('-', 6), new string('-', 20));

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
                    x86Faster = "√";
                    difference = diff;
                }
                else if (x64TestResult.AverageResult > x86TestResult.AverageResult)
                {
                    string diff = $"{Math.Round((x64TestResult.AverageResult - x86TestResult.AverageResult) / x64TestResult.AverageResult * 100, 2)}%";
                    x64Faster = "√";
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
        }

        static string FindFile(string rootPath, string fileName)
        {
            string[] files = Directory.GetFiles(rootPath, fileName, SearchOption.AllDirectories);
            if (files.Length == 0)
            {
                return null;
            }
            return files[0];
        }

        static string PromptPath()
        {
            string message = "Enter the folder path where the results files are located. Subdirectories will be searched too.";
            Console.WriteLine(message);
            Console.Write("Enter: ");
            return Console.ReadLine();
        }
    }
}
