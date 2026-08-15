using System.Collections.Generic;




public static class LocalizationInspectorUtility
{
    public static List<string> GetKeys(CSVKey csvKey)
    {
        var keys = new List<string>();

        if (csvKey == null || csvKey.table == null)
            return keys;

        var rows =
            CSVUtility.ReadCSV(csvKey.table.text);

        if (rows.Count == 0)
            return keys;

        var keyColumn =
            CSVHeaderParser.FindKeyColumn(rows[0]);

        if (keyColumn < 0)
            return keys;

        for (var i = 1; i < rows.Count; i++)
        {
            var row = rows[i];

            if (keyColumn >= row.Length)
                continue;

            var key = row[keyColumn].Trim();

            if (string.IsNullOrWhiteSpace(key))
                continue;

            keys.Add(key);
        }

        return keys;
    }
    
    public static Dictionary<string, string> GetEntry(
        CSVKey csvKey,
        string id)
    {
        var result = new Dictionary<string, string>();

        if (csvKey == null ||
            csvKey.table == null ||
            string.IsNullOrWhiteSpace(id))
        {
            return result;
        }

        var rows =
            CSVUtility.ReadCSV(csvKey.table.text);

        if (rows.Count == 0)
            return result;

        var header = rows[0];

        var keyColumn =
            CSVHeaderParser.FindKeyColumn(header);

        if (keyColumn < 0)
            return result;

        for (var rowIndex = 1;
             rowIndex < rows.Count;
             rowIndex++)
        {
            var row = rows[rowIndex];

            if (keyColumn >= row.Length)
                continue;

            if (row[keyColumn].Trim() != id)
                continue;

            var columnCount =
                CSVUtility.GetEffectiveColumnCount(header);

            for (var column = 0;
                 column < columnCount;
                 column++)
            {
                var headerName =
                    header[column].Trim();

                if (string.IsNullOrWhiteSpace(headerName))
                    continue;

                var value =
                    column < row.Length
                        ? row[column]
                        : string.Empty;

                result[headerName] = value;
            }

            break;
        }

        return result;
    }
}