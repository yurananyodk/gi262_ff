using System;
using UnityEngine;
using Assignment.Core.Interfaces;

namespace AssignmentSystem.Services
{
    /// <summary>
    /// Implementation of ISubmissionService
    /// </summary>
    public class SubmissionService : ISubmissionService
    {
        private readonly IApiClient _apiClient;
        private readonly ITestResultService _testResultService;

        public SubmissionService(IApiClient apiClient, ITestResultService testResultService)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _testResultService = testResultService ?? throw new ArgumentNullException(nameof(testResultService));
        }

        public void SubmitAssignmentWithPassword(string password, Action<SubmissionResult> callback)
        {
            Debug.Log("[SubmissionService] Submitting assignment with default ID and password");

            if (string.IsNullOrEmpty(password))
            {
                callback?.Invoke(new SubmissionResult(false, "Password cannot be empty"));
                return;
            }

            if (!ValidateSubmissionPrerequisites(callback))
                return;

            _apiClient.SubmitAssignmentWithPassword(password, (success, message) =>
            {
                var result = new SubmissionResult(success, message);
                Debug.Log($"[SubmissionService] Submission result: {(success ? "Success" : "Failed")} - {message}");
                callback?.Invoke(result);
            });
        }

        public void SubmitAssignmentWithPassword(string assignmentId, string password, Action<SubmissionResult> callback)
        {
            Debug.Log($"[SubmissionService] Submitting assignment with ID: {assignmentId} and password");

            if (string.IsNullOrEmpty(assignmentId))
            {
                callback?.Invoke(new SubmissionResult(false, "Assignment ID cannot be empty"));
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                callback?.Invoke(new SubmissionResult(false, "Password cannot be empty"));
                return;
            }

            if (!ValidateSubmissionPrerequisites(callback))
                return;

            _apiClient.SubmitAssignmentWithPassword(assignmentId, password, (success, message) =>
            {
                var result = new SubmissionResult(success, message);
                Debug.Log($"[SubmissionService] Submission result: {(success ? "Success" : "Failed")} - {message}");
                callback?.Invoke(result);
            });
        }

        public void SubmitAssignment(Action<SubmissionResult> callback)
        {
            Debug.LogWarning("[SubmissionService] Legacy submission method called without password");
            callback?.Invoke(new SubmissionResult(false, "Password is required for submission. Please use the updated submission method."));
        }

        public void SubmitAssignment(string assignmentId, Action<SubmissionResult> callback)
        {
            Debug.LogWarning("[SubmissionService] Legacy submission method called without password");
            callback?.Invoke(new SubmissionResult(false, "Password is required for submission. Please use the updated submission method."));
        }

        private bool ValidateSubmissionPrerequisites(Action<SubmissionResult> callback)
        {
            // Check if user is authenticated
            if (string.IsNullOrEmpty(_apiClient.GetAuthToken()))
            {
                Debug.LogError("[SubmissionService] User is not authenticated");
                callback?.Invoke(new SubmissionResult(false, "Please login first before submitting assignment"));
                return false;
            }

            // Check if test results are available
            if (!_testResultService.HasTestResults())
            {
                Debug.LogError("[SubmissionService] No test results available");
                callback?.Invoke(new SubmissionResult(false, "Please run tests first before submitting assignment"));
                return false;
            }

            return true;
        }
    }
}
