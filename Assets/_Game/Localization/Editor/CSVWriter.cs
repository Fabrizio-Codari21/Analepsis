using System.IO;
using UnityEditor;
using UnityEngine;

public static class CSVWriter 
{
    public static bool Write(CSVKey key, string content)
    {
        if (key == null)
        {
            Debug.LogError("No key provided");
            return false;
        }


        if (key.table == null)
        {
            Debug.LogError("No table provided");
            return false;
        }
        
        var assetPath = AssetDatabase.GetAssetPath(key.table);

        if (string.IsNullOrWhiteSpace(assetPath))
        {
            Debug.LogError($"[Localization] Cannot find asset path for " + $"'{key.table.name}'.");

            return false;
        }
        
        
        File.WriteAllText(assetPath, content,new System.Text.UTF8Encoding(true));
        
        AssetDatabase.ImportAsset(assetPath);
        AssetDatabase.Refresh();
        return true;
    }
}