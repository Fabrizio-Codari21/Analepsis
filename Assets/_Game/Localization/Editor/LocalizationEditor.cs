using System.Collections.Generic;
using UnityEngine;

public static class LocalizationCSVEditor
{
    public static bool AddKey(
        CSVKey csvKey,
        string id,
        Dictionary<string, string> values = null)
    {
        if (!ValidateInput(csvKey, id))
            return false;

        var rows =
            CSVUtility.ReadCSV(csvKey.table.text);

        if (rows.Count == 0)
        {
            Debug.LogError(
                $"[Localization] CSV '{csvKey.name}' is empty.");

            return false;
        }

        var keyColumn =
            CSVHeaderParser.FindKeyColumn(rows[0]);

        if (keyColumn < 0)
        {
            Debug.LogError(
                $"[Localization] CSV '{csvKey.name}' " +
                "does not contain a Key column.");

            return false;
        }

        if (ContainsKey(
                rows,
                keyColumn,
                id))
        {
            Debug.LogWarning(
                $"[Localization] Key '{id}' already exists " +
                $"in '{csvKey.name}'.");

            return false;
        }

        AddRow(
            rows,
            keyColumn,
            id,
            values);

        var columnCount =
            CSVUtility.GetEffectiveColumnCount(rows[0]);

        var cleanRows =
            CSVUtility.TrimColumns(
                rows,
                columnCount);

        var csv =
            CSVUtility.WriteCSV(cleanRows);

        return CSVWriter.Write(
            csvKey,
            csv);
    }

    private static bool ValidateInput(
        CSVKey csvKey,
        string id)
    {
        if (csvKey == null)
        {
            Debug.LogError(
                "[Localization] Cannot add Key. CSVKey is null.");

            return false;
        }

        if (csvKey.table == null)
        {
            Debug.LogError(
                $"[Localization] CSVKey '{csvKey.name}' " +
                "has no TextAsset assigned.");

            return false;
        }

        if (string.IsNullOrWhiteSpace(id))
        {
            Debug.LogError(
                "[Localization] Cannot add an empty Key.");

            return false;
        }

        return true;
    }

    private static bool ContainsKey(
        List<string[]> rows,
        int keyColumn,
        string id)
    {
        for (var i = 1; i < rows.Count; i++)
        {
            var row = rows[i];

            if (keyColumn >= row.Length)
                continue;

            if (row[keyColumn].Trim() == id)
                return true;
        }

        return false;
    }

    private static void AddRow(
        List<string[]> rows,
        int keyColumn,
        string id,
        Dictionary<string, string> values)
    {
        var header = rows[0];

        var columnCount =
            CSVUtility.GetEffectiveColumnCount(header);

        if (columnCount <= 0)
            return;

        var row =
            new string[columnCount];

        // 默认所有 Cell 都为空
        for (var i = 0; i < row.Length; i++)
        {
            row[i] = string.Empty;
        }

        // 写入 Key
        row[keyColumn] = id;

        // 没有其他数据就直接结束
        if (values == null)
        {
            rows.Add(row);
            return;
        }

        // 根据 Header 名称，把输入的数据写到正确 Column
        for (var column = 0;
             column < columnCount;
             column++)
        {
            // Key 已经单独处理，不覆盖
            if (column == keyColumn)
                continue;

            var headerName =
                header[column].Trim();

            if (string.IsNullOrWhiteSpace(headerName))
                continue;

            if (!values.TryGetValue(
                    headerName,
                    out var value))
            {
                continue;
            }

            row[column] =
                value ?? string.Empty;
        }

        rows.Add(row);
    }
    
    public static bool UpdateKey(
        CSVKey csvKey,
        string id,
        Dictionary<string, string> values)
    {
        if (!ValidateInput(csvKey, id))
            return false;

        var rows =
            CSVUtility.ReadCSV(csvKey.table.text);

        if (rows.Count == 0)
        {
            Debug.LogError(
                $"[Localization] CSV '{csvKey.name}' is empty.");

            return false;
        }

        var keyColumn =
            CSVHeaderParser.FindKeyColumn(rows[0]);

        if (keyColumn < 0)
        {
            Debug.LogError(
                $"[Localization] CSV '{csvKey.name}' " +
                "does not contain a Key column.");

            return false;
        }

        var rowIndex =
            FindKeyRow(
                rows,
                keyColumn,
                id);

        if (rowIndex < 0)
        {
            Debug.LogError(
                $"[Localization] Key '{id}' does not exist " +
                $"in '{csvKey.name}'.");

            return false;
        }

        UpdateRow(
            rows,
            rowIndex,
            keyColumn,
            values);

        var columnCount =
            CSVUtility.GetEffectiveColumnCount(rows[0]);

        var cleanRows =
            CSVUtility.TrimColumns(
                rows,
                columnCount);

        var csv =
            CSVUtility.WriteCSV(cleanRows);

        return CSVWriter.Write(
            csvKey,
            csv);
    }
    
    
    
    private static int FindKeyRow(
        List<string[]> rows,
        int keyColumn,
        string id)
    {
        for (var i = 1; i < rows.Count; i++)
        {
            var row = rows[i];

            if (keyColumn >= row.Length)
                continue;

            if (row[keyColumn].Trim() == id)
                return i;
        }

        return -1;
    }
    
    private static void UpdateRow(
        List<string[]> rows,
        int rowIndex,
        int keyColumn,
        Dictionary<string, string> values)
    {
        if (values == null)
            return;

        var header = rows[0];

        var columnCount =
            CSVUtility.GetEffectiveColumnCount(header);

        var oldRow =
            rows[rowIndex];

        var newRow =
            new string[columnCount];

        // 先保留旧数据。
        for (var column = 0;
             column < columnCount;
             column++)
        {
            newRow[column] =
                column < oldRow.Length
                    ? oldRow[column]
                    : string.Empty;
        }

        // Key 本身不允许在这里修改。
        newRow[keyColumn] =
            oldRow[keyColumn];

        // 根据 Header 更新对应的值。
        for (var column = 0;
             column < columnCount;
             column++)
        {
            if (column == keyColumn)
                continue;

            var headerName =
                header[column].Trim();

            if (string.IsNullOrWhiteSpace(headerName))
                continue;

            if (!values.TryGetValue(
                    headerName,
                    out var value))
            {
                continue;
            }

            newRow[column] =
                value ?? string.Empty;
        }

        rows[rowIndex] = newRow;
    }
}