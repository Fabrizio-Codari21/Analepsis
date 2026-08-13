using System.Collections.Generic;
using System.Text;

public static class CSVUtility
{
    /// <summary>
    /// 读取 CSV 字符串，并转换成 Row -> Cell。
    ///
    /// 支持：
    /// - 自定义分隔符，默认 ;
    /// - 引号包裹内容
    /// - Cell 内包含分隔符
    /// - Cell 内包含换行
    /// - 双引号转义："" -> "
    /// - 忽略完全为空的 Row
    ///
    /// 注意：
    /// 不会删除空 Cell。
    /// 因为空 Cell 在 Localization 中可能代表：
    /// “这个语言存在，但是还没有翻译。”
    /// </summary>
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

            // CSV 中 "" 代表真正的一个 "
            if (IsEscapedQuote(csv, i, quoted))
            {
                cell.Append('"');
                i++;
                continue;
            }

            // 进入 / 离开 "..." 区域
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

                // Windows 换行是 \r\n
                if (IsWindowsLineBreak(csv, i))
                    i++;

                continue;
            }

            // 普通字符
            cell.Append(current);
        }

        // 最后一行可能没有换行
        AddCell(row, cell);
        AddRow(rows, row);

        return rows;
    }

    /// <summary>
    /// 是否为双引号。
    /// </summary>
    private static bool IsQuote(char value)
    {
        return value == '"';
    }

    /// <summary>
    /// 是否为真正的 Cell 分隔符。
    /// 引号内部的 separator 只是普通文字。
    /// </summary>
    private static bool IsSeparator(char value, char separator, bool quoted)
    {
        return value == separator && !quoted;
    }

    /// <summary>
    /// 是否为真正的 Row 换行。
    /// 引号内部的换行属于 Cell 内容。
    /// </summary>
    private static bool IsLineBreak(
        char value,
        bool quoted)
    {
        return !quoted && (value == '\n' || value == '\r');
    }

    /// <summary>
    /// 判断当前位置是不是 CSV 转义双引号：
    ///
    /// ""
    ///
    /// 代表：
    ///
    /// "
    /// </summary>
    private static bool IsEscapedQuote(string csv, int index, bool quoted)
    {
        return quoted && csv[index] == '"'
                      && index + 1 < csv.Length
                      && csv[index + 1] == '"';
    }

    /// <summary>
    /// 判断是否为 Windows 换行：
    ///
    /// \r\n
    /// </summary>
    private static bool IsWindowsLineBreak(string csv, int index)
    {
        return csv[index] == '\r'
               && index + 1 < csv.Length
               && csv[index + 1] == '\n';
    }

    /// <summary>
    /// 完成一个 Cell。
    /// </summary>
    private static void AddCell(List<string> row, StringBuilder cell)
    {
        row.Add(cell.ToString());
        cell.Clear();
    }

    /// <summary>
    /// 完成一个 Row。
    ///
    /// 完全空的 Row 会忽略，
    /// 但 Row 中的空 Cell 会保留。
    /// </summary>
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

    /// <summary>
    /// 判断整行是否完全为空。
    ///
    /// 例如：
    ///
    /// ;;;;;;;;
    ///
    /// 会被认为是空 Row。
    /// </summary>
    private static bool IsEmptyRow(List<string> row)
    {
        return row.TrueForAll(string.IsNullOrWhiteSpace);
    }
}