using Assignment.Core.Interfaces;

namespace AssignmentSystem.Services
{
    /// <summary>
    /// Wrapper around the existing LoginCredentialsManager to implement ICredentialsManager interface
    /// </summary>
    public class CredentialsManager : ICredentialsManager
    {
        public void SaveCredentials(string studentId, string password, bool rememberMe)
        {
            LoginCredentialsManager.SaveCredentials(studentId, password, rememberMe);
        }

        public bool LoadCredentials(out string studentId, out string password)
        {
            return LoginCredentialsManager.LoadCredentials(out studentId, out password);
        }

        public bool IsRememberMeEnabled()
        {
            return LoginCredentialsManager.IsRememberMeEnabled();
        }

        public void ClearCredentials()
        {
            LoginCredentialsManager.ClearCredentials();
        }
    }
}
