using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Dialogue))]
public class DialogueInspectorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);

        if (GUILayout.Button("Open Dialogue Graph"))
        {
            Dialogue dialogue = (Dialogue)target;

            DialogueGraphWindow.OpenWithDialogue(dialogue);
        }
    }

    [UnityEditor.Callbacks.OnOpenAsset]
    public static bool OnOpenAsset(int instanceID, int line)
    {
        Object obj = EditorUtility.InstanceIDToObject(instanceID);

        if (obj is Dialogue dialogue)
        {
            DialogueGraphWindow.OpenWithDialogue(dialogue);
            return true;
        }

        return false;
    }
}