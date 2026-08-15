using System.Collections.Generic;
using System.Text;

public static class CSVUtility
{

    public static List<string[]> ReadCSV(string csv, char separator = ';')
    {
        var rows = new List<string[]>();

        if (string.IsNullOrEmpty(csv))
            return rows;

        var row = new List<string>();
        var cell = new StringBuilder();

        var quoted = false;

        for (var i = 0; i < csv.Length; i++)
        {
            var current = csv[i];

            
            if (IsEscapedQuote(csv, i, quoted))
            {
                cell.Append('"');
                i++;
                continue;
            }

         
            if (IsQuote(current))
            {
                quoted = !quoted;
                continue;
            }

            // 只有引号外的 separator 才代表 Cell 结束
            if (IsSeparator(current, separator, quoted))
            {
                AddCell(row, cell);
                continue;
            }

            // 只有引号外的换行才代表 Row 结束
            if (IsLineBreak(current, quoted))
            {
                AddCell(row, cell);
                AddRow(rows, row);

              
                if (IsWindowsLineBreak(csv, i))
                    i++;

                continue;
            }

         
            cell.Append(current);
        }

        AddCell(row, cell);
        AddRow(rows, row);

        return rows;
    }

   
    private static bool IsQuote(char value)
    {
        return value == '"';
    }

    private static bool IsSeparator(char value, char separator, bool quoted)
    {
        return value == separator && !quoted;
    }

    private static bool IsLineBreak(
        char value,
        bool quoted)
    {
        return !quoted && (value == '\n' || value == '\r');
    }

    private static bool IsEscapedQuote(string csv, int index, bool quoted)
    {
        return quoted && csv[index] == '"'
                      && index + 1 < csv.Length
                      && csv[index + 1] == '"';
    }


    private static bool IsWindowsLineBreak(string csv, int index)
    {
        return csv[index] == '\r'
               && index + 1 < csv.Length
               && csv[index + 1] == '\n';
    }

    private static void AddCell(List<string> row, StringBuilder cell)
    {
        row.Add(cell.ToString());
        cell.Clear();
    }


    private static void AddRow(List<string[]> rows, List<string> row)
    {
        if (IsEmptyRow(row))
        {
            row.Clear();
            return;
        }

        rows.Add(row.ToArray());
        row.Clear();
    }


    private static bool IsEmptyRow(List<string> row)
    {
        return row.TrueForAll(string.IsNullOrWhiteSpace);
    }
    
    
    public static string WriteCSV(
        IEnumerable<string[]> rows,
        char separator = ';')
    {
        var csv = new StringBuilder();

        foreach (var row in rows)
        {
            for (var i = 0; i < row.Length; i++)
            {
                if (i > 0)
                    csv.Append(separator);

                csv.Append(EscapeCell(
                    row[i],
                    separator));
            }

            csv.AppendLine();
        }

        return csv.ToString();
    }
    
    
    private static string EscapeCell(string value, char separator)
    {
        value ??= string.Empty;

        var needsQuotes =
            value.Contains(separator) ||
            value.Contains('"') ||
            value.Contains('\n') ||
            value.Contains('\r');

        if (!needsQuotes)
            return value;

        var escaped = value.Replace(
            "\"",
            "\"\"");

        return $"\"{escaped}\"";
    }
    
    
    public static List<string[]> TrimColumns(
        IEnumerable<string[]> rows,
        int columnCount)
    {
        var result = new List<string[]>();

        if (rows == null || columnCount <= 0)
            return result;

        foreach (var row in rows)
        {
            var cleanRow = new string[columnCount];

            for (var i = 0; i < columnCount; i++)
            {
                cleanRow[i] =
                    row != null && i < row.Length
                        ? row[i]
                        : string.Empty;
            }

            result.Add(cleanRow);
        }

        return result;
    }
    
    public static int GetEffectiveColumnCount(string[] header)
    {
        if (header == null || header.Length == 0)
            return 0;

        for (var i = header.Length - 1; i >= 0; i--)
        {
            if (!string.IsNullOrWhiteSpace(header[i]))
                return i + 1;
        }

        return 0;
    }
}