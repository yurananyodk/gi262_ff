using System;
using UnityEngine;
using Assignment.Core.Interfaces;

namespace AssignmentSystem.Services
{
    /// <summary>
    /// Implementation of IAuthenticationService using the AssignmentApiClient
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IApiClient _apiClient;
        // private string _authToken;
        // private string _authStudentId;

        public bool IsLoggedIn => !string.IsNullOrEmpty(AuthToken);
        public string AuthToken => _apiClient.GetAuthToken();
        public string AuthStudentId => _apiClient.GetAuthStudentId();

        public event Action<bool> OnAuthenticationChanged;

        public AuthenticationService(IApiClient apiClient)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        }

        public void Login(string studentId, string password, Action<AuthResult> callback)
        {
            if (string.IsNullOrEmpty(studentId) || string.IsNullOrEmpty(password))
            {
                callback?.Invoke(new AuthResult(false, null, "Student ID and password are required"));
                return;
            }

            Debug.Log($"[AuthenticationService] Attempting login for student: {studentId}");

            _apiClient.Login(studentId, password, (success, token) =>
            {
                if (success)
                {
                    OnAuthenticationChanged?.Invoke(true);
                    Debug.Log("[AuthenticationService] Login successful");
                }
                else
                {
                    OnAuthenticationChanged?.Invoke(false);
                    Debug.LogError("[AuthenticationService] Login failed");
                }

                callback?.Invoke(new AuthResult(
                    success,
                    token,
                    success ? "Login successful" : "Login failed. Please check your credentials."
                ));
            });
        }

        public void Signup(string studentId, Action<AuthResult> callback)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                callback?.Invoke(new AuthResult(false, null, "Student ID is required"));
                return;
            }

            Debug.Log($"[AuthenticationService] Attempting signup for student: {studentId}");

            _apiClient.Signup(studentId, (success, message) =>
            {
                if (success)
                {
                    // For signup, we automatically get the token and are logged in
                    OnAuthenticationChanged?.Invoke(true);
                    Debug.Log("[AuthenticationService] Signup successful");
                }
                else
                {
                    OnAuthenticationChanged?.Invoke(false);
                    Debug.LogError("[AuthenticationService] Signup failed");
                }

                callback?.Invoke(new AuthResult(
                    success,
                    AuthToken,
                    message ?? (success ? "Registration successful" : "Registration failed. Please try again.")
                ));
            });
        }

        public void Logout()
        {
            Debug.Log("[AuthenticationService] Logging out");
            _apiClient.Logout();
            OnAuthenticationChanged?.Invoke(false);
        }
    }
}
