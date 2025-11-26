using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework.Constraints;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// Captures test results and writes them to a file according to Unity Test Framework documentation
/// </summary>
public class TestResultCapture : ICallbacks
{
    private readonly List<TestResult> testResults = new();
    private DateTime runStartTime;
    private readonly string outputDirPath = "";
    private string currentOutputFilePath;
    private string currentJsonOutputFilePath;
    private readonly string[] testcaseFilePaths;

    public event Action OnRunFinished;

    private const string focusNamespace = "Assignment";

    public TestResultCapture(string[] testcaseFilePaths, string outputDirPath_ = "")
    {
        if (!string.IsNullOrEmpty(outputDirPath_))
        {
            outputDirPath = outputDirPath_;
            return;
        }
        outputDirPath = Path.Combine(Application.persistentDataPath, $"test-{DateTime.Now:yyyyMMdd-HHmmss}");
        this.testcaseFilePaths = testcaseFilePaths;
    }

    public void RunStarted(ITestAdaptor testsToRun)
    {
        if (!Directory.Exists(outputDirPath))
        {
            Directory.CreateDirectory(outputDirPath);
        }
        currentOutputFilePath = Path.Combine(outputDirPath, "TestResults.txt");
        currentJsonOutputFilePath = Path.Combine(outputDirPath, "TestResults.json");

        runStartTime = DateTime.Now;
        testResults.Clear();

        if (AssignmentSystemConfig.VERBOSE)
        {
            Debug.Log($"Test run started at {runStartTime:yyyy-MM-dd HH:mm:ss}");
            Debug.Log($"Total tests to run: {CountTests(testsToRun)}");
        }

        // Initialize results file
        WriteToFile($"=== TEST RUN STARTED ===\n");
        WriteToFile($"Start Time: {runStartTime:yyyy-MM-dd HH:mm:ss}\n");
        WriteToFile($"Total Tests: {CountTests(testsToRun)}\n");
        WriteToFile($"Test Suite: {testsToRun.Name}\n\n");
    }

    public void RunFinished(ITestResultAdaptor result)
    {
        var runEndTime = DateTime.Now;
        var duration = runEndTime - runStartTime;

        var summary = GenerateTestSummary(result);

        if (AssignmentSystemConfig.VERBOSE)
        {
            Debug.Log($"Test {result.FullName} run finished at {runEndTime:yyyy-MM-dd HH:mm:ss}");
            Debug.Log($"Duration: {duration.TotalSeconds:F2} seconds");
            Debug.Log($"Results: {summary.PassedCount} passed, {summary.FailedCount} failed, {summary.SkippedCount} skipped");
        }

        // Write summary to file
        WriteToFile($"\n=== TEST RUN FINISHED ===\n");
        WriteToFile($"End Time: {runEndTime:yyyy-MM-dd HH:mm:ss}\n");
        WriteToFile($"Duration: {duration.TotalSeconds:F2} seconds\n");
        WriteToFile($"Total Tests: {summary.TotalCount}\n");
        WriteToFile($"Passed: {summary.PassedCount}\n");
        WriteToFile($"Failed: {summary.FailedCount}\n");
        WriteToFile($"Skipped: {summary.SkippedCount}\n");
        WriteToFile($"Success Rate: {(summary.TotalCount > 0 ? (summary.PassedCount * 100.0 / summary.TotalCount) : 0):F1}%\n\n");

        // Write detailed results
        WriteToFile("=== DETAILED RESULTS ===\n");
        WriteTestResults(result, 0);

        // make md5 hash of the testcase file
        WriteToFile("\n=== TEST CASE FILES COMBINED MD5 HASHES ===\n");
        var md5Hash = CalculateCombinedTestcaseFilesHash(testcaseFilePaths);
        WriteToFile($"Combined MD5 Hash of Test Case Files: {md5Hash}\n");
        WriteToFile("\n=== END OF TEST CASE FILES MD5 HASHES ===\n");

        // Save JSON results
        SaveJsonResults(result, runEndTime, duration, summary, md5Hash);

        if (AssignmentSystemConfig.VERBOSE)
        {
            Debug.Log($"Test results written to: {currentOutputFilePath}");
            Debug.Log($"JSON test results written to: {currentJsonOutputFilePath}");
        }

        if (GetShowDialogAfterTestRun())
        {
            var dialogContent = string.Join("\n",
                $"Test run finished at {runEndTime:yyyy-MM-dd HH:mm:ss}",
                $"Duration: {duration.TotalSeconds:F2} seconds",
                $"Total Tests: {summary.TotalCount}",
                $"Passed: {summary.PassedCount}",
                $"Failed: {summary.FailedCount}",
                $"Skipped: {summary.SkippedCount}",
                $"Success Rate: {(summary.TotalCount > 0 ? (summary.PassedCount * 100.0 / summary.TotalCount) : 0):F1}%",
                $"Test results written to: {currentOutputFilePath}",
                $"JSON results written to: {currentJsonOutputFilePath}"
            );
            EditorUtility.DisplayDialog("Test Results", dialogContent, "OK");

            SetShowDialogAfterTestRun(false);
            PlayerPrefs.Save();
        }

        // Invoke the run finished event if there are any subscribers
        this.OnRunFinished?.Invoke();
    }

