using UnityEngine;
using UnityEditor;
using Assignment.Core.DI;
using Assignment.Core.Interfaces;
using Assignment.Core;
using Assignment;

namespace AssignmentSystem.Services
{
    /// <summary>
    /// Initializes the assignment system services on editor load
    /// </summary>
    [InitializeOnLoad]
    public static class AssignmentSystemInitializer
    {
        /// <summary>
        /// Reference to the test result service.
        /// Note: we need to keep this static to manage the lifecycle of the service correctly
        /// This allows us to gracefully stop the old service instance and start a new one
        /// </summary>
        private static TestResultService _testResultService;

        static AssignmentSystemInitializer()
        {
            InitializeServices();
        }

        /// <summary>
        /// Initializes all services and registers them in the service container
        /// </summary>
        private static void InitializeServices()
        {
            if (AssignmentSystemConfig.VERBOSE)
            {
                Debug.Log("[AssignmentSystemInitializer] Initializing assignment system services...");
            }
            var container = ServiceContainer.Instance;

            // Clear any existing services first
            container.Clear();

            // Create and register services
            var apiClient = new AssignmentApiClient(AssignmentConfig.ApiBaseUrl);
            var authService = new AuthenticationService(apiClient);
            var testResultService = new TestResultService();
            var submissionService = new SubmissionService(apiClient, testResultService);
            var credentialsManager = new CredentialsManager();

            container.RegisterService<IApiClient>(apiClient);
            container.RegisterService<IAuthenticationService>(authService);
            container.RegisterService<ITestResultService>(testResultService);
            container.RegisterService<ISubmissionService>(submissionService);
            container.RegisterService<ICredentialsManager>(credentialsManager);

            // Initialize test capture
            testResultService.InitializeCapture();

            // Subscribe to authentication events for logging
            authService.OnAuthenticationChanged += (isLoggedIn) =>
            {
                Debug.Log($"[AssignmentSystemInitializer] Authentication state changed: {(isLoggedIn ? "Logged In" : "Logged Out")}");
            };

            if (AssignmentSystemConfig.VERBOSE)
            {
                Debug.Log("[AssignmentSystemInitializer] Assignment system services initialized successfully");
            }

            if (_testResultService != null)
            {
                _testResultService.CleanUp();
            }
            _testResultService = testResultService; // Keep a reference to the service for later use
        }

        /// <summary>
        /// Reinitialize services (useful after domain reloads)
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void ReinitializeServices()
        {
            // InitializeServices();
            // do nothing ...
        }
    }
}
