using UnityEngine.Pool;
using UnityEngine;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEditor;
using System.Collections.Generic;
using System;
using Assignment.Core.Interfaces;
using Assignment.Core.DI;
using Assignment;

/// <summary>
/// Improved assignment menu with modular architecture
/// Demonstrates the new service-based approach
/// </summary>
public static class AssignmentMenu
{
    // Constants for menu paths
    private const string MENU_ROOT = "Assignment/";
    private const string MENU_AUTH = MENU_ROOT + "Authentication/";
    private const string MENU_TESTS = MENU_ROOT + "Testing/";
    private const string MENU_SUBMISSION = MENU_ROOT + "Submission/";
    private const string MENU_TOOLS = MENU_ROOT + "Tools/";

    #region Authentication Menu Items

    // // [MenuItem(MENU_AUTH + "Login", priority = 100)]
    public static void ShowLoginDialog()
    {
        Debug.Log("[AssignmentMenu] Opening login dialog...");
        LoginDialogWindow.ShowLoginWindow();
    }

    // // [MenuItem(MENU_AUTH + "Logout", priority = 101)]
    public static void Logout()
    {
        if (EditorUtility.DisplayDialog("Logout",
            "Are you sure you want to logout?", "Yes", "No"))
        {
            WrapServiceContainer.Logout();
            Debug.Log("[AssignmentMenu] User logged out successfully");
        }
    }

    // // [MenuItem(MENU_AUTH + "Check Login Status", priority = 102)]
    public static void CheckLoginStatus()
    {
        bool isLoggedIn = WrapServiceContainer.HasLoggedIn();
        string message = isLoggedIn ?
            "You are currently logged in to the Assignment System." :
            "You are not logged in. Please login first.";

        EditorUtility.DisplayDialog("Login Status", message, "OK");
        Debug.Log($"[AssignmentMenu] Login status checked: {(isLoggedIn ? "Logged In" : "Not Logged In")}");
    }

    // Validation for auth menu items
    // // [MenuItem(MENU_AUTH + "Logout", true)]
    public static bool ValidateLogout()
    {
        return WrapServiceContainer.HasLoggedIn();
    }

    #endregion

    #region Testing Menu Items

    // // [MenuItem(MENU_TESTS + "Run Edit Mode Tests", priority = 200)]
    // public static void RunEditModeTests()
    // {
    //     Debug.Log("[AssignmentMenu] Running Edit Mode tests...");
    //     WrapServiceContainer.RunTestsWithCapture(TestMode.EditMode);

    //     EditorUtility.DisplayDialog("Tests Started",
    //         "Edit Mode tests are now running. Check the console for progress.", "OK");
    // }

    // [MenuItem(MENU_TESTS + "Run Play Mode Tests", priority = 201)]
    public static void RunPlayModeTests()
    {
        Debug.Log("[AssignmentMenu] Running Play Mode tests...");
        WrapServiceContainer.RunTestsWithCapture(TestMode.PlayMode);

        EditorUtility.DisplayDialog("Tests Started",
            "Play Mode tests are now running. Check the console for progress.", "OK");
    }

    // [MenuItem(MENU_TESTS + "Check Test Results", priority = 202)]
    public static void CheckTestResults()
    {
        bool hasResults = WrapServiceContainer.HasTestResults();
        string resultsPath = WrapServiceContainer.GetJsonOutputFilePath();

        if (hasResults)
        {
            string message = $"Test results are available at:\n{resultsPath}\n\nWould you like to open the results folder?";
            if (EditorUtility.DisplayDialog("Test Results Available", message, "Open Folder", "OK"))
            {
                EditorUtility.RevealInFinder(resultsPath);
            }
        }
        else
        {
            EditorUtility.DisplayDialog("No Test Results",
                "No test results found. Please run tests first.", "OK");
        }

        Debug.Log($"[AssignmentMenu] Test results check: {(hasResults ? "Available" : "Not Available")}");
    }

    // [MenuItem(MENU_TESTS + "Open Results Folder", priority = 203)]
    public static void OpenResultsFolder()
    {
        string outputDir = WrapServiceContainer.GetOutputDirPath();
        if (!string.IsNullOrEmpty(outputDir))
        {
            EditorUtility.RevealInFinder(outputDir);
            Debug.Log($"[AssignmentMenu] Opened results folder: {outputDir}");
        }
        else
        {
            EditorUtility.DisplayDialog("No Results Folder",
                "No test results folder found. Please run tests first.", "OK");
        }
    }

    // [MenuItem(MENU_TESTS + "Open Results Folder", true)]
    public static bool ValidateOpenResultsFolder()
    {
        return !string.IsNullOrEmpty(WrapServiceContainer.GetOutputDirPath());
    }

    #endregion