    public static void SetShowDialogAfterTestRun(bool set)
    {
        PlayerPrefs.SetInt("test-result-capture.show-dialog", set ? 1 : 0);
    }

    public static bool GetShowDialogAfterTestRun()
    {
        return PlayerPrefs.GetInt("test-result-capture.show-dialog", 1) == 1;
    }

    public string GetCurrentOutputDirPath()
    {
        return outputDirPath;
    }
    public string GetCurrentOutputFilePath()
    {
        return currentOutputFilePath;
    }

    public string GetCurrentJsonOutputFilePath()
    {
        return currentJsonOutputFilePath;
    }

    public void TestStarted(ITestAdaptor test)
    {
        if (AssignmentSystemConfig.VERBOSE)
        {
            Debug.Log($"Test started: {test.FullName}");
        }
    }

    public void TestFinished(ITestResultAdaptor result)
    {
        var ns = result.Test.TypeInfo?.Namespace ?? "";
        if (!result.HasChildren && ns == focusNamespace)
        {
            var testResult = new TestResult
            {
                Namespace = ns,
                TestName = result.Test.Name,
                FullName = result.Test.FullName,
                ResultState = result.ResultState,
                TestStatus = result.TestStatus,
                Duration = result.Duration,
                Message = result.Message,
                StackTrace = result.StackTrace,
                StartTime = DateTime.Now.AddSeconds(-result.Duration),
                EndTime = DateTime.Now
            };

            testResults.Add(testResult);

            // Log individual test result
            var status = result.ResultState == "Passed" ? "✓" : "✗";
            if (AssignmentSystemConfig.VERBOSE)
            {
                Debug.Log($"{status} {result.Test.Name}: {result.ResultState} ({result.Duration:F3}s)");
                if (result.ResultState != "Passed")
                {
                    Debug.LogError($"Test Failed: {result.Test.Name}\nMessage: {result.Message}\nStack: {result.StackTrace}");
                }
            }
        }
    }

