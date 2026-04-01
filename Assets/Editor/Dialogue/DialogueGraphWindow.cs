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
    private List<Dialogue> openedDialogues = new();
    private Toolbar toolbar;
    private VisualElement dialogueTabsContainer;
    private VisualElement savePathContainer;
    private Dialogue currentDialogue;
    private DialogueGraphView graphView;
    [MenuItem("Tools/Dialogue Graph")]
    public static void Open()
    {
        DialogueGraphWindow window = GetWindow<DialogueGraphWindow>();
        window.titleContent = new GUIContent("Dialogue Graph");
        
        
    }
    
    public static void OpenWithDialogue(Dialogue dialogue)
    {
        DialogueGraphWindow window = GetWindow<DialogueGraphWindow>();
        window.titleContent = new GUIContent("Dialogue Graph");

        if (!window.openedDialogues.Contains(dialogue))
        {
            window.openedDialogues.Add(dialogue);
            window.RefreshTabs();
        }

        window.OpenDialogue(dialogue);
    }

    private void OnEnable()
    {
        defaultSavePath = EditorPrefs.GetString(DefaultSavePathKey, "Assets/Dialogues");

        if (!AssetDatabase.IsValidFolder(defaultSavePath))
        {
            defaultSavePath = "Assets/Dialogues";
        }

        CreateGraphView();
        CreateToolBar();
        RegisterDragAndDrop();
    }

    private void OnDisable()
    {
        rootVisualElement.Remove(graphView);
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

        Button changePathButton = new Button(() =>
        {
            ChangeDefaultPath();
        })
        {
            text = "Change Path"
        };

        savePathContainer.Add(savePathLabel);
        savePathContainer.Add(changePathButton);

        rootVisualElement.Add(savePathContainer);
    }
    
    private void CreateDialogueTabsBar()
    {
        dialogueTabsContainer = new VisualElement();
        dialogueTabsContainer.style.flexDirection = FlexDirection.Row;
        dialogueTabsContainer.style.height = 28;
        dialogueTabsContainer.style.marginTop = 4;
        dialogueTabsContainer.style.marginBottom = 4;
        dialogueTabsContainer.style.marginLeft = 4;
        dialogueTabsContainer.style.marginRight = 4;

        rootVisualElement.Add(dialogueTabsContainer);
    }
    public void RefreshTabs()
    {
        dialogueTabsContainer.Clear();

        foreach (var dialogue in openedDialogues)
        {
            Dialogue localDialogue = dialogue;

            Button tabButton = new Button(() =>
            {
                OpenDialogue(localDialogue);
            })
            {
                text = localDialogue.name
            };

            if (currentDialogue == localDialogue)
            {
                tabButton.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f);
            }

            dialogueTabsContainer.Add(tabButton);
        }
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
    
    private void OpenDialogue(Dialogue dialogue)
    {
        currentDialogue = dialogue;

        if (currentDialogue.startingNode == null)
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