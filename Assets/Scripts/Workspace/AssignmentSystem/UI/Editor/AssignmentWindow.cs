using UnityEngine;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using System.Collections.Generic;
using System.Linq;
using System;
using Assignment.Core.DI;
using Assignment.Core.Interfaces;
using AssignmentSystem.Services;
using System.IO;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Reflection;
using Assignment.Core;

namespace Assignment.UI
{
    public class AssignmentWindow : EditorWindow
    {
        // UI State
        private Vector2 testScrollPos;
        private Vector2 mainScrollPos;
        private bool testsLoaded = false;
        private bool isRunningTests = false;
        private string submitStatus = "";
        private string password = "";
        private string studentId = "";
        private bool rememberMe = false;
        private bool isProcessingLogin = false;

        // Registration fields
        private string registerStudentId = "";
        private bool isProcessingRegistration = false;

        // Test Data
        private List<TestInfo> testInfos = new();
        private Dictionary<string, TestStatus> testResults = new();
        private string recentTestResultJSONOutputPath = "";

        // Services
        private IAuthenticationService authService;
        private ITestResultService testResultService;
        private ISubmissionService submissionService;
        private ICredentialsManager credentialsManager;

        // UI Constants
        private readonly Color passedColor = new(0.2f, 0.8f, 0.2f);
        private readonly Color failedColor = new(0.8f, 0.2f, 0.2f);
        private readonly Color notRunColor = new(0.6f, 0.6f, 0.6f);

        private const string PPF_RecentTestResultJSONOutputPath = "assignmentWindow.recentTestResultJSONOutputPath";

        [MenuItem("Assignment/Assignment Window")]
        public static void ShowWindow()
        {
            var window = GetWindow<AssignmentWindow>("Assignment Management");
            window.minSize = new Vector2(500, 600);
        }

        private void OnEnable()
        {
            InitializeServices();
            LoadTestInformation();
            LoadSavedCredentials();

            // Subscribe to test completion events
            if (testResultService != null)
            {
                testResultService.OnTestCompleted += OnTestCompleted;
            }
        }

        private void OnDisable()
        {
            // Unsubscribe from events
            if (testResultService != null)
            {
                testResultService.OnTestCompleted -= OnTestCompleted;
            }
        }

        private void OnGUI()
        {
            mainScrollPos = EditorGUILayout.BeginScrollView(mainScrollPos);

            DrawHeader();
            EditorGUILayout.Space(10);

            DrawLoginSection();
            EditorGUILayout.Space(15);

            DrawTestRunnerSection();
            EditorGUILayout.Space(15);

            DrawSubmitSection();

            EditorGUILayout.EndScrollView();
        }

        private void DrawLoginSection()
        {
            EditorGUILayout.LabelField("Authentication", EditorStyles.largeLabel);

            if (authService != null && authService.IsLoggedIn)
            {
                DrawLogoutWidget();
            }
            else
            {
                using (new EditorGUILayout.HorizontalScope(GUILayout.ExpandWidth(false)))
                {
                    // Login Section
                    using (new EditorGUILayout.VerticalScope("box", GUILayout.Width(350)))
                    {
                        DrawLoginWidget();
                    }

                    GUILayout.Space(10);

                    // Register Section
                    using (new EditorGUILayout.VerticalScope("box", GUILayout.Width(350)))
                    {
                        DrawRegisterWidget();
                    }

                    GUILayout.FlexibleSpace();
                }
            }
        }

