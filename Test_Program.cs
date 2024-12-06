using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;

class Test_Program
{
    private const int ITERATIONS = 1000000;
    private const int LIST_SIZE = 10000;
    private const int TREE_SIZE = 100000;

    private const float test_intensity = 1F; // A multiplier of the constants above. 1F = uses the constants as is, 0.5F = half the values, etc.
    private const int TEST_COUNT = 3;        // Number of test runs

    // =========================== Don't change anything below this line - change above values instead ===========================

    private static List<Result> ResultsList = new List<Result>();
    private static float _test_intensity;
    private static int _ITERATIONS;
    private static int _LIST_SIZE;
    private static int _TREE_SIZE;

    private static bool DEBUGMODE = false;

    // -------------- Set test parameters based on variables above ---------------------
    private static void SetTestParams()
    {
        if (DEBUGMODE)
        {
            _test_intensity = 0.1F;
        }
        else
        {
            _test_intensity = test_intensity;
        }

        _ITERATIONS = (int)Math.Round(ITERATIONS * _test_intensity);
        _LIST_SIZE = (int)Math.Round(LIST_SIZE * _test_intensity);
        _TREE_SIZE = (int)Math.Round(TREE_SIZE * _test_intensity);
    }

    // Class for storing a particular test result for a single run
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

    // ----------------- Main method ---------------------

    static void Main(string[] args)
    {

        #if DEBUG
        DEBUGMODE = true;
        #endif

        if (Debugger.IsAttached)
        {
            DEBUGMODE = true;
        }

        SetTestParams();

        Console.WriteLine($"Running on {(Environment.Is64BitProcess ? "64-bit" : "32-bit")} process");
        Console.WriteLine($"Pointer size: {Marshal.SizeOf(typeof(IntPtr))} bytes");
        Console.WriteLine(CheckIfHiResolutionTimer());
        Console.WriteLine("Running performance tests...\n");

        if (DEBUGMODE)
        {
            Console.WriteLine("WARNING: Debug environment detected. ");
            Console.WriteLine("  >  Non-release compiled version may not produce realistic results due to lack of optimizations.");
            Console.WriteLine("  >  Also running tests with reduced intensity for faster debugging.");
            Console.WriteLine();
        }

        for (int i = 0; i < TEST_COUNT; i++)
        {
            Console.WriteLine($"Test run {i + 1}:");
            RunAllTests(i+1);
            Console.WriteLine();
        }

        SaveJsonToFile(ResultsList);

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    static void RunAllTests(int runNum)
    {
        void AddResult(Result resultVal, string testName)
        {
            resultVal.TestName = testName;
            resultVal.RunNumber = runNum;
            ResultsList.Add(resultVal);
        }

        Dictionary<string, string> testResultDict = new Dictionary<string, string>();
        var r = TestLinkedListTraversal();
        AddResult(r.resultVal, "LinkedList Traversal");
        testResultDict["LinkedList Traversal"] = r.resultStr;

        r = TestObjectAllocation();
        AddResult(r.resultVal, "Object Allocation");
        testResultDict["Object Allocation"] = r.resultStr;

        r = TestStringManipulation();
        AddResult(r.resultVal, "String Manipulation");
        testResultDict["String Manipulation"] = r.resultStr;

        r = TestBinaryTreeOperations();
        AddResult(r.resultVal, "Binary Tree Operations");
        testResultDict["Binary Tree Operations"] = r.resultStr;

        r = TestDictionaryOperations();
        AddResult(r.resultVal, "Dictionary Operations");
        testResultDict["Dictionary Operations"] = r.resultStr;

        // Display the results with the values lined up
        int maxKeyLength = testResultDict.Keys.Max(k => k.Length);
        foreach (var kvp in testResultDict)
        {
            Console.WriteLine($"{kvp.Key.PadRight(maxKeyLength)}: {kvp.Value}");
        }
    }

    private static void SaveJsonToFile(List<Result> resultList)
    {
        string fileNameBase = Environment.Is64BitProcess ? "Results64bit" : "Results32bit";
        string fileName;
        // If in debug mode, add _debug to the filename
        if (Debugger.IsAttached)
            fileNameBase += "_debug";

        fileName = $"{fileNameBase}.json";

        DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(List<Result>));

        using (FileStream fs = new FileStream(fileName, FileMode.Create))
        using (XmlDictionaryWriter writer = JsonReaderWriterFactory.CreateJsonWriter(fs, Encoding.UTF8, true, true, "  "))
        {
            ser.WriteObject(writer, resultList);
        }
    }

    // P/Invoke signature for QueryPerformanceFrequency - To determine if the system supports high-resolution performance counter
    [DllImport("Kernel32.dll")]
    private static extern bool QueryPerformanceFrequency(out long lpFrequency);

    public static string CheckIfHiResolutionTimer()
    {
        // Use QueryPerformanceFrequency  win32 api
        bool usingHiResTimer = Stopwatch.IsHighResolution;
        bool result;
        long frequency;
        result = QueryPerformanceFrequency(out frequency);

        string message = "Hi-Resolution Timer Support: " + usingHiResTimer + "  |  Frequency: " + frequency as string;
        return message;
    }

