using System;

public static class CSVHeaderParser
{
    
    public static int FindKeyColumn(string[] headerRow)
    {
        for (var i = 0; i < headerRow.Length; i++)
        {
            var columnName = headerRow[i].Trim();

            if (columnName.Equals(
                    "Key",
                    StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }
    public static CSVHeader Parse(
        string[] headerRow,
        LanguageList languageList)
    {
        var header = new CSVHeader();

        for (var i = 0; i < headerRow.Length; i++)
        {
            var columnName = headerRow[i].Trim();

            if (string.IsNullOrWhiteSpace(columnName)) continue;

            if (columnName.Equals("Key", StringComparison.OrdinalIgnoreCase))
            {
                header.keyColumn = i;
                continue;
            }

            if (languageList.TryGetLanguage(columnName, out var language))
            {
                header.languageColumns.Add(language, i);
            }

         
        }

        return header;
    }
}