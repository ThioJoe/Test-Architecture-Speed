using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

class Program
{
    private const int ITERATIONS = 1000000;
    private const int LIST_SIZE = 10000;
    private const int TREE_SIZE = 100000;

    private const float test_intensity = 0.3F;

    private const int TEST_COUNT = 3;


    // -----------------------------------

    private static int _ITERATIONS = (int)Math.Round(ITERATIONS * test_intensity);
    private static int _LIST_SIZE = (int)Math.Round(LIST_SIZE * test_intensity);
    private static int _TREE_SIZE = (int)Math.Round(LIST_SIZE * test_intensity);

    static void Main(string[] args)
    {
        Console.WriteLine($"Running on {(Environment.Is64BitProcess ? "64-bit" : "32-bit")} process");
        Console.WriteLine($"Pointer size: {Marshal.SizeOf(typeof(IntPtr))} bytes");
        Console.WriteLine(CheckIfHiResolutionTimer());
        Console.WriteLine("Running performance tests...\n");

        for (int i = 0; i < TEST_COUNT; i++)
        {
            Console.WriteLine($"Test run {i + 1}:");
            RunAllTests();
            Console.WriteLine();
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    static void RunAllTests()
    {
        Dictionary<string, string> testResultDict = new Dictionary<string, string>();
        testResultDict["LinkedList Traversal"] = TestLinkedListTraversal();
        testResultDict["Object Allocation"] = TestObjectAllocation();
        testResultDict["String Manipulation"] = TestStringManipulation();
        testResultDict["Binary Tree Operations"] = TestBinaryTreeOperations();
        testResultDict["Dictionary Operations"] = TestDictionaryOperations();

        // Display the results with the values lined up
        int maxKeyLength = testResultDict.Keys.Max(k => k.Length);
        foreach (var kvp in testResultDict)
        {
            Console.WriteLine($"{kvp.Key.PadRight(maxKeyLength)}: {kvp.Value}");
        }

    }

    // P/Invoke signature for QueryPerformanceFrequency
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

    static string CreateStopwatchDisplayString(long ticks)
    {
        // First convert to seconds
        decimal seconds = ticks / (decimal)Stopwatch.Frequency;
        string unit;
        string value;

        //if (seconds >= 1 || seconds >= 0.001M)
        //{
        //    unit = "s";
        //    value = seconds.ToString("0.000");
        //}

        // Use minimum resolution unit in milliseconds
        if (seconds >= 1 || seconds >= 0.001M) // milliseconds range
        {
            unit = "ms";
            value = (seconds * 1000).ToString("0.000");
        }
        else if (seconds >= 0.000001M) // microseconds range
        {
            unit = "μs";
            value = (seconds * 1000000).ToString("0.000");
        }
        else // nanoseconds range
        {
            unit = "ns";
            value = (seconds * 1000000000).ToString("0.000");
        }

        // Format it so the decimal points and units are lined up
        string resultStr = $"{value.PadLeft(8)} {unit}";
        return resultStr;
    }

    static string TestLinkedListTraversal()
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

        string resultStr = CreateStopwatchDisplayString(stopwatch.ElapsedTicks);
        return resultStr;
    }

    static string TestObjectAllocation()
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
        string resultStr = CreateStopwatchDisplayString(stopwatch.ElapsedTicks);
        return resultStr;
    }

    static string TestStringManipulation()
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
        string resultStr = CreateStopwatchDisplayString(stopwatch.ElapsedTicks);
        return resultStr;
    }

    static string TestBinaryTreeOperations()
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
        string resultStr = CreateStopwatchDisplayString(stopwatch.ElapsedTicks);
        return resultStr;
    }

    static string TestDictionaryOperations()
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
        string resultStr = CreateStopwatchDisplayString(stopwatch.ElapsedTicks);
        return resultStr;
    }
}

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