        private void DrawLoginWidget()
        {
            EditorGUILayout.LabelField("Login", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            if (authService == null || !authService.IsLoggedIn)
            {
                EditorGUILayout.LabelField("Please login to access assignment features:");
                EditorGUILayout.Space(5);

                // Student ID field
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Student ID:", GUILayout.Width(80));
                studentId = EditorGUILayout.TextField(studentId, GUILayout.Width(200));
                EditorGUILayout.EndHorizontal();

                // Password field
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Password:", GUILayout.Width(80));
                password = EditorGUILayout.PasswordField(password, GUILayout.Width(200));
                EditorGUILayout.EndHorizontal();

                // Remember me checkbox
                EditorGUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(80); // Align with other fields
                rememberMe = EditorGUILayout.Toggle(rememberMe, GUILayout.Width(20));
                EditorGUILayout.LabelField("Remember me", GUILayout.Width(180));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(10);

                // Login button
                EditorGUI.BeginDisabledGroup(isProcessingLogin || string.IsNullOrEmpty(studentId) || string.IsNullOrEmpty(password));

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                string buttonText = isProcessingLogin ? "Logging in..." : "Login";
                if (GUILayout.Button(buttonText, GUILayout.Height(30), GUILayout.Width(100)))
                {
                    PerformLogin();
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                EditorGUI.EndDisabledGroup();

                // Show validation messages
                if (!string.IsNullOrEmpty(submitStatus) && submitStatus.Contains("failed"))
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.HelpBox(submitStatus, MessageType.Error);
                }
            }
            else
            {
                EditorGUILayout.LabelField($"Status: Logged in", EditorStyles.boldLabel);
                EditorGUILayout.Space(5);

                if (GUILayout.Button("Logout", GUILayout.Height(30)))
                {
                    Logout();
                }
            }
        }

        private void DrawLogoutWidget()
        {
            if (authService != null && authService.AuthToken != null)
            {
                var studentId = authService.AuthStudentId;
                EditorGUILayout.LabelField($"Status: Logged in as {studentId}", EditorStyles.boldLabel);
                EditorGUILayout.Space(5);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Logout", GUILayout.Height(30)))
                    {
                        Logout();
                    }

                    GUILayout.Space(10);

                    // Open grading portal button
                    if (GUILayout.Button("Open Grading Portal", GUILayout.Height(30)))
                    {
                        try
                        {
                            var username = authService.AuthStudentId ?? string.Empty;
                            var url = $"https://grading-system-bu.happygocoding.com?user={Uri.EscapeDataString(username)}";
                            Application.OpenURL(url);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"[AssignmentWindow] Failed to open grading portal: {ex.Message}");
                            EditorUtility.DisplayDialog("Error", "Could not open grading portal.", "OK");
                        }
                    }
                }
            }
        }

        private void DrawRegisterWidget()
        {
            EditorGUILayout.LabelField("Don't have an account? Register below.", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            if (authService == null || !authService.IsLoggedIn)
            {
                EditorGUILayout.LabelField("Create a new account:");
                EditorGUILayout.Space(5);

                // Student ID field for registration
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Student ID:", GUILayout.Width(80));
                registerStudentId = EditorGUILayout.TextField(registerStudentId, GUILayout.Width(200));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(10);

                // Register button
                EditorGUI.BeginDisabledGroup(isProcessingRegistration || string.IsNullOrEmpty(registerStudentId));

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                string buttonText = isProcessingRegistration ? "Registering..." : "Register";
                if (GUILayout.Button(buttonText, GUILayout.Height(30), GUILayout.Width(100)))
                {
                    PerformRegistration();
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                EditorGUI.EndDisabledGroup();

                // Show registration info
                EditorGUILayout.Space(5);
                EditorGUILayout.HelpBox("Registration will create a new student account.", MessageType.Info);
            }

            // Show validation messages
            if (!string.IsNullOrEmpty(submitStatus) && submitStatus.Contains("failed"))
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.HelpBox(submitStatus, MessageType.Error);
            }
        }

        private enum TestFilterType { All, Passed, Failed }
        private TestFilterType testFilterType = TestFilterType.All;

        private void DrawTestRunnerSection()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginDisabledGroup(isRunningTests);
                EditorGUILayout.LabelField("Test Runner", EditorStyles.largeLabel);
                EditorGUILayout.Space(10, true);
                // if (GUILayout.Button("Run All Tests"))
                // {
                //     RunAllTests();
                // }
                // if (GUILayout.Button("Run Edit Mode Tests"))
                // {
                //     RunEditModeTests();
                // }

                if (GUILayout.Button("Run Play Mode Tests"))
                {
                    RunPlayModeTests();
                }
                EditorGUI.EndDisabledGroup();
            }

