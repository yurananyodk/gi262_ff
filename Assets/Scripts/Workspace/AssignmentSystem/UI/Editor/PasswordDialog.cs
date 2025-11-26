using UnityEngine;
using UnityEditor;
using System;

namespace Assignment.UI
{
    /// <summary>
    /// Dialog window to prompt for password entry
    /// </summary>
    public class PasswordDialog : EditorWindow
    {
        private string password = "";
        private Action<string> callback;
        private bool showPassword = false;
        private GUIStyle headerStyle;

        public void Initialize(Action<string> onComplete)
        {
            this.callback = onComplete;
            this.titleContent = new GUIContent("Password Required");
        }

        private void OnEnable()
        {
            headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.fontSize = 14;
            headerStyle.alignment = TextAnchor.MiddleCenter;
            headerStyle.margin = new RectOffset(0, 0, 10, 10);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Password Required for Submission", headerStyle);
            EditorGUILayout.HelpBox("Please enter your password to submit the assignment.", MessageType.Info);
            EditorGUILayout.Space(10);

            // Password field
            using (new EditorGUILayout.HorizontalScope())
            {
                if (showPassword)
                {
                    password = EditorGUILayout.TextField("Password:", password);
                }
                else
                {
                    password = EditorGUILayout.PasswordField("Password:", password);
                }

                // Show/Hide toggle
                showPassword = EditorGUILayout.Toggle("Show", showPassword, GUILayout.Width(60));
            }

            EditorGUILayout.Space(15);

            // Buttons
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Cancel", GUILayout.Height(30)))
                {
                    callback?.Invoke(null);
                    this.Close();
                }

                GUI.backgroundColor = new Color(0.2f, 0.8f, 0.2f);
                if (GUILayout.Button("Submit", GUILayout.Height(30)))
                {
                    if (string.IsNullOrEmpty(password))
                    {
                        EditorUtility.DisplayDialog("Error", "Password cannot be empty", "OK");
                        return;
                    }

                    callback?.Invoke(password);
                    this.Close();
                }
                GUI.backgroundColor = Color.white;
            }

            // Handle Enter key
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
            {
                if (!string.IsNullOrEmpty(password))
                {
                    callback?.Invoke(password);
                    this.Close();
                    Event.current.Use();
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Password cannot be empty", "OK");
                }
            }
        }
    }
}