    static (string resultStr, Result resultVal) CreateStopwatchDisplayString(long ticks)
    {
        // First convert to seconds
        decimal seconds = ticks / (decimal)Stopwatch.Frequency;
        string unit;
        decimal value;
        string valueStr;

        unit = "ms";
        value = (seconds * 1000);
        valueStr = (seconds * 1000).ToString("0.000");

        // Format it so the decimal points and units are lined up
        string resultStr = $"{valueStr.PadLeft(8)} {unit}";

        Result result = new Result
        {
            TestResultValue = value,
            TestResultUnit = unit
        };

        return (resultStr, result);
    }

    static (string resultStr, Result resultVal) TestLinkedListTraversal()
    {
        var stopwatch = Stopwatch.StartNew();

        var list = new LinkedList<long>();
        for (int i = 0; i < _LIST_SIZE; i++)
        {
            list.AddLast(i);
        }

        long sum = 0;
        for (int i = 0; i < _ITERATIONS; i++)
        {
            var node = list.First;
            while (node != null)
            {
                sum += node.Value;
                node = node.Next;
            }
        }

        stopwatch.Stop();

        (string resultStr, Result resultVal) = CreateStopwatchDisplayString(stopwatch.ElapsedTicks);
        return (resultStr, resultVal);
    }

    static (string resultStr, Result resultVal) TestObjectAllocation()
    {
        var stopwatch = Stopwatch.StartNew();

        for (int i = 0; i < _ITERATIONS; i++)
        {
            var objects = new List<TestObject>();
            for (int j = 0; j < 100; j++)
            {
                objects.Add(new TestObject { Id = j, Value = j * 2 });
            }
        }

        stopwatch.Stop();
        (string resultStr, Result resultVal) = CreateStopwatchDisplayString(stopwatch.ElapsedTicks);
        return (resultStr, resultVal);
    }

    static (string resultStr, Result resultVal) TestStringManipulation()
    {
        var stopwatch = Stopwatch.StartNew();

        var stringBuilder = new StringBuilder();
        for (int i = 0; i < _ITERATIONS / 1000; i++)
        {
            stringBuilder.Clear();
            for (int j = 0; j < 100; j++)
            {
                stringBuilder.Append($"Item{j},");
            }
            var result = stringBuilder.ToString();
        }

        stopwatch.Stop();
        (string resultStr, Result resultVal) = CreateStopwatchDisplayString(stopwatch.ElapsedTicks);
        return (resultStr, resultVal);
    }

    static (string resultStr, Result resultVal) TestBinaryTreeOperations()
    {
        var stopwatch = Stopwatch.StartNew();

        var random = new Random(42);
        var tree = new BinaryTree();

        // Increased tree size and number of operations
        for (int i = 0; i < _TREE_SIZE; i++)
        {
            tree.Insert(random.Next(1000000));
        }

        int searchCount = 0;
        for (int i = 0; i < _ITERATIONS / 10; i++)  // Increased from /100 to /10
        {
            if (tree.Search(random.Next(1000000)))
            {
                searchCount++;
            }
        }

        stopwatch.Stop();
        (string resultStr, Result resultVal) = CreateStopwatchDisplayString(stopwatch.ElapsedTicks);
        return (resultStr, resultVal);
    }

    static (string resultStr, Result resultVal) TestDictionaryOperations()
    {
        var stopwatch = Stopwatch.StartNew();

        var dict = new Dictionary<string, TestObject>();
        var random = new Random(42);

        // Increased number of operations by 10x
        for (int i = 0; i < _ITERATIONS / 10; i++)  // Changed from /100 to /10
        {
            string key = $"key{random.Next(10000)}";  // Increased key range

            if (!dict.ContainsKey(key))
            {
                dict[key] = new TestObject { Id = i, Value = random.Next(1000) };
            }
            else
            {
                var obj = dict[key];
                obj.Value = random.Next(1000);
            }

            // More frequent removals
            if (i % 50 == 0)  // Changed from 100 to 50
            {
                key = $"key{random.Next(10000)}";
                dict.Remove(key);
            }
        }

        stopwatch.Stop();
        (string resultStr, Result resultVal) = CreateStopwatchDisplayString(stopwatch.ElapsedTicks);
        return (resultStr, resultVal);
    }
}

// Classes used in the tests
class TestObject
{
    public int Id { get; set; }
    public int Value { get; set; }
    public string Data { get; set; } = new string('x', 100);
}

class BinaryTreeNode
{
    public int Value;
    public BinaryTreeNode Left;
    public BinaryTreeNode Right;

    public BinaryTreeNode(int value)
    {
        Value = value;
    }
}

class BinaryTree
{
    private BinaryTreeNode root;

    public void Insert(int value)
    {
        if (root == null)
        {
            root = new BinaryTreeNode(value);
            return;
        }

        BinaryTreeNode current = root;
        while (true)
        {
            if (value < current.Value)
            {
                if (current.Left == null)
                {
                    current.Left = new BinaryTreeNode(value);
                    break;
                }
                current = current.Left;
            }
            else
            {
                if (current.Right == null)
                {
                    current.Right = new BinaryTreeNode(value);
                    break;
                }
                current = current.Right;
            }
        }
    }

    public bool Search(int value)
    {
        BinaryTreeNode current = root;
        while (current != null)
        {
            if (value == current.Value)
                return true;

            current = value < current.Value ? current.Left : current.Right;
        }
        return false;
    }
}