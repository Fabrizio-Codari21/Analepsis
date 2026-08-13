using UnityEngine;
using System;
[CreateAssetMenu(fileName = "Loacalization_LanguageName", menuName = "Loacalization/Language List")]
public class LanguageList : ScriptableObject
{
    [Header("All Language")]
    public Language[] languages;


    [Header("Setting")]
    public Language defaultLanguage;
    public Language fallbackLanguage;




    public bool TryGetLanguage(string langName, out Language language)
    {
        language = null;

        if (languages == null) return false;


        foreach (var item in languages)
        {
            if(item == null) continue;
            
            if (!string.Equals(item.languageCode, langName, StringComparison.OrdinalIgnoreCase)) continue;

            language = item;
            return true;
        }
        
        return false;
    }
    public int IndexOf(Language language)
    {
        if (languages == null) return -1;

        return Array.IndexOf(languages, language);
    }
    
    public bool Contains(Language language)
    {
        if (language == null || languages == null)
            return false;

        foreach (var item in languages)
        {
            if (item == language) return true;
        }

        return false;
    }
}