    #region Submission Menu Items
    private static bool isSubmitting = false;
    private static readonly Dictionary<string, Action<TestRunCompleteResult>> _testCompleteCallbacks = new();

    // [MenuItem(MENU_SUBMISSION + "Submit Assignment", priority = 300)]
    public static void SubmitAssignment()
    {
        if (isSubmitting)
        {
            EditorUtility.DisplayDialog("Submission in Progress",
                "A submission is already in progress. Please wait for it to complete.", "OK");
            return;
        }

        var callbackID = Guid.NewGuid().ToString();
        var testCompleteCallback = new Action<TestRunCompleteResult>(result =>
        {
            isSubmitting = false;
            if (!result.Success)
            {
                EditorUtility.DisplayDialog("Tests Failed",
                    $"Play Mode tests failed: {result.Message}", "OK");
                Debug.LogError($"[AssignmentMenu] Play Mode tests failed: {result.Message}");
                return;
            }

            EditorUtility.DisplayDialog("Tests Completed",
                "Play Mode tests completed successfully!", "OK");
            Debug.Log("[AssignmentMenu] Play Mode tests completed successfully");

            if (!ValidateSubmissionPrerequisites(showPopupError: true))
                return;

            if (EditorUtility.DisplayDialog("Submit Assignment",
                "Are you sure you want to submit your assignment? This will upload your test results to the server.",
                "Submit", "Cancel"))
            {
                Debug.Log("[AssignmentMenu] Submitting assignment...");
                WrapServiceContainer.SubmitAssignment((success, message) =>
                {
                    if (success)
                    {
                        EditorUtility.DisplayDialog("Submission Successful",
                            "Your assignment has been submitted successfully!", "OK");
                        Debug.Log($"[AssignmentMenu] Assignment submitted successfully: {message}");
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Submission Failed",
                            $"Failed to submit assignment: {message}", "OK");
                        Debug.LogError($"[AssignmentMenu] Assignment submission failed: {message}");
                    }
                });
            }

            // detach the callback after submission
            WrapServiceContainer.DetachTestCompleteCallback(_testCompleteCallbacks[callbackID]);
            _testCompleteCallbacks.Remove(callbackID);
        });
        _testCompleteCallbacks[callbackID] = testCompleteCallback;

        WrapServiceContainer.AttachTestCompleteCallback(testCompleteCallback);

