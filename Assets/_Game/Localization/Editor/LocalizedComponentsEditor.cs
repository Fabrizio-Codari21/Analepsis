using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LocalizedComponents), true)]
public class LocalizedComponentsEditor : OdinEditor
{
    
    private Dictionary<string, string> _editValues;
    private Dictionary<string, string> _originalValues;

    private string _loadedPreviewId;
    private CSVKey _loadedPreviewKey;
    private SerializedProperty _keyProperty;
    private SerializedProperty _idProperty;

    private string _newId;

    protected override void OnEnable()
    {
        base.OnEnable();

        _keyProperty =
            serializedObject.FindProperty("m_key");

        _idProperty =
            serializedObject.FindProperty("m_id");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawCSVKey();
        DrawLocalizationId();
        DrawEntryPreview();

        serializedObject.ApplyModifiedProperties();

        DrawPropertiesExcluding(
            serializedObject,
            "m_Script",
            "m_key",
            "m_id");
    }
    private void DrawEntryPreview()
    {
        var csvKey =
            _keyProperty.objectReferenceValue as CSVKey;

        var id =
            _idProperty.stringValue;

        if (csvKey == null ||
            csvKey.table == null ||
            string.IsNullOrWhiteSpace(id))
        {
            return;
        }

        
        if (_loadedPreviewKey != csvKey ||
            _loadedPreviewId != id ||
            _editValues == null)
        {
            LoadPreview(csvKey, id);
        }

        if (_editValues == null ||
            _editValues.Count == 0)
        {
            return;
        }

        EditorGUILayout.Space();

        EditorGUILayout.LabelField(
            "Localization Preview",
            EditorStyles.boldLabel);

        if (HasUnsavedChanges())
        {
            EditorGUILayout.HelpBox(
                "Unsaved localization changes.",
                MessageType.Warning);
        }

        var headers =
            new List<string>(_editValues.Keys);

        foreach (var header in headers)
        {
            if (header.Equals(
                    "Key",
                    System.StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            DrawEditablePreviewField(header);
        }

        DrawPreviewButtons(csvKey, id);
    }
    
    private void DrawEditablePreviewField(
        string header)
    {
        EditorGUILayout.LabelField(
            header,
            EditorStyles.boldLabel);

        _editValues[header] =
            EditorGUILayout.TextArea(
                _editValues[header] ?? string.Empty,
                GUILayout.MinHeight(40f));

        EditorGUILayout.Space();
    }
    
    private void DrawPreviewButtons(
        CSVKey csvKey,
        string id)
    {
        EditorGUILayout.BeginHorizontal();

        using (new EditorGUI.DisabledScope(
                   !HasUnsavedChanges()))
        {
            if (GUILayout.Button("Revert"))
            {
                LoadPreview(csvKey, id);
            }

            if (GUILayout.Button("Save"))
            {
                SavePreview(csvKey, id);
            }
        }

        EditorGUILayout.EndHorizontal();
    }
    private static void DrawPreviewField(
        string label,
        string value)
    {
        EditorGUILayout.BeginVertical(
            EditorStyles.helpBox);

        EditorGUILayout.LabelField(
            label,
            EditorStyles.boldLabel);

        EditorGUILayout.SelectableLabel(
            string.IsNullOrEmpty(value)
                ? "(Empty)"
                : value,
            EditorStyles.wordWrappedLabel,
            GUILayout.MinHeight(20f));

        EditorGUILayout.EndVertical();
    }
    private void SavePreview(
        CSVKey csvKey,
        string id)
    {
        if (!LocalizationCSVEditor.UpdateKey(
                csvKey,
                id,
                _editValues))
        {
            return;
        }

        LoadPreview(csvKey, id);

        Repaint();
    }
    private void LoadPreview(
        CSVKey csvKey,
        string id)
    {
        _editValues =
            LocalizationInspectorUtility.GetEntry(
                csvKey,
                id);

        _originalValues =
            new Dictionary<string, string>(_editValues);

        _loadedPreviewKey = csvKey;
        _loadedPreviewId = id;
    }
    
    private bool HasUnsavedChanges()
    {
        if (_editValues == null ||
            _originalValues == null)
        {
            return false;
        }

        if (_editValues.Count != _originalValues.Count)
            return true;

        foreach (var pair in _editValues)
        {
            if (!_originalValues.TryGetValue(
                    pair.Key,
                    out var original))
            {
                return true;
            }

            if (pair.Value != original)
                return true;
        }

        return false;
    }

    /// <summary>
    /// 绘制 CSVKey 选择。
    /// </summary>
    private void DrawCSVKey()
    {
        EditorGUILayout.PropertyField(
            _keyProperty,
            new GUIContent("CSV Key"));
    }

    /// <summary>
    /// 绘制 Localization ID 区域。
    ///
    /// 根据当前 CSVKey：
    /// - 显示已有 ID Dropdown
    /// - 检查当前 ID 是否仍然存在
    /// - 提供新增 Key 功能
    /// </summary>
    private void DrawLocalizationId()
    {
        var csvKey =
            _keyProperty.objectReferenceValue as CSVKey;

        if (csvKey == null || csvKey.table == null)
        {
            EditorGUILayout.HelpBox(
                "Select a CSV Key first.",
                MessageType.Info);

            return;
        }

        var keys =
            LocalizationInspectorUtility.GetKeys(csvKey);

        DrawKeyDropdown(
            csvKey,
            keys);

        DrawAddKey(csvKey);
    }

    private void DrawKeyDropdown(
        CSVKey csvKey,
        List<string> keys)
    {
        var currentId =
            _idProperty.stringValue;

        if (keys.Count == 0)
        {
            EditorGUILayout.HelpBox(
                "No Localization Keys found in this CSV.",
                MessageType.Warning);

            DrawMissingKey(
                csvKey,
                currentId);

            return;
        }

        var currentIndex =
            keys.IndexOf(currentId);

        if (currentIndex < 0)
        {
            EditorGUILayout.LabelField(
                "Current ID",
                string.IsNullOrWhiteSpace(currentId)
                    ? "(None)"
                    : currentId);

            DrawMissingKey(
                csvKey,
                currentId);

            DrawKeySelector(keys);

            return;
        }

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.PrefixLabel(
            "Localization ID");

        if (GUILayout.Button(
                currentId,
                EditorStyles.popup))
        {
            ShowKeySelector(keys);
        }

        if (GUILayout.Button(
                "Edit",
                GUILayout.Width(50f)))
        {
            LocalizationAddKeyWindow.OpenEdit(
                csvKey,
                currentId,
                target as LocalizedComponents);
        }

        EditorGUILayout.EndHorizontal();
    }
    
    private void ShowKeySelector(List<string> keys)
    {
        var component =
            target as LocalizedComponents;

        var currentId =
            _idProperty.stringValue;

        var selector =
            new GenericSelector<string>(
                "Select Localization ID",
                false,
                keys);

        if (!string.IsNullOrWhiteSpace(currentId))
        {
            selector.SetSelection(currentId);
        }

        selector.SelectionConfirmed += selection =>
        {
            foreach (var selected in selection)
            {
                ApplyId(component, selected);
                break;
            }
        };

        selector.ShowInPopup();
    }
    private static void ApplyId(LocalizedComponents component, string id)
    {
        if (component == null)
            return;

        var serializedTarget =
            new SerializedObject(component);

        var idProperty =
            serializedTarget.FindProperty("m_id");

        if (idProperty == null)
            return;

        idProperty.stringValue = id;

        serializedTarget.ApplyModifiedProperties();

        EditorUtility.SetDirty(component);
    }
    private void DrawKeySelector(
        List<string> keys)
    {
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.PrefixLabel(
            "Select ID");

        if (GUILayout.Button(
                "Select Localization ID...",
                EditorStyles.popup))
        {
            ShowKeySelector(keys);
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawMissingKey(
        CSVKey csvKey,
        string currentId)
    {
        if (string.IsNullOrWhiteSpace(currentId))
            return;

        EditorGUILayout.HelpBox(
            $"Localization Key '{currentId}' does not exist " +
            $"in '{csvKey.name}'.",
            MessageType.Error);

        if (!GUILayout.Button(
                $"Recreate '{currentId}'"))
        {
            return;
        }

        LocalizationAddKeyWindow.Open(
            csvKey,
            currentId,
            target as LocalizedComponents);
    }

    private void DrawAddKey(CSVKey csvKey)
    {
        EditorGUILayout.Space();

        EditorGUILayout.LabelField(
            "Create New Localization Key",
            EditorStyles.boldLabel);

        _newId =
            EditorGUILayout.TextField(
                "New ID",
                _newId);

        using (new EditorGUI.DisabledScope(
                   string.IsNullOrWhiteSpace(_newId)))
        {
            if (!GUILayout.Button("Add New Key"))
                return;
        }

        var newId =
            _newId.Trim();

        LocalizationAddKeyWindow.Open(
            csvKey,
            newId,
            target as LocalizedComponents);

        _newId = string.Empty;
    }

}