using System;
using System.Collections.Generic;
using System.Text;
using Sirenix.OdinInspector;
using UnityEngine;


public class LcaManager : PersistentSingleton<LcaManager>
{
    [Header("Configuration")] [SerializeField]
    private LanguageList languageList;

    [SerializeField] private List<CSVKey> csvKeys;
    [ReadOnly, ShowInInspector] private Language currentLanguage;

    [Header("CSV DATA")] private Dictionary<CSVKey, Dictionary<Language, Dictionary<string, string>>> tables = new();


    private int _currentLanguageIndex;
    public event Action OnLanguageChanged = delegate { };

    protected override void Awake()
    {
        base.Awake();

        if (!LcaValidator.ValidateLanguageList(languageList))
            return;

        if (!LcaValidator.ValidateCSVKey(csvKeys))
            return;

        if (!LoadCsvToDictionary())
            return;

        currentLanguage = languageList.defaultLanguage;
        _currentLanguageIndex =
            languageList.IndexOf(currentLanguage);
    }


    public void ModifyLanguage(int dir = 1)
    {
        var count = languageList.languages.Length;

        _currentLanguageIndex = ((_currentLanguageIndex + dir) % count + count) % count;

        ModifyLanguage(languageList.languages[_currentLanguageIndex]);
    }

    public void ModifyLanguage(Language toLanguage)
    {
        if (!CanChangeLanguage(toLanguage))
            return;

        currentLanguage = toLanguage;
        _currentLanguageIndex = languageList.IndexOf(toLanguage);

        OnLanguageChanged.Invoke();
    }
    
    private bool CanChangeLanguage(Language newLanguage)
    {
        if (newLanguage == null)
        {
            Debug.LogWarning("[Localization] Cannot change language. Language is null.");

            return false;
        }

        if (newLanguage == currentLanguage) return false;

        if (!languageList.Contains(newLanguage))
        {
            Debug.LogWarning($"[Localization] Language '{newLanguage.name}' " + $"is not registered in LanguageList.");

            return false;
        }

        return true;
    }

    
    private bool HasLanguage(CSVKey csvKey, Language language)
    {
        if (csvKey == null || language == null) return false;

        return tables.TryGetValue(csvKey, out var languageTable) && languageTable.ContainsKey(language);
    }
    private bool LoadCsvToDictionary()
    {
        tables.Clear();

        var valid = true;

        foreach (var csvKey in csvKeys)
        {
            if (csvKey == null || csvKey.table == null)
            {
                valid = false;
                continue;
            }

            var rows = CSVUtility.ReadCSV(csvKey.table.text);

            if (rows.Count == 0)
            {
                Debug.LogError($"[Localization] CSV '{csvKey.name}' is empty.");

                valid = false;
                continue;
            }

            if (!LcaValidator.ValidateHeaderDuplicates(rows[0], csvKey.name))
            {
                valid = false;
                continue;
            }

            var header = CSVHeaderParser.Parse(rows[0], languageList);
            
            if (!LcaValidator.ValidateHeader(header.keyColumn, header.languageColumns.Count, csvKey.name))
            {
                valid = false;
                continue;
            }

            if (!LcaValidator.ValidateFallbackLanguage(header.languageColumns, languageList.fallbackLanguage, csvKey.name))
            {
                valid = false;
                continue;
            }

            if (!LcaValidator.ValidateDuplicateKey(rows, header.keyColumn, csvKey.name))
            {
                valid = false;
                continue;
            }

            tables[csvKey] = BuildTable(rows, header, csvKey.name);
        }

        return valid;
    }


    private Dictionary<Language, Dictionary<string, string>> BuildTable(List<string[]> rows, CSVHeader header, string sourceName)
    {
        var table = new Dictionary<Language, Dictionary<string, string>>();

        CreateLanguageTables(table, header);

        for (var rowIndex = 1; rowIndex < rows.Count; rowIndex++)
        {
            AddLocalizationRow(table, rows[rowIndex], header, sourceName, rowIndex + 1);
        }

        return table;
    }


    private void CreateLanguageTables(Dictionary<Language, Dictionary<string, string>> table, CSVHeader header)
    {
        foreach (var language in header.languageColumns.Keys) table.Add(language, new Dictionary<string, string>());
    }

