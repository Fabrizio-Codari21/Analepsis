using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class CaseSlot
{
    [ReadOnly] 
    public CaseSlotIdentity Identity;

    [ShowInInspector, ReadOnly]
    public string SlotTitle => Identity == null ? "(Missing Identity)" : Identity.Description;

    // 内部真实保存的数据（GUID 列表）
    [HideInInspector]
    public List<SerializableGuid> requiredClueGuids = new();

    // ========================================================================
    // Inspector 显示与下拉选择
    // 外面呈现格式：[GUID] [类型] / 名称
    // ========================================================================

    [ShowInInspector]
    [ListDrawerSettings(
        CustomAddFunction = nameof(OnAddNewClue), 
        OnTitleBarGUI = nameof(DrawCleanupButton),
        ShowIndexLabels = true
    )]
    [ValueDropdown(nameof(GetClueDropdownList), IsUniqueList = true, DropdownTitle = "Elegir Clue")]
    [LabelText("Clue")]
    private List<SerializableGuid> SelectedClues
    {
        get => requiredClueGuids;
        set => requiredClueGuids = value;
    }

    
    private IEnumerable<ValueDropdownItem<SerializableGuid>> GetClueDropdownList()
    {
        var list = new List<ValueDropdownItem<SerializableGuid>>();

#if UNITY_EDITOR
        var availableClues = ClueProvider.GetAvailableClues();
        foreach (var clueItem in availableClues)
        {
            IClue clue = clueItem.Value;
            if (clue == null) continue;

            SerializableGuid guid = clue.CompareGuid();
            

            list.Add(new ValueDropdownItem<SerializableGuid>(clueItem.Text, guid));
        }
#endif
        return list;
    }
    
    private void DrawCleanupButton()
    {
#if UNITY_EDITOR
        if (HasMissingClues)
        {
            if (Sirenix.Utilities.Editor.SirenixEditorGUI.IconButton(Sirenix.Utilities.Editor.EditorIcons.AlertTriangle))
            {
                CleanupMissingClues();
            }
        }
#endif
    }

    private void OnAddNewClue()
    {
        requiredClueGuids ??= new List<SerializableGuid>();
#if UNITY_EDITOR
        var firstAvailable = ClueProvider.GetAvailableClues().FirstOrDefault();
        if (firstAvailable.Value != null)
        {
            var guid = firstAvailable.Value.CompareGuid();
            if (!requiredClueGuids.Contains(guid))
            {
                requiredClueGuids.Add(guid);
            }
        }
#endif
    }

    // 检测是否有线索被彻底删除
    private bool HasMissingClues => requiredClueGuids != null && 
#if UNITY_EDITOR
        requiredClueGuids.Any(g => ClueProvider.GetClueByGuidInEditor(g) == null);
#else
        false;
#endif

    [ShowIf(nameof(HasMissingClues))]
    [InfoBox("警告：列表中有部分 Clue 已在源头（Item/NPC/Dialogue）中被删除！", InfoMessageType.Error)]
    [Button("一键清理已删除线索", ButtonSizes.Medium), GUIColor(1f, 0.4f, 0.4f)]
    public void CleanupMissingClues()
    {
#if UNITY_EDITOR
        if (requiredClueGuids == null) return;
        requiredClueGuids.RemoveAll(guid => ClueProvider.GetClueByGuidInEditor(guid) == null);
        Sirenix.Utilities.Editor.GUIHelper.RequestRepaint();
#endif
    }

    // 运行时逻辑校验
    public bool Validate(IClue clue)
    {
        if (clue == null) return false;
        if (requiredClueGuids == null || requiredClueGuids.Count == 0) return true;

        return requiredClueGuids.Contains(clue.CompareGuid());
    }
}