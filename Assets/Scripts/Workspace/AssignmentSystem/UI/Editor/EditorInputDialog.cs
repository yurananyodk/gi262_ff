using UnityEngine;
using UnityEditor;
using System;

namespace Assignment.UI
{
    /// <summary>
    /// Utility class for showing text input dialogs in the editor
    /// </summary>
    public class EditorInputDialog : EditorWindow
    {
        private string value = "";
        private string title = "";
        private string label = "";
        private bool isPassword = false;
        private bool confirmed = false;
        private bool canceled = false;
        private static EditorInputDialog window;

        /// <summary>
        /// Shows a text input dialog with the specified title and label
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="label">Input field label</param>
        /// <param name="isPassword">Whether to mask input as password</param>
        /// <returns>The entered value or null if canceled</returns>
        public static string Show(string title, string label, bool isPassword = false)
        {
            window = GetWindow<EditorInputDialog>(true, title, true);
            window.titleContent = new GUIContent(title);
            window.title = title;
            window.label = label;
            window.isPassword = isPassword;
            window.minSize = new Vector2(300, 120);
            window.maxSize = new Vector2(300, 120);
            window.position = new Rect(
                (Screen.currentResolution.width - window.minSize.x) / 2,
                (Screen.currentResolution.height - window.minSize.y) / 2,
                window.minSize.x,
                window.minSize.y);
            window.ShowModal();

            // Wait for the user to close the dialog
            while (window != null && !window.confirmed && !window.canceled)
            {
                // Process events to prevent freezing
                EditorApplication.Step();
            }

            return window.confirmed ? window.value : null;
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            // Input field (regular or password)
            EditorGUILayout.Space(5);
            if (isPassword)
            {
                value = EditorGUILayout.PasswordField("", value);
            }
            else
            {
                value = EditorGUILayout.TextField("", value);
            }

            EditorGUILayout.Space(15);

            // Buttons
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Cancel", GUILayout.Height(30)))
                {
                    canceled = true;
                    Close();
                }

                GUI.backgroundColor = new Color(0.2f, 0.8f, 0.2f);
                if (GUILayout.Button("OK", GUILayout.Height(30)))
                {
                    confirmed = true;
                    Close();
                }
                GUI.backgroundColor = Color.white;
            }

            // Handle Enter key to confirm
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
            {
                confirmed = true;
                Close();
                Event.current.Use();
            }

            // Handle Escape key to cancel
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            {
                canceled = true;
                Close();
                Event.current.Use();
            }
        }
    }
}
