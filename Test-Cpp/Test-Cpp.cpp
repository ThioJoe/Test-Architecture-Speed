// Test-Cpp.cpp : C++ port of Test_Program.cs
// All the same tests: LinkedList Traversal, Object Allocation, String Manipulation, Binary Tree Operations, Dictionary Operations

#include <iostream>
#include <vector>
#include <list>
#include <map>
#include <unordered_map>
#include <string>
#include <sstream>
#include <fstream>
#include <chrono>
#include <cmath>
#include <cstdlib>
#include <cstring>
#include <iomanip>
#include <algorithm>
#include <random>

#ifdef _WIN32
#include <windows.h>
#endif

// ===================== Constants =====================
static const int ITERATIONS = 1000000;
static const int LIST_SIZE = 10000;
static const int TREE_SIZE = 100000;

static const float test_intensity = 1.0f;
static const int TEST_COUNT = 3;

// ===================== Globals =====================
static float _test_intensity;
static int _ITERATIONS;
static int _LIST_SIZE;
static int _TREE_SIZE;
static int _TEST_COUNT;

static bool DEBUG_BUILD = false;
static bool OPTIMIZATIONS_OFF = false;
static bool showHelp = false;

static const char* HELP_TEXT =
    "Usage: Test-Cpp.exe [arguments]\n"
    "\n"
    "Arguments:\n"
    "   -iterations <int>       Number of iterations used in the tests (positive integer), Default = 1000000\n"
    "   -list_size <int>        Size of the list for the LinkedList traversal (positive integer), Default = 10000\n"
    "   -tree_size <int>        Size of the binary tree for that test (positive integer), Default = 100000\n"
    "   -test_intensity <float> Intensity multiplier for the tests (positive float), Default = 1\n"
    "   -test_count <int>       Number of test runs (positive integer), Default = 3\n"
    "\n"
    "Example:\n"
    "Test-Cpp.exe -iterations 500000 -list_size 5000 -tree_size 50000 -test_intensity 0.5 -test_count 5\n\n"
    "--------------------------------------------------------------------------------------------------------";

// ===================== Result struct =====================
struct Result {
    std::string TestName;
    double TestResultValue;
    std::string TestResultUnit;
    int RunNumber;
};

static std::vector<Result> ResultsList;

// ===================== TestObject =====================
struct TestObject {
    int Id;
    int Value;
    std::string Data;
    TestObject() : Id(0), Value(0), Data(std::string(100, 'x')) {}
};

// ===================== BinaryTree =====================
struct BinaryTreeNode {
    int Value;
    BinaryTreeNode* Left;
    BinaryTreeNode* Right;
    BinaryTreeNode(int v) : Value(v), Left(nullptr), Right(nullptr) {}
};

class BinaryTree {
    BinaryTreeNode* root;

    void DeleteTree(BinaryTreeNode* node) {
        if (!node) return;
        DeleteTree(node->Left);
        DeleteTree(node->Right);
        delete node;
    }

public:
    BinaryTree() : root(nullptr) {}
    ~BinaryTree() { DeleteTree(root); }

    void Insert(int value) {
        if (!root) {
            root = new BinaryTreeNode(value);
            return;
        }
        BinaryTreeNode* current = root;
        while (true) {
            if (value < current->Value) {
                if (!current->Left) {
                    current->Left = new BinaryTreeNode(value);
                    break;
                }
                current = current->Left;
            } else {
                if (!current->Right) {
                    current->Right = new BinaryTreeNode(value);
                    break;
                }
                current = current->Right;
            }
        }
    }

    bool Search(int value) {
        BinaryTreeNode* current = root;
        while (current) {
            if (value == current->Value)
                return true;
            current = value < current->Value ? current->Left : current->Right;
        }
        return false;
    }
};

// ===================== Timer helpers =====================
using Clock = std::chrono::high_resolution_clock;

static std::pair<std::string, Result> CreateStopwatchDisplayString(std::chrono::duration<double> elapsed)
{
    double seconds = elapsed.count();
    double ms = seconds * 1000.0;

    std::ostringstream oss;
    oss << std::fixed << std::setprecision(3) << ms;
    std::string valueStr = oss.str();

    // Pad left to 8 chars
    while (valueStr.size() < 8)
        valueStr = " " + valueStr;

    std::string resultStr = valueStr + " ms";

    Result result;
    result.TestResultValue = ms;
    result.TestResultUnit = "ms";

    return { resultStr, result };
}

