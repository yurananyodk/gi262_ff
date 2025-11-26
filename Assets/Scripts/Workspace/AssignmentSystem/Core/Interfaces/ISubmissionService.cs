using System;

namespace Assignment.Core.Interfaces
{
    /// <summary>
    /// Interface for handling assignment submission operations
    /// </summary>
    public interface ISubmissionService
    {
        /// <summary>
        /// Submits assignment using default assignment ID with password
        /// </summary>
        /// <param name="password">Password for verification</param>
        /// <param name="callback">Callback with submission result</param>
        void SubmitAssignmentWithPassword(string password, Action<SubmissionResult> callback);

        /// <summary>
        /// Submits assignment with specific assignment ID and password
        /// </summary>
        /// <param name="assignmentId">Base64 encoded assignment ID</param>
        /// <param name="password">Password for verification</param>
        /// <param name="callback">Callback with submission result</param>
        void SubmitAssignmentWithPassword(string assignmentId, string password, Action<SubmissionResult> callback);

        /// <summary>
        /// Legacy method for submitting assignment - will be removed in future versions
        /// </summary>
        /// <param name="callback">Callback with submission result</param>
        void SubmitAssignment(Action<SubmissionResult> callback);

        /// <summary>
        /// Legacy method for submitting assignment - will be removed in future versions
        /// </summary>
        /// <param name="assignmentId">Base64 encoded assignment ID</param>
        /// <param name="callback">Callback with submission result</param>
        void SubmitAssignment(string assignmentId, Action<SubmissionResult> callback);
    }

    /// <summary>
    /// Result of a submission operation
    /// </summary>
    public class SubmissionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }

        public SubmissionResult(bool success, string message = null)
        {
            Success = success;
            Message = message;
        }
    }
}
