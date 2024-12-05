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
            string x64FileName = "Results64bit.json";
            string x86FileName = "Results32bit.json";

            if (Debugger.IsAttached)
            {
                x64FileName = "Results64bit_debug.json";
                x86FileName = "Results32bit_debug.json";
            }

            // Root path with the results files
            string rootPath = "D:\\Users\\Joe\\Documents\\Development\\Test-Architecture-Speed";

            // Check hard coded paths first
            string x64Path = FindFile(rootPath, x64FileName);
            string x86Path = FindFile(rootPath, x86FileName);

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

            Console.WriteLine("Press any key to exit");
            Console.ReadLine();

        }

        static void CompareResults(List<Result> x64Results, List<Result> x86Results)
        {

            // For each test, get the average of the results of the runs. See which was faster and by what percentage
            // Assume same number of tests in both lists

            // Local function
            List<TestResult> GetResultsList(List<Result> results)
            {
                List<TestResult> testResults = new List<TestResult>();
                foreach (Result result in results)
                {
                    TestResult testResult = new TestResult();
                    testResult.TestName = result.TestName;
                    testResult.AverageResult = result.TestResultValue;
                    testResult.ResultUnit = result.TestResultUnit;
                    testResults.Add(testResult);
                }
                return testResults;
            }

            List<TestResult> x64TestResults = GetResultsList(x64Results);
            List<TestResult> x86TestResults = GetResultsList(x86Results);

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

                if (x64TestResult.AverageResult < x86TestResult.AverageResult)
                {
                    Console.WriteLine($"{x64TestResult.TestName} was faster on x64 by {Math.Round((x86TestResult.AverageResult - x64TestResult.AverageResult) / x86TestResult.AverageResult * 100, 2)}%");
                }
                else if (x64TestResult.AverageResult > x86TestResult.AverageResult)
                {
                    Console.WriteLine($"{x64TestResult.TestName} was faster on x86 by {Math.Round((x64TestResult.AverageResult - x86TestResult.AverageResult) / x64TestResult.AverageResult * 100, 2)}%");
                }
                else
                {
                    Console.WriteLine($"{x64TestResult.TestName} had the same average result on both x64 and x86");
                }
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