// ===================== Tests =====================

static std::pair<std::string, Result> TestLinkedListTraversal()
{
    auto start = Clock::now();

    std::list<long long> linkedList;
    for (int i = 0; i < _LIST_SIZE; i++)
        linkedList.push_back(i);

    long long sum = 0;
    for (int i = 0; i < _ITERATIONS; i++) {
        for (auto it = linkedList.begin(); it != linkedList.end(); ++it) {
            sum += *it;
        }
    }

    auto elapsed = Clock::now() - start;
    // Prevent optimization of sum away
    if (sum == -1) std::cout << sum;
    return CreateStopwatchDisplayString(elapsed);
}

static std::pair<std::string, Result> TestObjectAllocation()
{
    auto start = Clock::now();

    for (int i = 0; i < _ITERATIONS; i++) {
        std::vector<TestObject> objects;
        objects.reserve(100);
        for (int j = 0; j < 100; j++) {
            TestObject obj;
            obj.Id = j;
            obj.Value = j * 2;
            objects.push_back(std::move(obj));
        }
    }

    auto elapsed = Clock::now() - start;
    return CreateStopwatchDisplayString(elapsed);
}

static std::pair<std::string, Result> TestStringManipulation()
{
    auto start = Clock::now();

    std::string builder;
    for (int i = 0; i < _ITERATIONS / 1000; i++) {
        builder.clear();
        for (int j = 0; j < 100; j++) {
            builder += "Item";
            builder += std::to_string(j);
            builder += ",";
        }
        // Use result to prevent optimization
        volatile size_t len = builder.size();
        (void)len;
    }

    auto elapsed = Clock::now() - start;
    return CreateStopwatchDisplayString(elapsed);
}

static std::pair<std::string, Result> TestBinaryTreeOperations()
{
    auto start = Clock::now();

    std::mt19937 rng(42);
    std::uniform_int_distribution<int> dist(0, 999999);

    BinaryTree tree;
    for (int i = 0; i < _TREE_SIZE; i++)
        tree.Insert(dist(rng));

    int searchCount = 0;
    for (int i = 0; i < _ITERATIONS / 10; i++) {
        if (tree.Search(dist(rng)))
            searchCount++;
    }

    auto elapsed = Clock::now() - start;
    if (searchCount == -1) std::cout << searchCount;
    return CreateStopwatchDisplayString(elapsed);
}

static std::pair<std::string, Result> TestDictionaryOperations()
{
    auto start = Clock::now();

    std::unordered_map<std::string, TestObject> dict;
    std::mt19937 rng(42);
    std::uniform_int_distribution<int> distKey(0, 9999);
    std::uniform_int_distribution<int> distVal(0, 999);

    for (int i = 0; i < _ITERATIONS / 10; i++) {
        std::string key = "key" + std::to_string(distKey(rng));

        auto it = dict.find(key);
        if (it == dict.end()) {
            TestObject obj;
            obj.Id = i;
            obj.Value = distVal(rng);
            dict[key] = std::move(obj);
        } else {
            it->second.Value = distVal(rng);
        }

        if (i % 50 == 0) {
            key = "key" + std::to_string(distKey(rng));
            dict.erase(key);
        }
    }

    auto elapsed = Clock::now() - start;
    return CreateStopwatchDisplayString(elapsed);
}

// ===================== RunAllTests =====================

static void RunAllTests(int runNum)
{
    std::vector<std::pair<std::string, std::string>> testResults;

    auto addResult = [&](std::pair<std::string, Result>& r, const char* testName) {
        r.second.TestName = testName;
        r.second.RunNumber = runNum;
        ResultsList.push_back(r.second);
        testResults.push_back({ testName, r.first });
    };

    auto r = TestLinkedListTraversal();
    addResult(r, "LinkedList Traversal");

    r = TestObjectAllocation();
    addResult(r, "Object Allocation");

    r = TestStringManipulation();
    addResult(r, "String Manipulation");

    r = TestBinaryTreeOperations();
    addResult(r, "Binary Tree Operations");

    r = TestDictionaryOperations();
    addResult(r, "Dictionary Operations");

    // Find max key length for alignment
    size_t maxLen = 0;
    for (auto& p : testResults)
        if (p.first.size() > maxLen) maxLen = p.first.size();

    for (auto& p : testResults) {
        std::string padded = p.first;
        while (padded.size() < maxLen) padded += ' ';
        std::cout << padded << ": " << p.second << std::endl;
    }
}

