using System;
using UnityEngine.Rendering;

namespace Assignment.Core.Interfaces
{
    /// <summary>
    /// Interface for handling authentication operations
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// Gets whether the user is currently logged in
        /// </summary>
        bool IsLoggedIn { get; }

        /// <summary>
        /// Gets the current authentication token
        /// </summary>
        string AuthToken { get; }

        /// <summary>
        /// Gets the current authenticated student's ID
        /// </summary>
        string AuthStudentId { get; }

        /// <summary>
        /// Performs login with credentials
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <param name="password">Password</param>
        /// <param name="callback">Callback with authentication result</param>
        void Login(string studentId, string password, Action<AuthResult> callback);

        /// <summary>
        /// Performs signup with student ID
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <param name="callback">Callback with authentication result</param>
        void Signup(string studentId, Action<AuthResult> callback);

        /// <summary>
        /// Logs out the current user
        /// </summary>
        void Logout();

        /// <summary>
        /// Event fired when authentication state changes
        /// </summary>
        event Action<bool> OnAuthenticationChanged;
    }

    /// <summary>
    /// Result of an authentication operation
    /// </summary>
    public class AuthResult
    {
        public bool Success { get; set; }
        public string Token { get; set; }
        public string Message { get; set; }

        public AuthResult(bool success, string token = null, string message = null)
        {
            Success = success;
            Token = token;
            Message = message;
        }
    }
}
