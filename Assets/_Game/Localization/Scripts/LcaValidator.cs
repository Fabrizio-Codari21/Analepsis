using System;
using System.Collections.Generic;
using UnityEngine;

public static class LcaValidator
{

    
    public static bool ValidateDuplicateKey(List<string[]> rows, int keyColumn, string sourceName)
    {
        var keys = new HashSet<string>();

        var valid = true;


     
        for (int i = 1; i < rows.Count; i++)
        {
            var row = rows[i];
            
            if (keyColumn < 0 || keyColumn >= row.Length) continue;
            
            var key= row[keyColumn].Trim();
            
            if(string.IsNullOrWhiteSpace(key)) continue;
            
            if(keys.Add(key)) continue;
            
            Debug.LogError($"[Localization] Duplicate Key '{key}' " + $"in '{sourceName}' at row {i + 1}.");

            valid = false;
        }
        
        return valid;


    }
    
    public static bool ValidateHeaderDuplicates(
        string[] headerRow,
        string sourceName)
    {
        var headers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var valid = true;

        foreach (var value in headerRow)
        {
            var header = value.Trim();
            
            if (string.IsNullOrWhiteSpace(header))
                continue;

            if (headers.Add(header))
                continue;

            Debug.LogError(
                $"[Localization] Duplicate Header '{header}' " +
                $"in '{sourceName}'.");

            valid = false;
        }

        return valid;
    }
    
    
    
    public static bool ValidateLanguageList(LanguageList languageList)
{
    if (languageList == null)
    {
        Debug.LogError(
            "[Localization] LanguageList is null.");

        return false;
    }

    if (languageList.languages == null ||
        languageList.languages.Length == 0)
    {
        Debug.LogError(
            "[Localization] LanguageList has no languages.");

        return false;
    }

    var valid = true;

    var uniqueCodes = new HashSet<string>(
        StringComparer.OrdinalIgnoreCase);

    foreach (var language in languageList.languages)
    {
        if (language == null)
        {
            Debug.LogError(
                "[Localization] LanguageList contains a null Language.");

            valid = false;
            continue;
        }

        if (string.IsNullOrWhiteSpace(language.languageCode))
        {
            Debug.LogError($"[Localization] '{language.name}' LanguageCode is empty.");

            valid = false;
            continue;
        }

        if (uniqueCodes.Add(language.languageCode)) continue;

        Debug.LogError($"[Localization] Duplicate LanguageCode: " + $"'{language.languageCode}'.");

        valid = false;
    }

    if (languageList.defaultLanguage == null)
    {
        Debug.LogError("[Localization] Default Language is null.");

        valid = false;
    }
    else if (!languageList.Contains(languageList.defaultLanguage))
    {
        Debug.LogError($"[Localization] Default Language " + $"'{languageList.defaultLanguage.name}' " + $"is not registered in LanguageList.");

        valid = false;
    }

    if (languageList.fallbackLanguage == null)
    {
        Debug.LogError("[Localization] Fallback Language is null.");

        valid = false;
    }
    else if (!languageList.Contains(languageList.fallbackLanguage))
    {
        Debug.LogError($"[Localization] Fallback Language " + $"'{languageList.fallbackLanguage.name}' " + $"is not registered in LanguageList.");

        valid = false;
    }

    return valid;
}
    
    public static bool ValidateTranslation(string text,
        string key,
        string languageCode,
        string sourceName,
        int rowNumber)
    {
        if (!string.IsNullOrWhiteSpace(text))
            return true;

        Debug.LogWarning(
            $"[Localization] Empty translation for Key '{key}' " +
            $"Language '{languageCode}' in '{sourceName}' " +
            $"at row {rowNumber}.");

        return false;
    }
    
    public static bool ValidateFallbackLanguage(Dictionary<Language, int> languageColumns, Language fallbackLanguage, string sourceName)
    {
        if (languageColumns.ContainsKey(fallbackLanguage))return true;

        Debug.LogError($"[Localization] Fallback Language " + $"'{fallbackLanguage.languageCode}' " + $"is missing from '{sourceName}'.");

        return false;
    }

    public static bool ValidateCSVKey(List<CSVKey> csvKeys)
    {
        if (csvKeys == null || csvKeys.Count == 0)
        {
            Debug.LogError("[Localization] CSV Key list is empty.");

            return false;
        }
        
        var uniqueKeys = new HashSet<CSVKey>();
        var valid = true;

        foreach (var key in csvKeys)
        {
            if (key == null)
            {
                Debug.LogWarning("[Localization] CSV Key List Contains Null/Empty Key."); valid = false;
                continue;
            }

            if (key.table == null)
            {
                Debug.LogError($"[Localization] CSVKey '{key.name}' has no TextAsset assigned.");

                valid = false;
                continue;
            }
            
            
            
            if (uniqueKeys.Add(key)) continue;

            Debug.LogError($"[Localization] Duplicate CSVKey '{key.name}'.");
            valid = false;
        }
        
        
        return valid;
    }
    
    public static bool ValidateHeader(
        int keyColumn,
        int languageCount,
        string sourceName)
    {
        var valid = true;

        if (keyColumn < 0)
        {
            Debug.LogError($"[Localization] Missing 'Key' column in '{sourceName}'.");

            valid = false;
        }

        if (languageCount <= 0)
        {
            Debug.LogError($"[Localization] No valid language columns found in '{sourceName}'.");

            valid = false;
        }

        return valid;
    }
    
    public static bool ValidateKey(
        string key,
        string sourceName,
        int rowNumber)
    {
        if (!string.IsNullOrWhiteSpace(key))
            return true;

        Debug.LogWarning(
            $"[Localization] Missing Key in '{sourceName}' at row {rowNumber}.");
        return false;
    }
}