// ===================== JSON output =====================

static std::string EscapeJson(const std::string& s)
{
    std::string out;
    for (char c : s) {
        if (c == '"') out += "\\\"";
        else if (c == '\\') out += "\\\\";
        else out += c;
    }
    return out;
}

static void SaveJsonToFile(const std::vector<Result>& results)
{
    bool is64 = (sizeof(void*) == 8);
    std::string fileNameBase = std::string(is64 ? "Results64bit" : "Results32bit") + "_c++";

#ifndef NDEBUG
    fileNameBase += "_debug";
#endif

    std::string fileName = fileNameBase + ".json";

    std::ofstream ofs(fileName);
    ofs << "{\n";
    ofs << "  \"DebugMode\": " << (DEBUG_BUILD ? "true" : "false") << ",\n";
    ofs << "  \"OptimizationsDisabled\": " << (OPTIMIZATIONS_OFF ? "true" : "false") << ",\n";
    ofs << "  \"Results\": [\n";

    for (size_t i = 0; i < results.size(); i++) {
        const auto& r = results[i];
        ofs << "    {\n";
        ofs << "      \"RunNumber\": " << r.RunNumber << ",\n";
        ofs << "      \"TestName\": \"" << EscapeJson(r.TestName) << "\",\n";
        ofs << "      \"TestResultUnit\": \"" << EscapeJson(r.TestResultUnit) << "\",\n";
        ofs << "      \"TestResultValue\": " << std::fixed << std::setprecision(6) << r.TestResultValue << "\n";
        ofs << "    }";
        if (i + 1 < results.size()) ofs << ",";
        ofs << "\n";
    }

    ofs << "  ]\n";
    ofs << "}\n";
    ofs.close();
}

// ===================== Argument Parsing =====================

static bool IsValidArg(const std::string& key)
{
    static const char* valid[] = { "iterations", "list_size", "tree_size", "test_intensity", "test_count", "?", "help" };
    for (auto& v : valid)
        if (key == v) return true;
    return false;
}

static bool IsSwitch(const std::string& key)
{
    return key == "?" || key == "help";
}

static bool IsIntArg(const std::string& key)
{
    return key == "iterations" || key == "list_size" || key == "tree_size" || key == "test_count";
}

static bool IsFloatArg(const std::string& key)
{
    return key == "test_intensity";
}

static std::string TrimLeading(const std::string& s, char c)
{
    size_t start = 0;
    while (start < s.size() && s[start] == c) start++;
    return s.substr(start);
}

static void ParseArguments(int argc, char* argv[])
{
    bool usedInvalidArgs = false;

    for (int i = 1; i < argc; i++) {
        std::string arg = argv[i];
        if (arg[0] == '-' || arg[0] == '/') {
            std::string key = TrimLeading(arg, '-');
            key = TrimLeading(key, '/');

            if (!IsValidArg(key)) {
                std::cout << "ERROR: Skipping Invalid argument: " << key << std::endl;
                usedInvalidArgs = true;
                continue;
            }

            if (IsSwitch(key)) {
                if (key == "?" || key == "help")
                    showHelp = true;
                continue;
            }

            // Expect a value
            std::string value;
            bool hasValue = false;
            if (i + 1 < argc) {
                std::string next = argv[i + 1];
                if (next[0] != '-' && next[0] != '/') {
                    value = next;
                    hasValue = true;
                } else {
                    // Check if it's a negative number
                    char* end;
                    std::strtol(next.c_str(), &end, 10);
                    if (*end == '\0') {
                        value = next;
                        hasValue = true;
                    }
                }
            }

            if (IsIntArg(key)) {
                if (!hasValue) {
                    std::cout << "ERROR: Missing value for argument " << key << std::endl;
                    usedInvalidArgs = true;
                    continue;
                }
                char* end;
                long val = std::strtol(value.c_str(), &end, 10);
                if (*end != '\0' || val <= 0) {
                    std::cout << "ERROR: Invalid value for argument " << key << ": " << value << " (Must be a positive integer)" << std::endl;
                    usedInvalidArgs = true;
                    continue;
                }
                std::cout << "Setting " << key << " to " << val << std::endl;
                if (key == "iterations") _ITERATIONS = (int)val;
                else if (key == "list_size") _LIST_SIZE = (int)val;
                else if (key == "tree_size") _TREE_SIZE = (int)val;
                else if (key == "test_count") _TEST_COUNT = (int)val;
                i++; // skip value
            } else if (IsFloatArg(key)) {
                if (!hasValue) {
                    std::cout << "ERROR: Missing value for argument " << key << std::endl;
                    usedInvalidArgs = true;
                    continue;
                }
                char* end;
                float val = std::strtof(value.c_str(), &end);
                if (*end != '\0' || val <= 0) {
                    std::cout << "ERROR: Invalid value for argument " << key << ": " << value << " (Must be a positive float)" << std::endl;
                    usedInvalidArgs = true;
                    continue;
                }
                std::cout << "Setting " << key << " to " << val << std::endl;
                _test_intensity = val;
                // Recalculate parameters with new intensity
                _ITERATIONS = (int)std::round(ITERATIONS * _test_intensity);
                _LIST_SIZE = (int)std::round(LIST_SIZE * _test_intensity);
                _TREE_SIZE = (int)std::round(TREE_SIZE * _test_intensity);
                i++; // skip value
            }
        }
    }

    if (usedInvalidArgs)
        std::cout << "----- WARNING: Some arguments were invalid or missing values. See above errors. -----\n" << std::endl;
}

