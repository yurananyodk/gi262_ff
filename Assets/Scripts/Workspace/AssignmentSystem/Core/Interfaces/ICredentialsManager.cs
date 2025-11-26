namespace Assignment.Core.Interfaces
{
    /// <summary>
    /// Interface for managing user credentials
    /// </summary>
    public interface ICredentialsManager
    {
        /// <summary>
        /// Saves user credentials
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <param name="password">Password</param>
        /// <param name="rememberMe">Whether to remember credentials</param>
        void SaveCredentials(string studentId, string password, bool rememberMe);
        
        /// <summary>
        /// Loads saved credentials
        /// </summary>
        /// <param name="studentId">Output student ID</param>
        /// <param name="password">Output password</param>
        /// <returns>True if credentials were loaded successfully</returns>
        bool LoadCredentials(out string studentId, out string password);
        
        /// <summary>
        /// Checks if remember me is enabled
        /// </summary>
        /// <returns>True if remember me is enabled</returns>
        bool IsRememberMeEnabled();
        
        /// <summary>
        /// Clears saved credentials
        /// </summary>
        void ClearCredentials();
    }
}
