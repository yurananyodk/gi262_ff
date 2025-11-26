using System;

namespace Assignment.Core.Interfaces
{
    /// <summary>
    /// Interface for API client operations
    /// </summary>
    public interface IApiClient
    {
        /// <summary>
        /// Gets the current authentication token
        /// </summary>
        string GetAuthToken();

        /// <summary>
        /// Gets the current authenticated student's ID
        /// </summary>
        /// <returns></returns>
        string GetAuthStudentId();

        /// <summary>
        /// Performs login operation
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <param name="password">Password</param>
        /// <param name="callback">Callback with success status and token</param>
        void Login(string studentId, string password, Action<bool, string> callback);

        /// <summary>
        /// Performs signup operation
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <param name="callback">Callback with success status and message</param>
        void Signup(string studentId, Action<bool, string> callback);

        /// <summary>
        /// Performs logout operation
        /// </summary>
        void Logout();

        /// <summary>
        /// Submits assignment with default ID
        /// </summary>
        /// <param name="password">Password for verification</param>
        /// <param name="callback">Callback with success status and message</param>
        void SubmitAssignmentWithPassword(string password, Action<bool, string> callback);

        /// <summary>
        /// Submits assignment with specific ID
        /// </summary>
        /// <param name="assignmentId">Base64 encoded assignment ID</param>
        /// <param name="password">Password for verification</param>
        /// <param name="callback">Callback with success status and message</param>
        void SubmitAssignmentWithPassword(string assignmentId, string password, Action<bool, string> callback);

        /// <summary>
        /// Legacy submission method - kept for backward compatibility but will be removed
        /// </summary>
        /// <param name="callback">Callback with success status and message</param>
        [Obsolete("Use SubmitAssignmentWithPassword instead")]
        void SubmitAssignment(Action<bool, string> callback);

        /// <summary>
        /// Legacy submission method with specific ID - kept for backward compatibility but will be removed
        /// </summary>
        /// <param name="assignmentId">Base64 encoded assignment ID</param>
        /// <param name="callback">Callback with success status and message</param>
        [Obsolete("Use SubmitAssignmentWithPassword instead")]
        void SubmitAssignment(string assignmentId, Action<bool, string> callback);
    }
}
