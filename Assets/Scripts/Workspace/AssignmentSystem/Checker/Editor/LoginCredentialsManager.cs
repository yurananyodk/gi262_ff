using UnityEngine;
using System;
using System.Text;
using System.Security.Cryptography;

/// <summary>
/// Manages login credentials storage with encryption for passwords
/// </summary>
public static class LoginCredentialsManager
{
    public const string STUDENT_ID_KEY = "LoginDialog_StudentID";
    public const string PASSWORD_KEY = "LoginDialog_Password_Encrypted";
    public const string REMEMBER_ME_KEY = "LoginDialog_RememberMe";

    // Simple encryption key - in production, this should be more sophisticated
    private static readonly byte[] EncryptionKey = Encoding.UTF8.GetBytes("BangkokUniAssign"); // 16 bytes for AES

    /// <summary>
    /// Encrypts a string using AES encryption
    /// </summary>
    /// <param name="plainText">The text to encrypt</param>
    /// <returns>Base64 encoded encrypted string</returns>
    private static string EncryptString(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        try
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = EncryptionKey;
                aes.GenerateIV();

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                    byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

                    // Combine IV and encrypted data
                    byte[] result = new byte[aes.IV.Length + encryptedBytes.Length];
                    Array.Copy(aes.IV, 0, result, 0, aes.IV.Length);
                    Array.Copy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);

                    return Convert.ToBase64String(result);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to encrypt password: {ex.Message}");
            return string.Empty;
        }
    }

    /// <summary>
    /// Decrypts a Base64 encoded encrypted string using AES
    /// </summary>
    /// <param name="encryptedText">Base64 encoded encrypted string</param>
    /// <returns>Decrypted plain text</returns>
    private static string DecryptString(string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText))
            return string.Empty;

        try
        {
            byte[] encryptedData = Convert.FromBase64String(encryptedText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = EncryptionKey;

                // Extract IV (first 16 bytes)
                byte[] iv = new byte[16];
                Array.Copy(encryptedData, 0, iv, 0, 16);
                aes.IV = iv;

                // Extract encrypted content (remaining bytes)
                byte[] encryptedBytes = new byte[encryptedData.Length - 16];
                Array.Copy(encryptedData, 16, encryptedBytes, 0, encryptedBytes.Length);

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                {
                    byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                    return Encoding.UTF8.GetString(decryptedBytes);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to decrypt password: {ex.Message}");
            return string.Empty;
        }
    }

    /// <summary>
    /// Saves login credentials to PlayerPrefs
    /// </summary>
    /// <param name="studentId">Student ID to save</param>
    /// <param name="password">Password to save (will be encrypted)</param>
    /// <param name="rememberMe">Whether to remember the credentials</param>
    public static void SaveCredentials(string studentId, string password, bool rememberMe)
    {
        if (rememberMe)
        {
            PlayerPrefs.SetString(STUDENT_ID_KEY, studentId);
            string encryptedPassword = EncryptString(password);
            PlayerPrefs.SetString(PASSWORD_KEY, encryptedPassword);
            PlayerPrefs.SetInt(REMEMBER_ME_KEY, 1);
        }
        else
        {
            // Clear saved credentials
            ClearCredentials();
        }

        PlayerPrefs.Save();
    }

    /// <summary>
    /// Loads saved login credentials from PlayerPrefs
    /// </summary>
    /// <param name="studentId">Output student ID</param>
    /// <param name="password">Output decrypted password</param>
    /// <returns>True if remember me was enabled and credentials were loaded</returns>
    public static bool LoadCredentials(out string studentId, out string password)
    {
        studentId = string.Empty;
        password = string.Empty;

        bool rememberMe = PlayerPrefs.GetInt(REMEMBER_ME_KEY, 0) == 1;

        if (rememberMe)
        {
            studentId = PlayerPrefs.GetString(STUDENT_ID_KEY, string.Empty);
            string encryptedPassword = PlayerPrefs.GetString(PASSWORD_KEY, string.Empty);
            password = DecryptString(encryptedPassword);

            return !string.IsNullOrEmpty(studentId) && !string.IsNullOrEmpty(password);
        }

        return false;
    }

    /// <summary>
    /// Checks if remember me is currently enabled
    /// </summary>
    /// <returns>True if remember me is enabled</returns>
    public static bool IsRememberMeEnabled()
    {
        return PlayerPrefs.GetInt(REMEMBER_ME_KEY, 0) == 1;
    }

    /// <summary>
    /// Clears all saved credentials from PlayerPrefs
    /// </summary>
    public static void ClearCredentials()
    {
        PlayerPrefs.DeleteKey(STUDENT_ID_KEY);
        PlayerPrefs.DeleteKey(PASSWORD_KEY);
        PlayerPrefs.DeleteKey(REMEMBER_ME_KEY);
        PlayerPrefs.Save();
    }
}
