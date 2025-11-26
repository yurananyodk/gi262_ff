using UnityEngine;
using UnityEditor;
using Assignment.Core.DI;
using Assignment.Core.Interfaces;

/// <summary>
/// Editor window for handling student login to the assignment system
/// Refactored to use improved architecture patterns
/// </summary>
public class LoginDialogWindow : EditorWindow
{
    private string studentId = "";
    private string password = "";
    private Vector2 windowSize = new Vector2(300, 210);
    private bool isProcessing = false;
    private bool rememberMe = false;

    public static void ShowLoginWindow()
    {
        LoginDialogWindow window = (LoginDialogWindow)GetWindow(
            typeof(LoginDialogWindow), true, "Student Login", true);
        window.position = new Rect(
            (Screen.currentResolution.width - window.windowSize.x) / 2,
            (Screen.currentResolution.height - window.windowSize.y) / 2,
            window.windowSize.x,
            window.windowSize.y);

        // Load saved credentials if available
        window.LoadSavedCredentials();

        window.ShowUtility();
    }

    private void OnGUI()
    {
        GUILayout.BeginVertical();
        GUILayout.Space(10);

        GUILayout.Label("Please enter your login information:", EditorStyles.boldLabel);
        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Student ID:", GUILayout.Width(80));
        studentId = GUILayout.TextField(studentId, GUILayout.Width(200));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Password:", GUILayout.Width(80));
        password = GUILayout.PasswordField(password, '*', GUILayout.Width(200));
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        // Remember me checkbox
        GUILayout.BeginHorizontal();
        GUILayout.Space(80); // Align with other fields
        rememberMe = GUILayout.Toggle(rememberMe, "Remember me", GUILayout.Width(200));
        GUILayout.EndHorizontal();

        GUILayout.Space(20);

        EditorGUI.BeginDisabledGroup(isProcessing);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Cancel", GUILayout.Width(100)))
        {
            Close();
        }

        GUI.enabled = !string.IsNullOrEmpty(studentId) && !string.IsNullOrEmpty(password);
        if (GUILayout.Button("Login", GUILayout.Width(100)))
        {
            PerformLogin();
        }
        GUI.enabled = true;

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        EditorGUI.EndDisabledGroup();

        GUILayout.EndVertical();
    }

    private void PerformLogin()
    {
        if (!ValidateLoginInput())
            return;

        isProcessing = true;

        // Use the refactored authentication approach
        PerformAuthenticationRequest();
    }

    private bool ValidateLoginInput()
    {
        if (string.IsNullOrEmpty(studentId) || string.IsNullOrEmpty(password))
        {
            EditorUtility.DisplayDialog("Error", "Please enter both Student ID and Password", "OK");
            return false;
        }
        return true;
    }

    private void PerformAuthenticationRequest()
    {
        ServiceContainer.Instance.
            GetService<IAuthenticationService>().
            Login(studentId, password, (result) =>
            {
                HandleAuthenticationResult(result.Success, result.Token);
            });
    }

    private void HandleAuthenticationResult(bool success, string token)
    {
        isProcessing = false;

        if (success)
        {
            HandleSuccessfulLogin(token);
        }
        else
        {
            HandleFailedLogin();
        }
    }

    private void HandleSuccessfulLogin(string token)
    {
        // Save credentials if remember me is checked
        LoginCredentialsManager.SaveCredentials(studentId, password, rememberMe);

        EditorUtility.DisplayDialog("Login Successful",
            "You have successfully logged in to the Assignment System.", "OK");
        Debug.Log($"Successfully logged in with token: {token}");
        Close();
    }

    private void HandleFailedLogin()
    {
        EditorUtility.DisplayDialog("Login Failed",
            "Login failed. Please check your credentials and try again.", "OK");
        Debug.LogError("Login failed. Please check your credentials.");
    }

    /// <summary>
    /// Loads saved credentials from PlayerPrefs if available
    /// </summary>
    private void LoadSavedCredentials()
    {
        if (LoginCredentialsManager.LoadCredentials(out string savedStudentId, out string savedPassword))
        {
            studentId = savedStudentId;
            password = savedPassword;
            rememberMe = true;
        }
        else
        {
            rememberMe = LoginCredentialsManager.IsRememberMeEnabled();
        }
    }
}
