using UnityEngine;
using UnityEditor;
using Assignment.Core.Interfaces;
using Assignment.Core.DI;

namespace Assignment.UI
{
    /// <summary>
    /// Refactored editor window for handling student login using the modular architecture
    /// </summary>
    public class LoginDialogWindow : EditorWindow
    {
        private string _studentId = "";
        private string _password = "";
        private bool _isProcessing = false;
        private bool _rememberMe = false;
        private Vector2 _windowSize = new Vector2(300, 210);

        private IAuthenticationService _authService;
        private ICredentialsManager _credentialsManager;

        public static void ShowLoginWindow()
        {
            var window = GetWindow<LoginDialogWindow>(true, "Student Login", true);
            window.position = new Rect(
                (Screen.currentResolution.width - window._windowSize.x) / 2,
                (Screen.currentResolution.height - window._windowSize.y) / 2,
                window._windowSize.x,
                window._windowSize.y);

            window.Initialize();
            window.ShowUtility();
        }

        private void Initialize()
        {
            try
            {
                var container = ServiceContainer.Instance;
                _authService = container.GetService<IAuthenticationService>();
                _credentialsManager = container.GetService<ICredentialsManager>();

                LoadSavedCredentials();
                Debug.Log("[LoginDialogWindow] Initialized with dependency injection");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[LoginDialogWindow] Failed to initialize: {ex.Message}");
                EditorUtility.DisplayDialog("Initialization Error",
                    "Failed to initialize login system. Please restart Unity.", "OK");
            }
        }

        private void OnGUI()
        {
            if (_authService == null || _credentialsManager == null)
            {
                DrawErrorMessage();
                return;
            }

            DrawLoginForm();
        }

        private void DrawErrorMessage()
        {
            GUILayout.BeginVertical();
            GUILayout.Space(20);
            GUILayout.Label("System not initialized properly.", EditorStyles.boldLabel);
            GUILayout.Label("Please restart Unity Editor.", EditorStyles.helpBox);

            if (GUILayout.Button("Close", GUILayout.Width(100)))
            {
                Close();
            }
            GUILayout.EndVertical();
        }

        private void DrawLoginForm()
        {
            GUILayout.BeginVertical();
            GUILayout.Space(10);

            DrawHeader();
            DrawInputFields();
            DrawRememberMeCheckbox();
            DrawButtons();

            GUILayout.EndVertical();
        }

        private void DrawHeader()
        {
            GUILayout.Label("Please enter your login information:", EditorStyles.boldLabel);
            GUILayout.Space(10);
        }

        private void DrawInputFields()
        {
            // Student ID field
            GUILayout.BeginHorizontal();
            GUILayout.Label("Student ID:", GUILayout.Width(80));
            _studentId = GUILayout.TextField(_studentId, GUILayout.Width(200));
            GUILayout.EndHorizontal();

            // Password field
            GUILayout.BeginHorizontal();
            GUILayout.Label("Password:", GUILayout.Width(80));
            _password = GUILayout.PasswordField(_password, '*', GUILayout.Width(200));
            GUILayout.EndHorizontal();
        }

        private void DrawRememberMeCheckbox()
        {
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.Space(80); // Align with other fields
            _rememberMe = GUILayout.Toggle(_rememberMe, "Remember me", GUILayout.Width(200));
            GUILayout.EndHorizontal();
        }

        private void DrawButtons()
        {
            GUILayout.Space(20);

            EditorGUI.BeginDisabledGroup(_isProcessing);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            // Cancel button
            if (GUILayout.Button("Cancel", GUILayout.Width(100)))
            {
                Close();
            }

            // Login button
            bool canLogin = !string.IsNullOrEmpty(_studentId) &&
                           !string.IsNullOrEmpty(_password) &&
                           !_isProcessing;

            GUI.enabled = canLogin;
            if (GUILayout.Button("Login", GUILayout.Width(100)))
            {
                PerformLogin();
            }
            GUI.enabled = true;

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
        }

        private void PerformLogin()
        {
            if (!ValidateInput())
                return;

            _isProcessing = true;

            _authService.Login(_studentId, _password, result =>
            {
                _isProcessing = false;

                if (result.Success)
                {
                    HandleSuccessfulLogin(result);
                }
                else
                {
                    HandleFailedLogin(result);
                }
            });
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrEmpty(_studentId) || string.IsNullOrEmpty(_password))
            {
                EditorUtility.DisplayDialog("Error",
                    "Please enter both Student ID and Password", "OK");
                return false;
            }
            return true;
        }

        private void HandleSuccessfulLogin(AuthResult result)
        {
            // Save credentials if remember me is checked
            _credentialsManager.SaveCredentials(_studentId, _password, _rememberMe);

            EditorUtility.DisplayDialog("Login Successful",
                "You have successfully logged in to the Assignment System.", "OK");

            Debug.Log($"[LoginDialogWindow] Successfully logged in with token: {result.Token}");
            Close();
        }

        private void HandleFailedLogin(AuthResult result)
        {
            string message = result.Message ?? "Login failed. Please check your credentials and try again.";
            EditorUtility.DisplayDialog("Login Failed", message, "OK");
            Debug.LogError($"[LoginDialogWindow] Login failed: {result.Message}");
        }

        private void LoadSavedCredentials()
        {
            if (_credentialsManager.LoadCredentials(out string savedStudentId, out string savedPassword))
            {
                _studentId = savedStudentId;
                _password = savedPassword;
                _rememberMe = true;
                Debug.Log("[LoginDialogWindow] Loaded saved credentials");
            }
            else
            {
                _rememberMe = _credentialsManager.IsRememberMeEnabled();
            }
        }
    }
}
