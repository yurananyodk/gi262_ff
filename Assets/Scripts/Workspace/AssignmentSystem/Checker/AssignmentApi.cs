using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Assignment;
using Assignment.Core.DI;
using Assignment.Core.Interfaces;
using Assignment.Core;

/// <summary>
/// API client for the assignment grading system
/// </summary>
public class AssignmentApi
{
    private readonly string baseUrl;
    private string authToken;
    private string authStudentId;

    // PlayerPrefs key for storing encrypted auth token
    private const string AUTH_TOKEN_KEY = "AssignmentApi_AuthToken";
    // PlayerPrefs key for storing encrypted auth student ID
    private const string AUTH_STUDENT_ID_KEY = "AssignmentApi_AuthStudentID";

    // Simple encryption key (in production, this should be more secure)
    private const string ENCRYPTION_KEY = "AssignmentApiKey2024";

    /// <summary>
    /// Initializes a new instance of the AssignmentApi class
    /// </summary>
    /// <param name="baseUrl">Base URL of the API</param>
    public AssignmentApi(string baseUrl)
    {
        this.baseUrl = baseUrl;

        // Load auth token from PlayerPrefs on initialization
        LoadAuthTokenFromPlayerPrefs();
    }

    /// <summary>
    /// Response model for login request
    /// </summary>
    [Serializable]
    public class LoginResponse
    {
        public string token;
        public string message;
    }

    /// <summary>
    /// Request model for login request
    /// </summary>
    [Serializable]
    public class LoginRequest
    {
        public string student_id;
        public string password;
    }

    /// <summary>
    /// Response model for signup request
    /// </summary>
    [Serializable]
    public class SignupResponse
    {
        public string student_id;
        public bool is_password_changed;
        public string token;
        public string message;
    }

    /// <summary>
    /// Request model for signup request
    /// </summary>
    [Serializable]
    public class SignupRequest
    {
        public string student_id;
    }

    /// <summary>
    /// Error response model for API errors
    /// </summary>
    [Serializable]
    public class ErrorResponse
    {
        public string error;
    }

    public delegate void LoginCallback(bool success, string token);
    public delegate void SignupCallback(bool success, string message);
    public delegate void SubmissionCallback(bool success, string message);

