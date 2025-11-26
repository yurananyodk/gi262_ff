using System;
using UnityEditor.TestTools.TestRunner.Api;

namespace Assignment.Core.Interfaces
{
    /// <summary>
    /// Interface for handling test result operations
    /// </summary>
    public interface ITestResultService
    {
        /// <summary>
        /// Gets the current output directory for test results
        /// </summary>
        string OutputDirectory { get; }

        /// <summary>
        /// Gets the current JSON output file path for test results
        /// </summary>
        string JsonOutputFilePath { get; }

        /// <summary>
        /// Initializes the test result capture system
        /// </summary>
        void InitializeCapture();

        /// <summary>
        /// Runs tests with the specified mode
        /// </summary>
        /// <param name="mode">Test mode (EditMode or PlayMode)</param>
        void RunTests(TestMode mode);

        /// <summary>
        /// Checks if test results are available
        /// </summary>
        /// <returns>True if test results exist</returns>
        bool HasTestResults();

        /// <summary>
        /// Event fired when test run completes
        /// </summary>
        event Action<TestRunCompleteResult> OnTestCompleted;
    }

    /// <summary>
    /// Result of a test run operation
    /// </summary>
    public class TestRunCompleteResult
    {
        public bool Success { get; set; }
        public string OutputPath { get; set; }
        public string Message { get; set; }

        public TestRunCompleteResult(bool success, string outputPath = null, string message = null)
        {
            Success = success;
            OutputPath = outputPath;
            Message = message;
        }
    }
}