            if (!string.IsNullOrEmpty(recentTestResultJSONOutputPath))
            {
                EditorGUILayout.Space(10);
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Recent Test Result:", EditorStyles.boldLabel);
                    // Trim the path for display: keep filename + extension, trim directory with ellipsis if too long
                    string displayPath = recentTestResultJSONOutputPath;
                    if (!string.IsNullOrEmpty(displayPath))
                    {
                        string fileName = Path.GetFileName(displayPath);
                        string dirName = Path.GetDirectoryName(displayPath);
                        string trimmedDir = dirName;

                        int maxLength = 50;
                        int minDirLength = 10;
                        int ellipsisCount = 3;
                        string ellipsis = new('.', ellipsisCount);

                        // Calculate allowed length for directory
                        int allowedDirLength = maxLength - fileName.Length - 1; // 1 for separator

                        if (displayPath.Length > maxLength && dirName.Length > minDirLength)
                        {
                            if (allowedDirLength > minDirLength)
                            {
                                trimmedDir = ellipsis + dirName[(dirName.Length - allowedDirLength + ellipsisCount)..];
                            }
                            else
                            {
                                trimmedDir = ellipsis;
                            }
                        }

                        displayPath = $"{trimmedDir}{Path.DirectorySeparatorChar}{fileName}";
                    }
                    EditorGUILayout.Space(10, true);
                    if (GUILayout.Button(displayPath, EditorStyles.linkLabel))
                    {
                        if (File.Exists(recentTestResultJSONOutputPath))
                        {
                            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(recentTestResultJSONOutputPath, 1);
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("File Not Found", $"Could not find file: {recentTestResultJSONOutputPath}", "OK");
                        }
                    }
                }
            }

            using (new EditorGUILayout.VerticalScope("box"))
            {
                // Control buttons

                EditorGUILayout.Space(5);

                // Test results display
                if (isRunningTests)
                {
                    EditorGUILayout.HelpBox("Running tests...", MessageType.Info);
                }

                // Results summary
                if (testResults.Count > 0)
                {
                    var passed = testResults.Values.Count(t => t == TestStatus.Passed);
                    var failed = testResults.Values.Count(t => t == TestStatus.Failed);
                    var total = testResults.Count;

                    EditorGUILayout.LabelField($"Results: {passed} passed, {failed} failed, {total} total");
                }

                // Filter toolbar
                EditorGUILayout.Space(5);
                testFilterType = (TestFilterType)GUILayout.Toolbar((int)testFilterType, new[] { "All", "Passed", "Failed" }, GUILayout.Height(25));

                // Test list
                testScrollPos = EditorGUILayout.BeginScrollView(testScrollPos, GUILayout.Height(200));

                IEnumerable<TestInfo> filteredTests = testInfos;
                filteredTests = testFilterType switch
                {
                    TestFilterType.Passed => testInfos.Where(t => testResults.ContainsKey(t.Name) && testResults[t.Name] == TestStatus.Passed),
                    TestFilterType.Failed => testInfos.Where(t => testResults.ContainsKey(t.Name) && testResults[t.Name] == TestStatus.Failed),
                    _ => testInfos,
                };
                foreach (var testInfo in filteredTests)
                {
                    DrawTestItem(testInfo);
                }

                if (!filteredTests.Any())
                {
                    EditorGUILayout.HelpBox("No tests found for the selected filter.", MessageType.Warning);
                }

                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawSubmitSection()
        {
            EditorGUILayout.LabelField("Assignment Submission", EditorStyles.largeLabel);

            using (new EditorGUILayout.VerticalScope("box"))
            {
                bool canSubmit = CanSubmitAssignment();

                if (!canSubmit)
                {
                    if (authService == null || !authService.IsLoggedIn)
                    {
                        EditorGUILayout.HelpBox("Please login first to submit assignment.", MessageType.Warning);
                    }
                    else if (!HasTestResults())
                    {
                        EditorGUILayout.HelpBox("Please run tests first before submitting.", MessageType.Warning);
                    }
                }

                EditorGUI.BeginDisabledGroup(!canSubmit);
                if (GUILayout.Button("Submit Assignment", GUILayout.Height(40)))
                {
                    SubmitAssignment();
                }
                EditorGUI.EndDisabledGroup();

                if (!string.IsNullOrEmpty(submitStatus))
                {
                    var messageType = submitStatus.Contains("success") ? MessageType.Info : MessageType.Error;
                    EditorGUILayout.HelpBox(submitStatus, messageType);
                }
            }
        }

        // Helper methods
        private void InitializeServices()
        {
            try
            {
                authService = ServiceContainer.Instance.GetService<IAuthenticationService>();
                testResultService = ServiceContainer.Instance.GetService<ITestResultService>();
                submissionService = ServiceContainer.Instance.GetService<ISubmissionService>();
                credentialsManager = ServiceContainer.Instance.GetService<ICredentialsManager>();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AssignmentWindow] Failed to initialize services: {ex.Message}");
            }
        }

        private void LoadTestInformation()
        {
            testInfos.Clear();

            try
            {
                // Try to discover tests from Unity Test Runner API
                var testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();
                var filter = new Filter()
                {
                    testMode = TestMode.EditMode | TestMode.PlayMode,
                    testNames = new string[0]
                };

                // Get tests using reflection or hardcoded list as fallback
                var discoveredTests = DiscoverTestsFromAssembly();

                if (discoveredTests.Count > 0)
                {
                    testInfos.AddRange(discoveredTests);
                }
                else
                {
                    // Fallback to hardcoded test names
                    Debug.LogWarning("[AssignmentWindow] No tests discovered via reflection, using hardcoded list");
                }

                testsLoaded = true;
                Debug.Log($"[AssignmentWindow] Loaded {testInfos.Count} tests");

#if UNITY_EDITOR
                DestroyImmediate(testRunnerApi);
#else
                Destroy(testRunnerApi);
#endif
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AssignmentWindow] Failed to load test information: {ex.Message}");
            }
        }

        private List<TestInfo> DiscoverTestsFromAssembly()
        {
            var tests = new List<TestInfo>();

            try
            {
                // Use reflection to find test methods in Assignment_Testcase
                var testcaseType = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .FirstOrDefault(t => t.Name == "Assignment_Testcase" && t.Namespace == "Assignment");
                if (testcaseType != null)
                {
                    var methods = testcaseType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                    foreach (var method in methods)
                    {
                        // Check for NUnit Test attributes
                        var testAttributes = method.GetCustomAttributes(typeof(NUnit.Framework.TestAttribute), false);
                        var testCaseAttributes = method.GetCustomAttributes(typeof(NUnit.Framework.TestCaseAttribute), false);

                        if (testAttributes.Length > 0 || testCaseAttributes.Length > 0)
                        {
                            if (testCaseAttributes.Length > 0)
                            {
                                // Handle TestCase attributes
                                foreach (var attr in testCaseAttributes.Cast<NUnit.Framework.TestCaseAttribute>())
                                {
                                    tests.Add(new TestInfo
                                    {
                                        Name = attr.TestName ?? method.Name,
                                        FullName = $"{testcaseType.Name}.{method.Name}",
                                        Category = GetTestCategory(method.Name)
                                    });
                                }
                            }
                            else
                            {
                                tests.Add(new TestInfo
                                {
                                    Name = method.Name,
                                    FullName = $"{testcaseType.Name}.{method.Name}",
                                    Category = GetTestCategory(method.Name)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[AssignmentWindow] Could not discover tests via reflection: {ex.Message}");
            }

            return tests;
        }

        private void DrawHeader()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Assignment Management", EditorStyles.largeLabel);
                GUILayout.FlexibleSpace();

                // Quick action buttons
                if (GUILayout.Button("Open Menu", GUILayout.Width(80)))
                {
                    // Show context menu with all assignment options
                    ShowQuickActionsMenu();
                }

                // Status indicators
                if (authService?.IsLoggedIn == true)
                {
                    GUI.color = passedColor;
                    GUILayout.Label("● Logged In", EditorStyles.boldLabel);
                }
                else
                {
                    GUI.color = failedColor;
                    GUILayout.Label("● Not Logged In", EditorStyles.boldLabel);
                }
                GUI.color = Color.white;
            }

            // Assignment info
            using (new EditorGUILayout.HorizontalScope())
            {
                try
                {
                    EditorGUILayout.LabelField($"Assignment: {AssignmentConfig.AssignmentName} v{AssignmentConfig.AssignmentVersion}");
                }
                catch
                {
                    EditorGUILayout.LabelField("Assignment");
                }
                GUILayout.FlexibleSpace();

                if (HasTestResults())
                {
                    var passed = testResults.Values.Count(t => t == TestStatus.Passed);
                    var total = testResults.Count;
                    EditorGUILayout.LabelField($"Tests: {passed}/{total}", EditorStyles.miniLabel);
                }
            }

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        private void ShowQuickActionsMenu()
        {
            var menu = new GenericMenu();

            // Authentication actions
            if (authService?.IsLoggedIn != true)
            {
                menu.AddItem(new GUIContent("Authentication/Login"), false, () =>
                {
                    LoginDialogWindow.ShowLoginWindow();
                });
            }
            else
            {
                menu.AddItem(new GUIContent("Authentication/Logout"), false, () =>
                {
                    Logout();
                });
                menu.AddItem(new GUIContent("Authentication/Check Status"), false, () =>
                {
                    EditorUtility.DisplayDialog("Login Status", "You are currently logged in.", "OK");
                });
            }

            menu.AddSeparator("");

            // Test actions
            menu.AddItem(new GUIContent("Testing/Run Play Mode Tests"), false, () => RunPlayModeTests());
            menu.AddItem(new GUIContent("Testing/Open Results Folder"), false, () =>
            {
                var outputDir = testResultService?.OutputDirectory;
                if (!string.IsNullOrEmpty(outputDir))
                {
                    EditorUtility.RevealInFinder(outputDir);
                }
            });

            const bool showOtherMenu = false;
            if (showOtherMenu)
            {
                menu.AddSeparator("");

                // Submission actions
                bool canSubmit = CanSubmitAssignment();
                if (canSubmit)
                {
                    menu.AddItem(new GUIContent("Submission/Submit Assignment"), false, () => SubmitAssignment());
                }
                else
                {
                    menu.AddDisabledItem(new GUIContent("Submission/Submit Assignment"));
                }

                menu.AddSeparator("");

                // Tools
                menu.AddItem(new GUIContent("Tools/View Assignment Info"), false, () =>
                {
                    EditorUtility.DisplayDialog("Assignment Info", "Assignment 01 v1.0.0\nBangkok University Assignment System", "OK");
                });
                menu.AddItem(new GUIContent("Tools/Validate Configuration"), false, () =>
                {
                    EditorUtility.DisplayDialog("Configuration", "Assignment configuration is valid.", "OK");
                });
                menu.AddItem(new GUIContent("Tools/Clear Test Results"), false, () => ClearResults());
            }
            menu.ShowAsContext();
        }

        private void DrawTestItem(TestInfo testInfo)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                // Test icon/status
                var status = testResults.ContainsKey(testInfo.Name) ? testResults[testInfo.Name] : TestStatus.NotRun;
                var statusText = GetStatusSymbol(status);
                var statusColor = GetStatusColor(status);

                var originalColor = GUI.color;
                GUI.color = statusColor;
                GUILayout.Label(statusText, GUILayout.Width(20));
                GUI.color = originalColor;

                // Test name
                GUILayout.Label(testInfo.Name, GUILayout.Width(300));

                // Category
                GUILayout.Label($"[{testInfo.Category}]", EditorStyles.miniLabel, GUILayout.Width(100));

                // Status text
                GUILayout.Label(status.ToString(), GUILayout.Width(80));

                // Open in VS Code button
                if (GUILayout.Button("Open Test File", GUILayout.Width(120)))
                {
                    string filePath = FindTestcaseFilePath(testInfo);
                    int lineNumber = FindTestcaseLineNumber(filePath, testInfo);
                    if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                    {
                        UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(filePath, lineNumber);
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("File Not Found", $"Could not find file for test: {testInfo.Name}", "OK");
                    }
                }
            }
        }

        private void OnTestCompleted(Assignment.Core.Interfaces.TestRunCompleteResult result)
        {
            isRunningTests = false;

            if (result.Success)
            {
                Debug.Log($"[AssignmentWindow] Tests completed successfully ...loading result from {testResultService.OutputDirectory}");

                var resultJsonFilePath = Path.Combine(testResultService.OutputDirectory, "TestResults.json");
                EditorPrefs.SetString(PPF_RecentTestResultJSONOutputPath, resultJsonFilePath);

                if (!File.Exists(resultJsonFilePath))
                {
                    foreach (var testInfo in testInfos)
                    {
                        testResults[testInfo.Name] = TestStatus.Failed;
                    }
                    return;
                }

                LoadRecentTestResult();
            }

            Repaint();
        }

        private void LoadRecentTestResult()
        {
            var resultJsonFilePath = EditorPrefs.GetString(PPF_RecentTestResultJSONOutputPath, "");
            if (string.IsNullOrEmpty(resultJsonFilePath) || !File.Exists(resultJsonFilePath))
            {
                Debug.LogWarning($"[AssignmentWindow] No recent test result found or file does not exist: {resultJsonFilePath}");
                return;
            }

            // store for display on UI
            recentTestResultJSONOutputPath = resultJsonFilePath;

            string jsonContent = File.ReadAllText(resultJsonFilePath);
            var testResultData = JsonConvert.DeserializeObject<TestRunResult>(jsonContent);

            foreach (var testInfo in testInfos)
            {
                var status = TestStatus.NotRun;
                foreach (var testResult in testResultData.TestResults)
                {
                    if (testResult.TestName == testInfo.Name)
                    {
                        status = testResult.TestStatus switch
                        {
                            UnityEditor.TestTools.TestRunner.Api.TestStatus.Passed => TestStatus.Passed,
                            UnityEditor.TestTools.TestRunner.Api.TestStatus.Failed => TestStatus.Failed,
                            _ => TestStatus.NotRun,
                        };
                        break;
                    }
                }
                testResults[testInfo.Name] = status;
                if (AssignmentSystemConfig.VERBOSE)
                {
                    Debug.Log($"[AssignmentWindow] Test {testInfo.Name} status: {status}");
                }
            }
        }

        // Implementation of functionality methods
        private void LoadSavedCredentials()
        {
            if (credentialsManager != null)
            {
                try
                {
                    if (credentialsManager.LoadCredentials(out string savedStudentId, out string savedPassword))
                    {
                        studentId = savedStudentId;
                        password = savedPassword;
                        rememberMe = true;
                        Debug.Log("[AssignmentWindow] Loaded saved credentials");

                        // If we have saved credentials and not already logged in, show a hint
                        if (authService != null && !authService.IsLoggedIn)
                        {
                            submitStatus = "Saved credentials loaded. Click Login to authenticate.";
                        }
                    }
                    else
                    {
                        rememberMe = credentialsManager.IsRememberMeEnabled();
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[AssignmentWindow] Failed to load saved credentials: {ex.Message}");
                }
            }
        }

        private bool ValidateLoginInput()
        {
            if (string.IsNullOrEmpty(studentId) || string.IsNullOrEmpty(password))
            {
                submitStatus = "Please enter both Student ID and Password";
                return false;
            }
            return true;
        }

        private void PerformLogin()
        {
            if (!ValidateLoginInput())
                return;

            isProcessingLogin = true;
            submitStatus = "Logging in...";

            authService.Login(studentId, password, result =>
            {
                isProcessingLogin = false;

                if (result.Success)
                {
                    HandleSuccessfulLogin(result);
                }
                else
                {
                    HandleFailedLogin(result);
                }

                Repaint();
            });
        }

        private void HandleSuccessfulLogin(AuthResult result)
        {
            // Save credentials if remember me is checked
            if (credentialsManager != null && rememberMe)
            {
                try
                {
                    credentialsManager.SaveCredentials(studentId, password, rememberMe);
                    Debug.Log("[AssignmentWindow] Saved credentials for future use");
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[AssignmentWindow] Failed to save credentials: {ex.Message}");
                }
            }

            submitStatus = "Login successful";

            // Clear password field for security (keep studentId for convenience)
            password = "";

            Debug.Log($"[AssignmentWindow] Successfully logged in with token: {result.Token}");
        }

        private void HandleFailedLogin(AuthResult result)
        {
            string message = result.Message ?? "Login failed. Please check your credentials and try again.";
            submitStatus = $"Login failed: {message}";
            Debug.LogError($"[AssignmentWindow] Login failed: {result.Message}");
        }

        // Registration methods
        private bool ValidateRegistrationInput()
        {
            if (string.IsNullOrEmpty(registerStudentId))
            {
                submitStatus = "Please enter a Student ID for registration";
                return false;
            }

            // Add basic validation for student ID format if needed
            if (registerStudentId.Length < 3)
            {
                submitStatus = "Student ID must be at least 3 characters long";
                return false;
            }

            return true;
        }

        private void PerformRegistration()
        {
            if (!ValidateRegistrationInput())
                return;

            isProcessingRegistration = true;
            submitStatus = "Registering...";

            Debug.Log($"[AssignmentWindow] Starting registration for Student ID: {registerStudentId}");

            authService.Signup(registerStudentId, result =>
            {
                isProcessingRegistration = false;

                if (result.Success)
                {
                    HandleRegistrationResult(true, result.Message);
                }
                else
                {
                    HandleRegistrationResult(false, result.Message);
                }

                Repaint();
            });
        }

        private void HandleRegistrationResult(bool success, string message)
        {
            isProcessingRegistration = false;

            if (success)
            {
                submitStatus = "Registration successful! You are now logged in.";
                registerStudentId = ""; // Clear the field

                // Since signup automatically logs the user in, we should update the login fields
                studentId = ""; // Clear login fields since user is now logged in
                password = "";

                Debug.Log($"[AssignmentWindow] Registration successful and user is now logged in");
            }
            else
            {
                submitStatus = $"Registration failed: {message}";
                Debug.LogError($"[AssignmentWindow] Registration failed: {message}");
            }
        }

        private void Login(string studentId, string password)
        {
            // This method is kept for backward compatibility
            // but now uses the enhanced login flow
            this.studentId = studentId;
            this.password = password;
            PerformLogin();
        }

        private void Logout()
        {
            authService?.Logout();

            // Clear credentials if not remembering
            if (!rememberMe && credentialsManager != null)
            {
                try
                {
                    credentialsManager.ClearCredentials();
                    Debug.Log("[AssignmentWindow] Cleared saved credentials on logout");
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[AssignmentWindow] Failed to clear credentials: {ex.Message}");
                }
            }

            submitStatus = "Logged out successfully";
            studentId = "";
            password = "";
            registerStudentId = ""; // Clear registration field as well
            isProcessingLogin = false;
            isProcessingRegistration = false;
            Repaint();
        }

        private void RunAllTests()
        {
            RunTests(TestMode.EditMode | TestMode.PlayMode);
        }

        private void RunEditModeTests()
        {
            RunTests(TestMode.EditMode);
        }

        private void RunPlayModeTests()
        {
            RunTests(TestMode.PlayMode);
        }

        private void RunTests(TestMode mode)
        {
            if (testResultService == null)
            {
                submitStatus = "Test service not available";
                return;
            }

            testResults.Clear();
            isRunningTests = true;

            TestResultCapture.SetShowDialogAfterTestRun(true);

            testResultService.RunTests(mode);
            submitStatus = $"Running {mode} tests...";
            Repaint();
        }

        private void ClearResults()
        {
            testResults.Clear();
            submitStatus = "Test results cleared";
            Repaint();
        }

        private void SubmitAssignment()
        {
            if (submissionService == null)
            {
                submitStatus = "Submission service not available";
                return;
            }

            // Show the password dialog
            ShowPasswordDialog();
        }

        private void ShowPasswordDialog()
        {
            // Create a custom window for password entry
            var passwordWindow = CreateInstance<PasswordDialog>();

            // Initialize with callback
            passwordWindow.Initialize((password) =>
            {
                if (string.IsNullOrEmpty(password))
                {
                    submitStatus = "Submission canceled. Password is required.";
                    Repaint();
                    return;
                }

                // Create the callback
                Action<SubmissionResult> callback = result =>
                {
                    submitStatus = result.Success
                        ? "Assignment submitted successfully!"
                        : $"Submission failed: {result.Message}";
                    Repaint();

                    EditorUtility.DisplayDialog("Submission Status", submitStatus, "OK");
                };

                // Call the new method with password
                submissionService.SubmitAssignmentWithPassword(password, callback);
            });

            // Show the window as a utility window (always on top)
            var windowSize = new Vector2(350, 180);
            var windowPos = new Vector2(
                (Screen.currentResolution.width - windowSize.x) / 2,
                (Screen.currentResolution.height - windowSize.y) / 2);

            passwordWindow.position = new Rect(windowPos.x, windowPos.y, windowSize.x, windowSize.y);
            passwordWindow.ShowUtility();
        }

        // Helper methods
        private bool CanSubmitAssignment()
        {
            return authService?.IsLoggedIn == true && HasTestResults();
        }

        private bool HasTestResults()
        {
            return testResultService?.HasTestResults() == true || testResults.Count > 0;
        }

        private string GetTestCategory(string testName)
        {
            if (testName.Contains("CheckNumberSign")) return "Number Sign";
            if (testName.Contains("FindLargest")) return "Find Largest";
            if (testName.Contains("GetLetterGrade")) return "Letter Grade";
            if (testName.Contains("GetLifeStage")) return "Life Stage";
            if (testName.Contains("IsEligibleToVote")) return "Vote Eligibility";
            return "General";
        }

        private string GetStatusSymbol(TestStatus status)
        {
            return status switch
            {
                TestStatus.Passed => "✓",
                TestStatus.Failed => "✗",
                TestStatus.NotRun => "○",
                _ => "?"
            };
        }

        private Color GetStatusColor(TestStatus status)
        {
            return status switch
            {
                TestStatus.Passed => passedColor,
                TestStatus.Failed => failedColor,
                TestStatus.NotRun => notRunColor,
                _ => Color.white
            };
        }

        // Helper to find the testcase file path using AssignmentConfig.AssignmentTestcaseFiles
        private string FindTestcaseFilePath(TestInfo testInfo)
        {
            if (AssignmentConfig.AssignmentTestcaseFiles == null)
                return null;
            foreach (var file in AssignmentConfig.AssignmentTestcaseFiles)
            {
                string className = testInfo.FullName.Split('.')[0];
                if (Path.GetFileNameWithoutExtension(file) == className)
                    return file;
            }
            return null;
        }

        // Helper to find the line number of the test method in the file
        private int FindTestcaseLineNumber(string filePath, TestInfo testInfo)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return 1;
            string methodName = testInfo.FullName.Split('.').Last();
            var lines = File.ReadAllLines(filePath);
            for (int i = 0; i < lines.Length; i++)
            {
                // Look for method definition (simple match)
                if (lines[i].Contains($"void {methodName}") || lines[i].Contains($"public void {methodName}") || lines[i].Contains($"private void {methodName}"))
                {
                    return i + 1; // Unity expects 1-based line numbers
                }
            }
            return 1;
        }
    }

    // Helper classes
    [System.Serializable]
    public class TestInfo
    {
        public string Name;
        public string FullName;
        public string Category;
    }

    public enum TestStatus
    {
        NotRun,
        Passed,
        Failed
    }
}