using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LocalizationAddKeyWindow : EditorWindow
{
    private bool _isEditMode;
    private CSVKey _csvKey;
    private string _id;
    private readonly Dictionary<string, string> _values = new();

    private string[] _headers;
    
    private Vector2 _scrollPosition;
    
    private System.Action<string> _onAdded;
    
    private LocalizedComponents _target;

    public static void Open(
        CSVKey csvKey,
        string id,
        LocalizedComponents target)
    {
        var window =
            CreateInstance<LocalizationAddKeyWindow>();

        window._csvKey = csvKey;
        window._id = id;
        window._target = target;

        window.LoadHeaders();

        window.titleContent =
            new GUIContent("Add Localization Key");

        window.minSize =
            new Vector2(480f, 420f);

        window.ShowUtility();
    }

    private void LoadHeaders()
    {
        if (_csvKey == null ||
            _csvKey.table == null)
        {
            return;
        }

        var rows =
            CSVUtility.ReadCSV(
                _csvKey.table.text);

        if (rows.Count == 0)
            return;

        var columnCount =
            CSVUtility.GetEffectiveColumnCount(
                rows[0]);

        _headers =
            new string[columnCount];

        for (var i = 0; i < columnCount; i++)
        {
            _headers[i] =
                rows[0][i].Trim();
        }
    }
    
    
    public static void OpenEdit(
        CSVKey csvKey,
        string id,
        LocalizedComponents target)
    {
        var window =
            CreateInstance<LocalizationAddKeyWindow>();

        window._csvKey = csvKey;
        window._id = id;
        window._target = target;
        window._isEditMode = true;

        window.LoadHeaders();
        window.LoadValues();

        window.titleContent =
            new GUIContent("Edit Localization Key");

        window.minSize =
            new Vector2(480f, 420f);

        window.ShowUtility();
    }
    private void LoadValues()
    {
        if (_csvKey == null ||
            _csvKey.table == null)
        {
            return;
        }

        var rows =
            CSVUtility.ReadCSV(
                _csvKey.table.text);

        if (rows.Count == 0)
            return;

        var header =
            rows[0];

        var keyColumn =
            CSVHeaderParser.FindKeyColumn(header);

        if (keyColumn < 0)
            return;

        var row =
            FindRow(
                rows,
                keyColumn,
                _id);

        if (row == null)
            return;

        var columnCount =
            CSVUtility.GetEffectiveColumnCount(header);

        for (var column = 0;
             column < columnCount;
             column++)
        {
            if (column == keyColumn)
                continue;

            var headerName =
                header[column].Trim();

            if (string.IsNullOrWhiteSpace(headerName))
                continue;

            _values[headerName] =
                column < row.Length
                    ? row[column]
                    : string.Empty;
        }
    }

    private static string[] FindRow(
        List<string[]> rows,
        int keyColumn,
        string id)
    {
        for (var i = 1; i < rows.Count; i++)
        {
            var row = rows[i];

            if (keyColumn >= row.Length)
                continue;

            if (row[keyColumn].Trim() == id)
                return row;
        }

        return null;
    }
    private void OnGUI()
    {
        if (_csvKey == null || _headers == null)
        {
            EditorGUILayout.HelpBox(
                "Invalid CSV.",
                MessageType.Error);

            return;
        }

        EditorGUILayout.LabelField(
            "Create Localization Key",
            EditorStyles.boldLabel);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField(
            "CSV",
            _csvKey.name);

        EditorGUILayout.LabelField(
            "Key",
            _id);

        EditorGUILayout.Space();

        _scrollPosition =
            EditorGUILayout.BeginScrollView(_scrollPosition);

        DrawValues();

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        DrawButtons();
    }
    private void DrawValues()
    {
        foreach (var header in _headers)
        {
            if (string.IsNullOrWhiteSpace(header))
                continue;

            if (header.Equals(
                    "Key",
                    System.StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            _values.TryGetValue(
                header,
                out var value);

            value =
                EditorGUILayout.TextField(
                    header,
                    value);

            _values[header] = value;
        }
    }

    private void DrawButtons()
    {
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Cancel"))
        {
            Close();
        }

        if (GUILayout.Button(
                _isEditMode ? "Save" : "Add"))
        {
            Save();
        }

        EditorGUILayout.EndHorizontal();
    }

    private void Save()
    {
        var success =
            _isEditMode
                ? LocalizationCSVEditor.UpdateKey(
                    _csvKey,
                    _id,
                    _values)
                : LocalizationCSVEditor.AddKey(
                    _csvKey,
                    _id,
                    _values);

        if (!success)
            return;

        ApplyIdToTarget();

        AssetDatabase.SaveAssets();

        Close();
    }
    private void ApplyIdToTarget()
    {
        if (_target == null)
            return;

        var serializedTarget =
            new SerializedObject(_target);

        serializedTarget.Update();

        var idProperty =
            serializedTarget.FindProperty("m_id");

        if (idProperty == null)
            return;

        idProperty.stringValue = _id;

        serializedTarget.ApplyModifiedProperties();

        EditorUtility.SetDirty(_target);
    }
}