    /// <summary>
    /// Authenticates a student and receives an authentication token
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <param name="password">Password</param>
    /// <param name="callback">Callback with results (success, response message or token)</param>
    public void Login(string studentId, string password, LoginCallback callback)
    {
        string endpoint = $"{baseUrl}/api/login";

        // Create login request body
        var loginRequest = new LoginRequest
        {
            student_id = studentId,
            password = password
        };
        string jsonBody = JsonConvert.SerializeObject(loginRequest);

        UnityWebRequest request = new(endpoint, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // Create operation to track request
        var operation = request.SendWebRequest();

        // Add completion callback
        operation.completed += (asyncOp) =>
        {
            try
            {
                if (request.result == UnityWebRequest.Result.Success)
                {
                    LoginResponse response = JsonConvert.DeserializeObject<LoginResponse>(request.downloadHandler.text);
                    this.authToken = response.token;
                    this.authStudentId = studentId; // Store student ID for later use
                    SaveAuthTokenToPlayerPrefs(); // Save token on successful login
                    callback(true, this.authToken);
                }
                else
                {
                    Debug.LogError($"Login error: {request.error}");
                    callback(false, request.error);
                }
            }
            finally
            {
                // Clean up the request
                request.Dispose();
            }
        };
    }

    /// <summary>
    /// Registers a new student account and receives an authentication token
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <param name="callback">Callback with results (success, message)</param>
    public void Signup(string studentId, SignupCallback callback)
    {
        string endpoint = $"{baseUrl}/api/signup";

        // Create signup request body
        var signupRequest = new SignupRequest
        {
            student_id = studentId
        };
        string jsonBody = JsonConvert.SerializeObject(signupRequest);

        UnityWebRequest request = new(endpoint, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // Create operation to track request
        var operation = request.SendWebRequest();

        // Add completion callback
        operation.completed += (asyncOp) =>
        {
            try
            {
                if (request.result == UnityWebRequest.Result.Success)
                {
                    SignupResponse response = JsonConvert.DeserializeObject<SignupResponse>(request.downloadHandler.text);
                    this.authToken = response.token;
                    this.authStudentId = studentId;
                    SaveAuthTokenToPlayerPrefs(); // Save token on successful signup
                    Debug.Log($"Signup successful: {response.message}");
                    callback(true, response.message);
                }
                else
                {
                    string errorMessage = "Registration failed";
                    try
                    {
                        // Try to parse error response for better error message
                        var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(request.downloadHandler.text);
                        if (!string.IsNullOrEmpty(errorResponse?.error))
                        {
                            errorMessage = errorResponse.error;
                        }
                    }
                    catch
                    {
                        // If we can't parse the error, use the default message
                        errorMessage = request.downloadHandler.text ?? request.error;
                    }

                    Debug.LogError($"Signup error: {errorMessage}");
                    callback(false, errorMessage);
                }
            }
            finally
            {
                // Clean up the request
                request.Dispose();
            }
        };
    }

    /// <summary>
    /// Submits assignment test results to the server
    /// </summary>
    /// <param name="assignmentIdBase64">Base64 encoded assignment ID</param>
    /// <param name="password">User's password for verification</param>
    /// <param name="callback">Callback with results (success, response message)</param>
    public void SubmitAssignment(string assignmentIdBase64, string password, SubmissionCallback callback)
    {
        if (string.IsNullOrEmpty(authToken))
        {
            callback(false, "Not authenticated. Please login first.");
            return;
        }

        // Get the JSON test results file path
        var testResultSvc = ServiceContainer.Instance.GetService<ITestResultService>();
        string jsonFilePath = testResultSvc.JsonOutputFilePath;

        if (string.IsNullOrEmpty(jsonFilePath) || !File.Exists(jsonFilePath))
        {
            callback(false, "No test results found. Please run tests first.");
            return;
        }

        try
        {
            // Read the JSON test results
            string jsonContent = File.ReadAllText(jsonFilePath);

            // Create submission request with password
            var submissionRequest = new
            {
                submission = JsonConvert.DeserializeObject(jsonContent),
                password
            };

            // Serialize the combined request
            string requestBody = JsonConvert.SerializeObject(submissionRequest);

            string endpoint = $"{baseUrl}/api/submissions?assignmentIdBase64={assignmentIdBase64}";

            UnityWebRequest request = new(endpoint, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(requestBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {authToken}");

            // Create operation to track request
            var operation = request.SendWebRequest();

            // Add completion callback
            operation.completed += (asyncOp) =>
            {
                try
                {
                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        Debug.Log($"Assignment submission successful: {request.downloadHandler.text}");
                        callback(true, "Assignment submitted successfully");
                    }
                    else
                    {
                        Debug.LogError($"Assignment submission error: {request.error}");
                        Debug.LogError($"Response: {request.downloadHandler.text}");
                        callback(false, $"Submission failed: {request.error}: {request.downloadHandler.text}");
                    }
                }
                finally
                {
                    // Clean up the request
                    request.Dispose();
                }
            };
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to submit assignment: {ex.Message}");
            callback(false, $"Failed to submit assignment: {ex.Message}");
        }
    }

    /// <summary>
    /// Submits assignment test results to the server using default assignment ID
    /// </summary>
    /// <param name="password">User's password for verification</param>
    /// <param name="callback">Callback with results (success, response message)</param>
    public void SubmitAssignment(string password, SubmissionCallback callback)
    {
        // Convert assignment resource ID to base64
        string assignmentId = AssignmentConfig.AssignmentResourceID;
        string assignmentIdBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(assignmentId));
        SubmitAssignment(assignmentIdBase64, password, callback);
    }

    /// <summary>
    /// Submits assignment test results to the server using default assignment ID
    /// This is kept for backward compatibility but will be deprecated
    /// </summary>
    /// <param name="callback">Callback with results (success, response message)</param>
    [Obsolete("Use SubmitAssignment with password parameter instead")]
    public void SubmitAssignment(SubmissionCallback callback)
    {
        Debug.LogError("SubmitAssignment method without password is no longer supported. Password is required for submission.");
        callback(false, "Password is required for submission. Please use the updated submission method.");
    }

    public void Logout()
    {
        // Clear the auth token
        authToken = null;
        authStudentId = "";

        // Remove token from PlayerPrefs
        PlayerPrefs.DeleteKey(AUTH_TOKEN_KEY);
        PlayerPrefs.DeleteKey(AUTH_STUDENT_ID_KEY);
        PlayerPrefs.Save();

        Debug.Log("Logged out successfully.");
    }

    /// <summary>
    /// Gets the current authentication token
    /// </summary>
    public string GetAuthToken()
    {
        return authToken;
    }

    public string GetAuthStudentId()
    {
        return authStudentId;
    }

    /// <summary>
    /// Checks if the user is currently authenticated
    /// </summary>
    /// <returns>True if authenticated, false otherwise</returns>
    public bool IsAuthenticated()
    {
        return !string.IsNullOrEmpty(authToken);
    }

    /// <summary>
    /// Loads the authentication token from PlayerPrefs
    /// </summary>
    private void LoadAuthTokenFromPlayerPrefs()
    {
        try
        {
            if (PlayerPrefs.HasKey(AUTH_TOKEN_KEY))
            {
                string encryptedToken = PlayerPrefs.GetString(AUTH_TOKEN_KEY);
                if (!string.IsNullOrEmpty(encryptedToken))
                {
                    authToken = DecryptString(encryptedToken);
                }
                authStudentId = PlayerPrefs.GetString(AUTH_STUDENT_ID_KEY);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load auth token from PlayerPrefs: {ex.Message}");
            // Clear corrupted data
            PlayerPrefs.DeleteKey(AUTH_TOKEN_KEY);
            PlayerPrefs.DeleteKey(AUTH_STUDENT_ID_KEY);
            authToken = null;
            authStudentId = "";
        }
    }

    /// <summary>
    /// Saves the authentication token to PlayerPrefs
    /// </summary>
    private void SaveAuthTokenToPlayerPrefs()
    {
        try
        {
            if (!string.IsNullOrEmpty(authToken))
            {
                string encryptedToken = EncryptString(authToken);
                PlayerPrefs.SetString(AUTH_TOKEN_KEY, encryptedToken);
                PlayerPrefs.SetString(AUTH_STUDENT_ID_KEY, authStudentId);
                PlayerPrefs.Save();
            }
            else
            {
                PlayerPrefs.DeleteKey(AUTH_TOKEN_KEY);
                PlayerPrefs.DeleteKey(AUTH_STUDENT_ID_KEY);
                PlayerPrefs.Save();
                authStudentId = "";
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save auth token to PlayerPrefs: {ex.Message}");
        }
    }

    /// <summary>
    /// Encrypts a string using AES encryption
    /// </summary>
    /// <param name="plainText">The text to encrypt</param>
    /// <returns>Base64 encoded encrypted string</returns>
    private string EncryptString(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        byte[] keyBytes = Encoding.UTF8.GetBytes(ENCRYPTION_KEY);

        // Ensure key is 32 bytes for AES-256
        Array.Resize(ref keyBytes, 32);

        using (var aes = Aes.Create())
        {
            aes.Key = keyBytes;
            aes.GenerateIV();

            using (var encryptor = aes.CreateEncryptor())
            {
                byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

                // Combine IV and encrypted data
                byte[] result = new byte[aes.IV.Length + encryptedBytes.Length];
                Array.Copy(aes.IV, 0, result, 0, aes.IV.Length);
                Array.Copy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);

                return Convert.ToBase64String(result);
            }
        }
    }

    /// <summary>
    /// Decrypts a string using AES decryption
    /// </summary>
    /// <param name="encryptedText">Base64 encoded encrypted string</param>
    /// <returns>Decrypted plain text</returns>
    private string DecryptString(string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText))
            return string.Empty;

        byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
        byte[] keyBytes = Encoding.UTF8.GetBytes(ENCRYPTION_KEY);

        // Ensure key is 32 bytes for AES-256
        Array.Resize(ref keyBytes, 32);

        using (var aes = Aes.Create())
        {
            aes.Key = keyBytes;

            // Extract IV from the beginning of encrypted data
            byte[] iv = new byte[aes.IV.Length];
            Array.Copy(encryptedBytes, 0, iv, 0, iv.Length);
            aes.IV = iv;

            // Extract encrypted data
            byte[] encrypted = new byte[encryptedBytes.Length - iv.Length];
            Array.Copy(encryptedBytes, iv.Length, encrypted, 0, encrypted.Length);

            using (var decryptor = aes.CreateDecryptor())
            {
                byte[] decryptedBytes = decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);
                return Encoding.UTF8.GetString(decryptedBytes);
            }
        }
    }
}
