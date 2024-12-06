# Test-Architecture-Speed Tool

## Overview

The **Test-Architecture-Speed Tool** is a simple utility to compare the performance of a binary when compiled as x64 vs x86. It runs a series of tests and outputs the results in JSON format.

## Features

- **Performance Tests**: Runs tests including LinkedList traversal, object allocation, string manipulation, binary tree operations, and dictionary operations.
- **Configurable Parameters**: Customize test parameters such as the number of iterations, list size, tree size, test intensity, and the number of test runs.
- **JSON Output**: Saves the test results in JSON format.

## Usage

### Running the Tests

To run the tests, execute both versions of the binary (not at the same time) with optional arguments to customize the test parameters.
  - `Test-Architecture-Speed-x64.exe`
  - `Test-Architecture-Speed-x86.exe`
It will display the results, and create json files with the data to be used with the included comparison tool.

### Comparing the results

You can use the `Results-Comparer.exe` tool to autuomatically read the json results file, and compare the average results. Just click to run the comparison tool with the json files in the same directory.


## Screenshots

<p align="center"> 
<img width="484" alt="image" src="https://github.com/user-attachments/assets/71541bc9-c7fc-4e22-a6d7-03714714e361">
</p>

<p align="center"> 
<img width="777" alt="image" src="https://github.com/user-attachments/assets/27b3d9f6-f975-4e7c-8c07-0ac0d305ff33">
</p>

#### Arguments

- `-iterations <int>`: Number of iterations for the tests (positive integer)
- `-list_size <int>`: Size of the list for the LinkedList traversal test (positive integer)
- `-tree_size <int>`: Size of the binary tree for the Binary Tree operations test (positive integer)
- `-test_intensity <float>`: Intensity multiplier for the tests (positive float)
- `-test_count <int>`: Number of test runs (positive integer)
- `-help` or `/?`: Display help information

#### Example
```
Test-Architecture-Speed.exe -iterations 500000 -list_size 5000 -tree_size 50000 -test_intensity 0.5 -test_count 5
```

## How To Compile:

### Requirements:
 - Only requires Visual Studio 2022
   - At the moment there are no external dependencies beyond the .NET Framework 4.8 built into windows (no need to download anything extra, install any Nuget packages, etc)

### Instructions:
1. Open the "Solution" file (`Test-Architecture-Speed.sln`) with Visual Studio 2022
   - The entire solution/project is included with the repo, so after opening it should be ready to compile and run immediately after opening it
2. Choose the "Release" build configuration mode for actual testing. The debug compilation mode doesn't use compiling optimizations and won't give accurate results.
   - However for development and testing the code itself, you can use the Debug mode. It wll also use a shorter version of the tests.
3. Build both the x86 and x64 version of the binary by selecting each in the dropdown then using: "Build" > "Build Solution"
   - The comparison tool doesn't need separate x86/x64 versions. It will be automatically compiled and placed in the build/release folder along with the main exes when you build those.

