using System;
using UnityEngine;
using UnityEditor.TestTools.TestRunner.Api;
using Assignment.Core.Interfaces;
using Assignment;
using System.Collections;
using System.Collections.Generic;
using Assignment.Core;

namespace AssignmentSystem.Services
{
    /// <summary>
    /// Implementation of ITestResultService using TestResultCapture
    /// </summary>
    public class TestResultService : ITestResultService
    {
        private TestResultCapture _testResultCapture;
        private TestRunnerApi _testRunnerApi;

        public string OutputDirectory => _testResultCapture?.GetCurrentOutputDirPath();
        public string JsonOutputFilePath => _testResultCapture?.GetCurrentJsonOutputFilePath();

        public event Action<TestRunCompleteResult> OnTestCompleted;

        public void InitializeCapture()
        {
            try
            {
                // Unregister previous callbacks if they exist
                if (_testRunnerApi != null && _testResultCapture != null)
                {
                    _testRunnerApi.UnregisterCallbacks(_testResultCapture);
                }

                _testResultCapture = new TestResultCapture(AssignmentConfig.AssignmentTestcaseFiles);
                _testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();

                _testResultCapture.OnRunFinished += () =>
                {
                    OnTestCompleted?.Invoke(new TestRunCompleteResult(
                        true,
                        OutputDirectory,
                        "Tests completed successfully"
                    ));

                    // Unregister callbacks after completion
                    _testRunnerApi.UnregisterCallbacks(_testResultCapture);
                };

                _testRunnerApi.RegisterCallbacks(_testResultCapture);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TestResultService] Failed to initialize test capture: {ex.Message}");
                OnTestCompleted?.Invoke(new TestRunCompleteResult(
                    false,
                    null,
                    $"Failed to initialize test capture: {ex.Message}"
                ));
            }
        }

        public void RunTests(TestMode mode)
        {
            if (_testRunnerApi == null)
            {
                Debug.LogWarning("[TestResultService] Test runner not initialized. Initializing now...");
                InitializeCapture();
            }

            var filter = new Filter { testMode = mode };
            _testRunnerApi.Execute(new ExecutionSettings(filter));
        }

        public bool HasTestResults()
        {
            var jsonPath = JsonOutputFilePath;
            bool hasResults = !string.IsNullOrEmpty(jsonPath) && System.IO.File.Exists(jsonPath);
            return hasResults;
        }

        public void CleanUp()
        {
            Debug.Log("[TestResultService] Destroying instance");
            if (_testResultCapture != null)
            {
                _testRunnerApi.UnregisterCallbacks(_testResultCapture);
            }
        }
    }
}
