using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueGraphWindow : EditorWindow
{
    
    private List<Dialogue> openedDialogues = new();
    private Toolbar toolbar;
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
    }

    private void RefreshTabs()
    {
        toolbar.Clear();

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

            toolbar.Add(tabButton);
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
    private void CreateGraphView()
    {
        graphView = new DialogueGraphView(this);
        rootVisualElement.Add(graphView);
    }
}