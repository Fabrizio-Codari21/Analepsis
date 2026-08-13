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

        if (!LcaValidator.ValidateLanguageList(languageList)) return;

        if (!LcaValidator.ValidateCSVKey(csvKeys)) return;

        LoadCsvToDictionary();

        currentLanguage = languageList.defaultLanguage;
    }


    public void ModifyLanguage(int dir = 1)
    {
        var count = languageList.languages.Length;

        _currentLanguageIndex = (_currentLanguageIndex + dir % count + count) % count;

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

    private void LoadCsvToDictionary()
    {
        tables.Clear();

        foreach (var csvKey in csvKeys)
        {
            if (csvKey == null || csvKey.table == null) continue;

            var rows = CSVUtility.ReadCSV(csvKey.table.text);

            if (rows.Count == 0) continue;

            if (!LcaValidator.ValidateHeaderDuplicates(rows[0], csvKey.name)) continue;

            var header = ReadHeader(rows[0]);

            if (!LcaValidator.ValidateHeader(header.keyColumn, header.languageColumns.Count, csvKey.name)) continue;

            if (!LcaValidator.ValidateFallbackLanguage(header.languageColumns, languageList.fallbackLanguage, csvKey.name)) continue;
            

            if (!LcaValidator.ValidateDuplicateKey(rows, header.keyColumn, csvKey.name)) continue;


            tables[csvKey] = BuildTable(rows, header, csvKey.name);

            DebugTable(csvKey);
        }
    }


    private Dictionary<Language, Dictionary<string, string>> BuildTable(List<string[]> rows, CSVHeader header,
        string sourceName)
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

    private void AddLocalizationRow(Dictionary<Language, Dictionary<string, string>> table, string[] row,
        CSVHeader header, string sourceName, int rowNumber)
    {
        if (header.keyColumn >= row.Length) return;

        var key = row[header.keyColumn].Trim();

        if (!LcaValidator.ValidateKey(key, sourceName, rowNumber))
        {
            return;
        }

        foreach (var pair in header.languageColumns)
        {
            var language = pair.Key;
            var column = pair.Value;

            var text = GetCell(row, column);

            table[language].Add(key, text);
        }
    }


    private static string GetCell(string[] row, int column)
    {
        return column < row.Length ? row[column] : string.Empty;
    }


    private void DebugTable(CSVKey csvKey)
    {
        if (!tables.TryGetValue(csvKey, out var languageTable))
            return;

        var log = new StringBuilder();

        log.AppendLine($"===== {csvKey.name} =====");

        foreach (var language in languageTable)
        {
            log.AppendLine($"Language [{language.Key}]");

            foreach (var entry in language.Value)
            {
                log.AppendLine(
                    $"    [{entry.Key}] = [{entry.Value}]");
            }
        }

        Debug.Log(log.ToString());
    }

    public string TranslateText(CSVKey key, string id)
    {
        if (TryGetValidTranslation(key, currentLanguage, id, out var result)) return result;
        

        if (TryGetValidTranslation(key, languageList.fallbackLanguage, id, out var fallback))
        {
            Debug.LogWarning($"[Localization] Using fallback translation for '{id}'.");
            return fallback;
        }

        var tableName = GetTableName(key);

        Debug.LogWarning($"[Localization] Translation not found for '{id}' " + $"in table '{tableName}'.");

        return $"[Localization Error] {tableName} : {id}";
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


    private CSVHeader ReadHeader(string[] headerRow)
    {
        var header = new CSVHeader();

        for (int i = 0; i < headerRow.Length; i++)
        {
            var columName = headerRow[i].Trim();
            if (string.IsNullOrWhiteSpace(columName)) continue;

            if (columName.Equals("Key", StringComparison.OrdinalIgnoreCase))
            {
                header.keyColumn = i;
                continue;
            }


            if (!languageList.TryGetLanguage(columName, out var language))
            {
                Debug.LogWarning($"[Localization] Unknown language column: {columName}");
                continue;
            }

            header.languageColumns[language] = i;
        }

        return header;
    }
    
    public bool ContainsKey(CSVKey csvKey, string id)
    {
        if (csvKey == null || string.IsNullOrWhiteSpace(id)) return false;

        if (!tables.TryGetValue(csvKey, out var languageTable)) return false;

        if (!languageTable.TryGetValue(languageList.fallbackLanguage, out var idTable))
        {
            return false;
        }

        return idTable.ContainsKey(id);
    }
}

public class CSVHeader
{
    public int keyColumn = -1;

    public Dictionary<Language, int> languageColumns = new();
}