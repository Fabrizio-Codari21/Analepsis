using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Dialogue))]
public class DialogueInspectorEditor : Editor
{
    public override void OnInspectorGUI()
    {
       
        DrawDefaultInspector();

        GUILayout.Space(15);
        
    
        Dialogue dialogue = (Dialogue)target;

      
     
        GUI.backgroundColor = new Color(0.3f, 0.6f, 0.9f); // 给按钮换个好看的蓝色，防止误触
        if (GUILayout.Button("🔄 Actualizar Todos los GUIDs (Resolver Conflicto)", GUILayout.Height(35)))
        {
            // 弹出一个二次确认窗口，防止不小心点错冲刷了存档标识
            if (EditorUtility.DisplayDialog("Actualizar GUIDs", 
                $"¿Seguro que quieres regenerar todos los GUIDs de los nodos en '{dialogue.name}'?\n\nSi copiaste este diálogo por Ctrl+C/V, esto resolverá el problema de que se compartan las marcas y nombres.", 
                "Sí, actualizar", "Cancelar"))
            {
                int updatedCount = 0;

                // 刷新 Starting Node
                if (dialogue.startingNode != null)
                {
                    dialogue.startingNode.guid = SerializableGuid.NewGuid();
                    updatedCount++;
                }

         
                foreach (var node in dialogue.allNodes)
                {
                    if (node != null)
                    {
                        node.guid = SerializableGuid.NewGuid();
                        updatedCount++;
                    }
                }

              
                EditorUtility.SetDirty(dialogue);
                AssetDatabase.SaveAssets();

                Debug.Log($"<color=green>【GUID Actualizado Exitosamente】</color> Se han regenerado {updatedCount} GUIDs para el diálogo: <b>{dialogue.name}</b>.");
            }
        }

        GUI.backgroundColor = Color.white; 

        GUILayout.Space(5);

 
        if (GUILayout.Button("Open Dialogue Graph", GUILayout.Height(30)))
        {
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