        WrapServiceContainer.RunTestsWithCapture(TestMode.PlayMode);
    }

    // [MenuItem(MENU_SUBMISSION + "Submit Assignment", true)]
    public static bool ValidateSubmitAssignment()
    {
        return ValidateSubmissionPrerequisites();
    }

    private static bool ValidateSubmissionPrerequisites(bool showPopupError = false)
    {
        // Check if user is logged in
        if (!WrapServiceContainer.HasLoggedIn())
        {
            if (showPopupError)
            {
                EditorUtility.DisplayDialog("Not Logged In",
                    "Please login first before submitting your assignment.", "OK");
            }
            return false;
        }

        // Check if test results exist
        if (!WrapServiceContainer.HasTestResults())
        {
            if (showPopupError)
            {
                EditorUtility.DisplayDialog("No Test Results",
                    "Please run tests first before submitting your assignment.", "OK");
            }
            return false;
        }

        return true;
    }

    #endregion

    #region Tools Menu Items

    // [MenuItem(MENU_TOOLS + "Initialize Test Capture", priority = 400)]
    public static void InitializeTestCapture()
    {
        Debug.Log("[AssignmentMenu] Initializing test capture...");
        WrapServiceContainer.InitializeTestCapture();

        EditorUtility.DisplayDialog("Test Capture Initialized",
            $"Test capture has been initialized. Results will be saved to:\n{WrapServiceContainer.GetOutputDirPath()}", "OK");
    }

    // [MenuItem(MENU_TOOLS + "Clear Test Results", priority = 402)]
    public static void ClearTestResults()
    {
        if (EditorUtility.DisplayDialog("Clear Test Results",
            "Are you sure you want to clear all test results? This action cannot be undone.",
            "Clear", "Cancel"))
        {
            string outputDir = WrapServiceContainer.GetOutputDirPath();
            if (!string.IsNullOrEmpty(outputDir) && System.IO.Directory.Exists(outputDir))
            {
                try
                {
                    System.IO.Directory.Delete(outputDir, true);
                    Debug.Log($"[AssignmentMenu] Test results cleared from: {outputDir}");
                    EditorUtility.DisplayDialog("Results Cleared",
                        "Test results have been cleared successfully.", "OK");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[AssignmentMenu] Failed to clear test results: {ex.Message}");
                    EditorUtility.DisplayDialog("Clear Failed",
                        $"Failed to clear test results: {ex.Message}", "OK");
                }
            }
            else
            {
                EditorUtility.DisplayDialog("No Results to Clear",
                    "No test results found to clear.", "OK");
            }
        }
    }

    // [MenuItem(MENU_TOOLS + "Clear Test Results", true)]
    public static bool ValidateClearTestResults()
    {
        return WrapServiceContainer.HasTestResults();
    }

    // [MenuItem(MENU_TOOLS + "Install Newtonsoft.Json Package", priority = 410)]
    public static void InstallNewtonsoftJsonPackage()
    {
        const string packageName = "com.unity.nuget.newtonsoft-json";
        Debug.Log($"[AssignmentMenu] Installing package: {packageName}");
        UnityEditor.PackageManager.Client.Add(packageName);
        EditorUtility.DisplayDialog("Package Installation Started",
            $"The package '{packageName}' is being installed. Check the Package Manager for progress.", "OK");
    }

    // [MenuItem(MENU_TOOLS + "Install Newtonsoft.Json Package", true)]
    public static bool ValidateInstallNewtonsoftJsonPackage()
    {
        // Only enable if not already installed
        var listRequest = UnityEditor.PackageManager.Client.List(true, false);
        if (listRequest.IsCompleted)
        {
            foreach (var package in listRequest.Result)
            {
                if (package.name == "com.unity.nuget.newtonsoft-json")
                    return false;
            }
        }
        return true;
    }

    #endregion

    #region Developer Tools (only in debug builds)

    // [MenuItem(MENU_TOOLS + "Developer/Force Logout", priority = 500)]
    public static void ForceLogout()
    {
        WrapServiceContainer.Logout();
        Debug.Log("[AssignmentMenu] Force logout executed");
    }

    // [MenuItem(MENU_TOOLS + "Developer/Reset All Settings", priority = 501)]
    public static void ResetAllSettings()
    {
        if (EditorUtility.DisplayDialog("Reset All Settings",
            "This will clear all saved credentials and logout. Continue?", "Yes", "No"))
        {
            LoginCredentialsManager.ClearCredentials();
            WrapServiceContainer.Logout();
            Debug.Log("[AssignmentMenu] All settings reset");
            EditorUtility.DisplayDialog("Settings Reset", "All settings have been reset.", "OK");
        }
    }

    // Only show developer tools in debug builds
    // [MenuItem(MENU_TOOLS + "Developer/Force Logout", true)]
    // [MenuItem(MENU_TOOLS + "Developer/Reset All Settings", true)]
    public static bool ValidateDeveloperTools()
    {
        return Debug.isDebugBuild;
    }

    #endregion

    #region Wrapping Service container function

    private class WrapServiceContainer
    {
        public static void Logout()
        {
            ServiceContainer.Instance.GetService<IAuthenticationService>().Logout();
        }

        public static bool HasLoggedIn()
        {
            return ServiceContainer.Instance.GetService<IAuthenticationService>().IsLoggedIn;
        }

        public static void AttachTestCompleteCallback(Action<TestRunCompleteResult> onTestCompleted)
        {
            var testResultService = ServiceContainer.Instance.GetService<ITestResultService>();
            if (onTestCompleted != null)
            {
                testResultService.OnTestCompleted += onTestCompleted;
                Debug.Log("[WrapServiceContainer] Attached test complete callback");
            }
            else
            {
                Debug.LogWarning("[WrapServiceContainer] Attempted to attach null test complete callback");
            }
        }

        public static void DetachTestCompleteCallback(Action<TestRunCompleteResult> onTestCompleted)
        {
            var testResultService = ServiceContainer.Instance.GetService<ITestResultService>();
            if (onTestCompleted != null)
            {
                testResultService.OnTestCompleted -= onTestCompleted;
                Debug.Log("[WrapServiceContainer] Detached test complete callback");
            }
        }

        public static void RunTestsWithCapture(TestMode mode)
        {
            var testResultService = ServiceContainer.Instance.GetService<ITestResultService>();
            testResultService.RunTests(mode);
        }

        public static string GetJsonOutputFilePath()
        {
            return ServiceContainer.Instance.GetService<ITestResultService>().JsonOutputFilePath;
        }

        public static string GetOutputDirPath()
        {
            return ServiceContainer.Instance.GetService<ITestResultService>().OutputDirectory;
        }

        public static bool HasTestResults()
        {
            return ServiceContainer.Instance.GetService<ITestResultService>().HasTestResults();
        }

        public static void SubmitAssignment(System.Action<bool, string> callback)
        {
            var submissionService = ServiceContainer.Instance.GetService<ISubmissionService>();
            submissionService.SubmitAssignment(result =>
            {
                if (result.Success)
                {
                    callback?.Invoke(true, result.Message);
                }
                else
                {
                    callback?.Invoke(false, result.Message);
                }
            });
        }

        public static void InitializeTestCapture()
        {
            ServiceContainer.Instance.GetService<ITestResultService>().InitializeCapture();
        }
    }
    #endregion

}