    private string CalculateCombinedTestcaseFilesHash(string[] testcaseFilePaths_)
    {
        var combinedContent = "";
        try
        {
            if (testcaseFilePaths_ != null && testcaseFilePaths_.Length > 0)
            {

                foreach (var filePath in testcaseFilePaths_)
                {
                    if (File.Exists(filePath))
                    {
                        combinedContent += File.ReadAllText(filePath);
                    }
                    else
                    {
                        throw new FileNotFoundException($"Test case file not found: {filePath}");
                    }
                }
            }
            else
            {
                return "<empty>";
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to generate MD5 hash for test case file: {ex.Message}");
        }

        using var md5Final = System.Security.Cryptography.MD5.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(combinedContent);
        var finalHash = md5Final.ComputeHash(bytes);
        var finalHashString = BitConverter.ToString(finalHash).Replace("-", "").ToLowerInvariant();
        return finalHashString;
    }

    private int CountTests(ITestAdaptor test)
    {
        if (AssignmentSystemConfig.VERBOSE)
        {
            Debug.Log($"Counting tests in: {test.FullName} namespace: {test.TypeInfo?.Namespace} children: {test.Children.Count()} testcases: {test.TestCaseCount}");
        }
        if (!test.HasChildren)
        {
            if (test.TypeInfo != null && test.TypeInfo.Namespace == focusNamespace) return 1;
            return 0;
        }

        int count = 0;
        foreach (var child in test.Children)
        {
            count += CountTests(child);
        }
        return count;
    }

    private TestSummary GenerateTestSummary(ITestResultAdaptor result)
    {
        var summary = new TestSummary();
        CountResults(result, summary);
        return summary;
    }

    private void CountResults(ITestResultAdaptor result, TestSummary summary)
    {
        if (!result.HasChildren)
        {
            if (AssignmentSystemConfig.VERBOSE)
            {
                Debug.Log($"Counting result: {result.Test.Name} state: {result.ResultState} namespace: {result.Test.TypeInfo?.Namespace}");
            }
            if (result.Test.TypeInfo != null && result.Test.TypeInfo.Namespace != focusNamespace) return;
            summary.TotalCount++;
            switch (result.ResultState)
            {
                case "Passed":
                    summary.PassedCount++;
                    break;
                case "Failed":
                case "Error":
                    summary.FailedCount++;
                    break;
                default:
                    summary.SkippedCount++;
                    break;
            }
        }
        else
        {
            foreach (var child in result.Children)
            {
                CountResults(child, summary);
            }
        }
    }

    private void WriteTestResults(ITestResultAdaptor result, int indent)
    {
        var indentStr = new string(' ', indent * 2);

        if (!result.HasChildren)
        {
            var status = result.ResultState == "Passed" ? "PASS" : "FAIL";
            WriteToFile($"{indentStr}[{status}] {result.Test.Name} ({result.Duration:F3}s)\n");

            if (result.ResultState != "Passed" && !string.IsNullOrEmpty(result.Message))
            {
                WriteToFile($"{indentStr}  Message: {result.Message}\n");
                if (!string.IsNullOrEmpty(result.StackTrace))
                {
                    WriteToFile($"{indentStr}  Stack Trace:\n{indentStr}    {result.StackTrace.Replace("\n", $"\n{indentStr}    ")}\n");
                }
            }
        }
        else
        {
            WriteToFile($"{indentStr}{result.Test.Name}:\n");
            foreach (var child in result.Children)
            {
                WriteTestResults(child, indent + 1);
            }
        }
    }

    private void WriteToFile(string content)
    {
        try
        {
            File.AppendAllText(currentOutputFilePath, content);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to write to test results file: {ex.Message}");
        }
    }

    private void SaveJsonResults(ITestResultAdaptor result, DateTime runEndTime, TimeSpan duration, TestSummary summary, string md5Hash)
    {
        try
        {
            var jsonResult = new TestRunResult
            {
                TestRunInfo = new TestRunInfo
                {
                    StartTime = runStartTime,
                    EndTime = runEndTime,
                    Duration = duration.TotalSeconds,
                    TestSuite = result.Test.Name
                },
                Summary = new TestSummaryJson
                {
                    TotalCount = summary.TotalCount,
                    PassedCount = summary.PassedCount,
                    FailedCount = summary.FailedCount,
                    SkippedCount = summary.SkippedCount,
                    SuccessRate = summary.TotalCount > 0 ? (summary.PassedCount * 100.0 / summary.TotalCount) : 0
                },
                TestResults = testResults.ToArray(),
                TestCaseFilesHash = md5Hash
            };
            var json = JsonConvert.SerializeObject(jsonResult, Formatting.Indented);
            File.WriteAllText(currentJsonOutputFilePath, json);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to write JSON test results: {ex.Message}");
        }
    }
}
