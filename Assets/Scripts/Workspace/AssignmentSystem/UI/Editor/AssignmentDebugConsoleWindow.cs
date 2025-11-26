using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using AssignmentSystem.Services;

namespace Assignment.UI
{
    /// <summary>
    /// Custom editor window to display AssignmentDebugConsole output.
    /// </summary>
    public class AssignmentDebugConsoleWindow : EditorWindow
    {
        private Vector2 _scrollPosition;
        private string _filter = "";
        private bool _showTimestamp = false;

        [MenuItem("Assignment/Debug Console")]
        public static void ShowWindow()
        {
            GetWindow<AssignmentDebugConsoleWindow>("Assignment Debug Console");
        }

        private void OnGUI()
        {
            GUILayout.Space(8);
            GUILayout.Label("Assignment Debug Console", EditorStyles.largeLabel);
            GUILayout.Space(4);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Filter:", GUILayout.Width(40));
            _filter = GUILayout.TextField(_filter, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Clear", GUILayout.Width(60)))
            {
                AssignmentDebugConsole.Clear();
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(8);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Message", EditorStyles.boldLabel);
            _showTimestamp = GUILayout.Toggle(_showTimestamp, "Show Timestamps", GUILayout.Width(120));
            GUILayout.EndHorizontal();

            var entries = AssignmentDebugConsole.GetEntries();
            List<AssignmentDebugConsole.LogEntry> filteredEntries = new List<AssignmentDebugConsole.LogEntry>();
            foreach (var entry in entries)
            {
                if (string.IsNullOrEmpty(_filter) || entry.Message.ToLower().Contains(_filter.ToLower()))
                {
                    filteredEntries.Add(entry);
                }
            }

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
            foreach (var entry in filteredEntries)
            {
                GUILayout.BeginHorizontal();
                if (_showTimestamp)
                {
                    GUILayout.Label($"{entry.Timestamp:HH:mm:ss}", GUILayout.Width(80));
                }
                if (GUILayout.Button(entry.Message, EditorStyles.linkLabel))
                {
                    NavigateToSource(entry.StackTrace);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
        }

        private void NavigateToSource(string stackTrace)
        {
            Debug.Log($"Navigating to source for stack trace: {stackTrace}");
            // Try to parse the first stack frame with file and line info
            if (string.IsNullOrEmpty(stackTrace)) { Debug.Log("no stacktrace"); return; }
            var lines = stackTrace.Split('\n');
            foreach (var line in lines)
            {
                // Example: at Namespace.Class.Method() in C:\Path\To\File.cs:181
                int inIndex = line.IndexOf(" in ");
                int colonIndex = line.LastIndexOf(":");
                if (inIndex > 0 && colonIndex > inIndex)
                {
                    string filePath = line[(inIndex + 4)..colonIndex].Trim();
                    string lineNumStr = line[(colonIndex + 1)..].Trim();
                    if (int.TryParse(lineNumStr, out int lineNum))
                    {
                        UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(filePath, lineNum);
                        break;
                    }
                }
            }
        }
    }
}
