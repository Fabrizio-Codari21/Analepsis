using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueGraphWindow : EditorWindow
{
    
    
    private string defaultSavePath = "Assets/Dialogues";
    private Label savePathLabel;
    private const string DefaultSavePathKey = "DialogueGraph_DefaultSavePath";
    
    private List<Dialogue> openedDialogues = new();  // 开启过的 dialogue
    private Toolbar toolbar;
    private VisualElement savePathContainer;
    private Dialogue currentDialogue;
    private DialogueGraphView graphView;
    private ScrollView dialogueTabsContainer;  // 所有已经开启的 dialogue tab
    #region Enable Disable
    private void OnEnable()
    {
        defaultSavePath = EditorPrefs.GetString(DefaultSavePathKey, "Assets/Dialogues");

        if (!AssetDatabase.IsValidFolder(defaultSavePath))
        {
            defaultSavePath = "Assets/Dialogues";
        }
        CreateToolBar();
        CreateGraphView();
        RegisterDragAndDrop();
    }

    private void OnDisable()
    {
        rootVisualElement.Remove(graphView);
    }
    #endregion

    
    [MenuItem("Tools/Dialogue Graph")]
    public static void Open()
    {
        DialogueGraphWindow window = GetWindow<DialogueGraphWindow>();
        window.titleContent = new GUIContent("Dialogue Graph");
    }
    
    public static void OpenWithDialogue(Dialogue dialogue)  // 根据dialogue 打开 dialogue window
    {
        DialogueGraphWindow window = GetWindow<DialogueGraphWindow>();  // unity editor 用来获取 或创建 自定义的 编辑器实例， 如果有 就返回该窗口的 实例，如果没有就创建新的
        window.titleContent = new GUIContent("Dialogue Graph");

        if (!window.openedDialogues.Contains(dialogue))  // 如果没有这个dialogue
        {
            window.openedDialogues.Add(dialogue);  // 加入这个dialogue
        }

        window.OpenDialogue(dialogue);
    }
    
    
    public void RefreshTabs()  // 刷新 tab
    {
        dialogueTabsContainer.Clear();  // 用 visualElement 的 clear 把 子元素全部 移除

        foreach (var dialogue in openedDialogues)
        {
            Dialogue localDialogue = dialogue;

            Button tabButton = new Button(() => OpenDialogue(localDialogue)) // 给 button 加入 点击后的 evt
            {
                text = localDialogue.name // 这个 tab 的名字
            };

            if (currentDialogue == localDialogue) // 如果当前的 dialogue 和 现在的 dialogue 是相同的话
            {
                tabButton.style.backgroundColor = Color.green;  // 他的 按钮背景变 成这个 颜色
                tabButton.style.color = Color.black;
            }

            dialogueTabsContainer.Add(tabButton);  // 把这个 tab 加入到 这个 visual  eleemnt 中
        }
    }
    
    private void OpenDialogue(Dialogue dialogue)  // 打开dialogue
    {

        if (dialogue != null && currentDialogue != dialogue) // 如果dialogue 不为空 同时当前的 dialogue 和 要开的 dialogue 不同的话，就要 refresh tab，要提前把
        {                                                      // current dialogue  设为 dialogue 正常referesh
            currentDialogue = dialogue;          
            RefreshTabs();
        }
        else
        {
            currentDialogue = dialogue; 
        }
        
        if (currentDialogue.startingNode == null) // 如果 当前的 dialogue 完全没有 dialogue
        {
            currentDialogue.startingNode = new DialogueNode
            {
                dialogueText = "Start Dialogue"
            };

            EditorUtility.SetDirty(currentDialogue);
            AssetDatabase.SaveAssets();
        }
        graphView.LoadDialogue(currentDialogue);
       
    }


    
   
    private void RegisterDragAndDrop()
    {
        rootVisualElement.RegisterCallback<DragUpdatedEvent>(evt =>
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
        });

        rootVisualElement.RegisterCallback<DragPerformEvent>(evt =>
        {
            DragAndDrop.AcceptDrag();

            foreach (Object draggedObject in DragAndDrop.objectReferences)
            {
                if (draggedObject is Dialogue dialogue)
                {
                    AddDialogueTab(dialogue);
                }
            }
        });
    }
    private void AddDialogueTab(Dialogue dialogue)
    {
        if (!openedDialogues.Contains(dialogue))
        {
            openedDialogues.Add(dialogue);
            RefreshTabs();
            OpenDialogue(dialogue);
            return;
        }

        if (currentDialogue != dialogue)
        {
            OpenDialogue(dialogue);
        }
    }

    private void CreateToolBar()
    {
        toolbar = new Toolbar();
        rootVisualElement.Add(toolbar);

        Button createDialogueButton = new Button(() =>
        {
            CreateNewDialogue(false);
        })
        {
            text = "New Dialogue"
        };

        Button quickCreateButton = new Button(() =>
        {
            CreateNewDialogue(true);
        })
        {
            text = "Quick Create"
        };

        toolbar.Add(createDialogueButton);
        toolbar.Add(quickCreateButton);

        CreateSavePathBar();
        CreateDialogueTabsBar();
    }
    
    private void CreateSavePathBar()
    {
        savePathContainer = new VisualElement();
        savePathContainer.style.flexDirection = FlexDirection.Row;
        savePathContainer.style.height = 24;
        savePathContainer.style.alignItems = Align.Center;
        savePathContainer.style.marginLeft = 4;
        savePathContainer.style.marginRight = 4;

        savePathLabel = new Label($"Save Path: {defaultSavePath}");
        savePathLabel.style.flexGrow = 1;

        Button changePathButton = new Button(ChangeDefaultPath)
        {
            text = "Change Path"
        };

        savePathContainer.Add(savePathLabel);
        savePathContainer.Add(changePathButton);

        rootVisualElement.Add(savePathContainer);
    }
    
    private void CreateDialogueTabsBar()
    {
        dialogueTabsContainer = new ScrollView(ScrollViewMode.Horizontal)
        {
            style =
            {
                height = 48,
                marginTop = 4,
                marginBottom = 4,
                marginLeft = 4,
                marginRight = 4
            }
        };

        dialogueTabsContainer.contentContainer.style.flexDirection = FlexDirection.Row;

        rootVisualElement.Add(dialogueTabsContainer);
    }
  
    
    private void ChangeDefaultPath()
    {
        string selectedPath = EditorUtility.OpenFolderPanel(
            "Select Default Dialogue Folder",
            "Assets",
            ""
        );

        if (string.IsNullOrEmpty(selectedPath))
            return;

        if (!selectedPath.StartsWith(Application.dataPath))
        {
            EditorUtility.DisplayDialog(
                "Invalid Folder",
                "Folder must be inside the Unity project's Assets folder.",
                "OK"
            );
            return;
        }

        string unityRelativePath =
            "Assets" + selectedPath.Substring(Application.dataPath.Length);

        if (!AssetDatabase.IsValidFolder(unityRelativePath))
        {
            EditorUtility.DisplayDialog(
                "Folder Not Found",
                $"The selected folder does not exist:\n\n{unityRelativePath}",
                "OK"
            );
            return;
        }

        defaultSavePath = unityRelativePath;

        EditorPrefs.SetString(DefaultSavePathKey, defaultSavePath);

        if (savePathLabel != null)
        {
            savePathLabel.text = $"Save Path: {defaultSavePath}";
        }
    }
    
 
    private void CreateNewDialogue(bool useDefaultPathDirectly)
    {
        string savePath = defaultSavePath;

        if (!AssetDatabase.IsValidFolder(savePath))
        {
            EditorUtility.DisplayDialog(
                "Invalid Save Path",
                $"The folder does not exist:\n\n{savePath}",
                "OK"
            );

            return;
        }

        if (!useDefaultPathDirectly)
        {
            bool useDefault = EditorUtility.DisplayDialog(
                "Create Dialogue",
                $"Do you want to save in default path?\n\n{defaultSavePath}",
                "Use Default",
                "Choose Folder"
            );

            if (!useDefault)
            {
                string selectedPath = EditorUtility.OpenFolderPanel(
                    "Choose Dialogue Folder",
                    "Assets",
                    ""
                );

                if (string.IsNullOrEmpty(selectedPath))
                    return;

                if (!selectedPath.StartsWith(Application.dataPath))
                {
                    EditorUtility.DisplayDialog(
                        "Invalid Folder",
                        "Folder must be inside the Unity project Assets folder.",
                        "OK"
                    );
                    return;
                }

                savePath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
            }
        }

        string fileName = EditorUtility.SaveFilePanelInProject(
            "Create Dialogue",
            "New Dialogue",
            "asset",
            "Enter dialogue file name",
            savePath
        );

        if (string.IsNullOrEmpty(fileName))
            return;

        Dialogue existing = AssetDatabase.LoadAssetAtPath<Dialogue>(fileName);

        if (existing != null)
        {
            EditorUtility.DisplayDialog(
                "File Exists",
                "A Dialogue asset with that name already exists.",
                "OK"
            );
            return;
        }

        Dialogue newDialogue = ScriptableObject.CreateInstance<Dialogue>();

        newDialogue.startingNode = new DialogueNode
        {
            dialogueText = "Start Dialogue",
            isRootNode = true
        };

        AssetDatabase.CreateAsset(newDialogue, fileName);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        AddDialogueTab(newDialogue);

        EditorGUIUtility.PingObject(newDialogue);
    }
    private void CreateGraphView()
    {
        graphView = new DialogueGraphView(this);
        rootVisualElement.Add(graphView);
    }
  
}