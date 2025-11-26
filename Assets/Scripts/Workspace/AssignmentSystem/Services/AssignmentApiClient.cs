using System;
using Assignment.Core.Interfaces;
using Assignment;
using UnityEngine;
using Assignment.Core;

namespace AssignmentSystem.Services
{
    /// <summary>
    /// Wrapper around the existing AssignmentApi to implement IApiClient interface
    /// </summary>
    public class AssignmentApiClient : IApiClient
    {
        private readonly AssignmentApi _assignmentApi;

        public AssignmentApiClient(string baseUrl)
        {
            _assignmentApi = new AssignmentApi(baseUrl);
        }

        public string GetAuthToken()
        {
            return _assignmentApi.GetAuthToken();
        }

        public string GetAuthStudentId()
        {
            return _assignmentApi.GetAuthStudentId();
        }

        public void Login(string studentId, string password, Action<bool, string> callback)
        {
            _assignmentApi.Login(studentId, password, (success, token) => callback(success, token));
        }

        public void Signup(string studentId, Action<bool, string> callback)
        {
            _assignmentApi.Signup(studentId, (success, message) => callback(success, message));
        }

        public void Logout()
        {
            _assignmentApi.Logout();
        }

        public void SubmitAssignmentWithPassword(string password, Action<bool, string> callback)
        {
            _assignmentApi.SubmitAssignment(password, (success, message) => callback(success, message));
        }

        public void SubmitAssignmentWithPassword(string assignmentId, string password, Action<bool, string> callback)
        {
            _assignmentApi.SubmitAssignment(assignmentId, password, (success, message) => callback(success, message));
        }

        [Obsolete("Use SubmitAssignmentWithPassword instead")]
        public void SubmitAssignment(Action<bool, string> callback)
        {
            Debug.LogWarning("Using deprecated SubmitAssignment method. Password is now required.");
            callback(false, "Password is required for submission. Please use the updated submission method.");
        }

        [Obsolete("Use SubmitAssignmentWithPassword instead")]
        public void SubmitAssignment(string assignmentId, Action<bool, string> callback)
        {
            Debug.LogWarning("Using deprecated SubmitAssignment method. Password is now required.");
            callback(false, "Password is required for submission. Please use the updated submission method.");
        }
    }
}