// ===================== Utility =====================

static std::string CheckIfHiResolutionTimer()
{
    std::ostringstream oss;

#ifdef _WIN32
    LARGE_INTEGER freq;
    bool result = QueryPerformanceFrequency(&freq) != 0;
    oss << "Hi-Resolution Timer Support: " << (result ? "True" : "False")
        << "  |  Frequency: " << freq.QuadPart;
#else
    oss << "Hi-Resolution Timer Support: True (chrono::high_resolution_clock)";
#endif

    return oss.str();
}

static void SetTestParams()
{
    _test_intensity = test_intensity;

#ifndef NDEBUG
    DEBUG_BUILD = true;
#endif

    _ITERATIONS = (int)std::round(ITERATIONS * _test_intensity);
    _LIST_SIZE = (int)std::round(LIST_SIZE * _test_intensity);
    _TREE_SIZE = (int)std::round(TREE_SIZE * _test_intensity);
    _TEST_COUNT = TEST_COUNT;
}

// ===================== Main =====================

int main(int argc, char* argv[])
{
    std::cout << "------------------ Test-Architecture-Speed Tool ------------------" << std::endl;
    std::cout << "Compare the performance of this binary when compiled as x64 vs x86" << std::endl;
    std::cout << "------------------------------------------------------------------\n" << std::endl;

    SetTestParams();

    if (argc <= 1) {
        std::cout << "For help, use launch parameter -help  or  /?\n" << std::endl;
    } else {
        ParseArguments(argc, argv);
        if (showHelp)
            std::cout << HELP_TEXT << std::endl;
        std::cout << std::endl;
    }

    bool is64 = (sizeof(void*) == 8);
    std::cout << "Running on " << (is64 ? "64-bit" : "32-bit") << " process" << std::endl;
    std::cout << "Pointer size: " << sizeof(void*) << " bytes" << std::endl;
    std::cout << CheckIfHiResolutionTimer() << std::endl;
    std::cout << "\nRunning performance with parameters:" << std::endl;
    std::cout << "    Iterations: " << _ITERATIONS << std::endl;
    std::cout << "    List size: " << _LIST_SIZE << std::endl;
    std::cout << "    Tree size: " << _TREE_SIZE << std::endl;
    std::cout << "    Test intensity: " << _test_intensity << " (Multiplier on above values)" << std::endl;
    std::cout << "    Test count: " << _TEST_COUNT << "\n" << std::endl;

    if (DEBUG_BUILD)
        std::cout << "WARNING: Non-release compiled version may not produce realistic results due to lack of optimizations.\n" << std::endl;

    for (int i = 0; i < _TEST_COUNT; i++) {
        std::cout << "Test run " << (i + 1) << ":" << std::endl;
        RunAllTests(i + 1);
        std::cout << std::endl;
    }

    SaveJsonToFile(ResultsList);

    std::cout << "Press any key to exit..." << std::endl;
    std::cin.get();

    return 0;
}