    private void AddLocalizationRow(Dictionary<Language, Dictionary<string, string>> table, string[] row, CSVHeader header, string sourceName, int rowNumber)
    {
        if (header.keyColumn >= row.Length)
            return;

        var key = row[header.keyColumn].Trim();

        if (!LcaValidator.ValidateKey(
                key,
                sourceName,
                rowNumber))
        {
            return;
        }

        foreach (var pair in header.languageColumns)
        {
            var language = pair.Key;
            var column = pair.Value;

            var text = GetCell(row, column);

            LcaValidator.ValidateTranslation(
                text,
                key,
                language.languageCode,
                sourceName,
                rowNumber);

            table[language].Add(key, text);
        }
    }

    private static string GetCell(string[] row, int column)
    {
        return column < row.Length ? row[column] : string.Empty;
    }


 
    public string TranslateText(CSVKey key, string id)
    {
        var tableName = GetTableName(key);

        if (!HasTable(key))
        {
            Debug.LogError($"[Localization] Table '{tableName}' is not loaded.");

            return $"[Localization Error] Table Not Loaded : {tableName}";
        }

        if (!ContainsKey(key, id))
        {
            Debug.LogError($"[Localization] Key '{id}' does not exist " + $"in table '{tableName}'.");

            return $"[Localization Error] Missing Key : {tableName} : {id}";
        }

        if (!HasLanguage(key, currentLanguage))
        {
            Debug.LogWarning($"[Localization] Language '{currentLanguage.languageCode}' " + $"does not exist in table '{tableName}'. Using fallback.");

            return GetFallbackTranslation(key, id, tableName);
        }

        if (TryGetValidTranslation(
                key,
                currentLanguage,
                id,
                out var result))
        {
            return result;
        }

        Debug.LogWarning($"[Localization] Translation for '{id}' is empty " + $"in language '{currentLanguage.languageCode}'. Using fallback.");

        return GetFallbackTranslation(key, id, tableName);
    }
    private string GetFallbackTranslation(CSVKey key, string id, string tableName)
    {
        if (TryGetValidTranslation(key, languageList.fallbackLanguage, id, out var fallback))
        {
            return fallback;
        }

        Debug.LogError($"[Localization] Fallback translation for '{id}' " + $"is empty in table '{tableName}'.");

        return $"[Localization Error] Empty Fallback : {tableName} : {id}";
    }
    
    
    private static string GetTableName(CSVKey csvKey)
    {
        if (csvKey == null) return "Null CSVKey";

        if (csvKey.table == null) return csvKey.name;

        return csvKey.table.name;
    }
    
    


    private bool TryGetValidTranslation(CSVKey csvKey, Language language, string id, out string result)
    {
        if (!TryGetTranslation(csvKey, language, id, out result)) return false;


        return !string.IsNullOrWhiteSpace(result);
    }


    private bool TryGetTranslation(CSVKey csvKey, Language language, string id, out string result)
    {
        result = null;
        if (csvKey == null || language == null || string.IsNullOrWhiteSpace(id)) return false;


        if (!tables.TryGetValue(csvKey, out var languageTable)) return false;


        return languageTable.TryGetValue(language, out var idTable) && idTable.TryGetValue(id, out result);
    }


    
    private bool HasTranslationKey(CSVKey csvKey, Language language, string id)
    {
        if (csvKey == null || language == null || string.IsNullOrWhiteSpace(id))
        {
            return false;
        }

        if (!tables.TryGetValue(csvKey, out var languageTable))
        {
            return false;
        }

        if (!languageTable.TryGetValue(language, out var idTable))
        {
            return false;
        }

        return idTable.ContainsKey(id);
    }
 
    
    public bool HasTable(CSVKey csvKey)
    {
        return csvKey != null && tables.ContainsKey(csvKey);
    }

    public bool ContainsKey(CSVKey csvKey, string id)
    {
        if (csvKey == null || string.IsNullOrWhiteSpace(id)) return false;

        if (!tables.TryGetValue(csvKey, out var languageTable)) return false;

        foreach (var idTable in languageTable.Values) return idTable.ContainsKey(id);

        return false;
    }
}

public class CSVHeader
{
    public int keyColumn = -1;

    public Dictionary<Language, int> languageColumns = new();
}