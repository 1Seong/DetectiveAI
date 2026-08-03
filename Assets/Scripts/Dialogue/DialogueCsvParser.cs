using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[Serializable]
public class DialogueData
{
    public string ID;
    public string Speaker;
    public string Dialogue;
    public string Background;
    public string LeftCharacter;
    public string RightCharacter;
    public string NextID;
}

public static class DialogueCsvParser
{
    private static readonly string[] RequiredHeaders =
    {
        "ID",
        "Speaker",
        "Dialogue",
        "Background",
        "LeftCharacter",
        "RightCharacter",
        "NextID"
    };

    public static List<DialogueData> Parse(TextAsset csvFile)
    {
        if (csvFile == null)
        {
            Debug.LogError("CSV 파일이 지정되지 않았습니다.");
            return new List<DialogueData>();
        }

        return Parse(csvFile.text);
    }

    public static List<DialogueData> Parse(string csvText)
    {
        var result = new List<DialogueData>();

        if (string.IsNullOrWhiteSpace(csvText))
        {
            Debug.LogWarning("CSV 내용이 비어 있습니다.");
            return result;
        }

        List<List<string>> rows = ParseRows(csvText);

        if (rows.Count == 0)
            return result;

        Dictionary<string, int> headerMap = CreateHeaderMap(rows[0]);

        if (!ValidateHeaders(headerMap))
            return result;

        for (int rowIndex = 1; rowIndex < rows.Count; rowIndex++)
        {
            List<string> row = rows[rowIndex];

            if (IsEmptyRow(row))
                continue;

            DialogueData data = new DialogueData
            {
                ID = GetValue(row, headerMap, "ID"),
                Speaker = GetValue(row, headerMap, "Speaker"),
                Dialogue = GetValue(row, headerMap, "Dialogue"),
                Background = GetValue(row, headerMap, "Background"),
                LeftCharacter = GetValue(row, headerMap, "LeftCharacter"),
                RightCharacter = GetValue(row, headerMap, "RightCharacter"),
                NextID = GetValue(row, headerMap, "NextID")
            };

            if (string.IsNullOrWhiteSpace(data.ID))
            {
                Debug.LogWarning(
                    $"CSV {rowIndex + 1}번째 행의 ID가 비어 있어 건너뜁니다."
                );
                continue;
            }

            result.Add(data);
        }

        return result;
    }

    private static List<List<string>> ParseRows(string csvText)
    {
        var rows = new List<List<string>>();
        var currentRow = new List<string>();
        var currentField = new StringBuilder();

        bool insideQuotes = false;

        for (int i = 0; i < csvText.Length; i++)
        {
            char current = csvText[i];

            if (current == '"')
            {
                // ""는 CSV 내부에서 큰따옴표 하나를 의미합니다.
                if (insideQuotes &&
                    i + 1 < csvText.Length &&
                    csvText[i + 1] == '"')
                {
                    currentField.Append('"');
                    i++;
                }
                else
                {
                    insideQuotes = !insideQuotes;
                }
            }
            else if (current == ',' && !insideQuotes)
            {
                currentRow.Add(currentField.ToString());
                currentField.Clear();
            }
            else if ((current == '\n' || current == '\r') && !insideQuotes)
            {
                // Windows 줄바꿈 \r\n을 하나의 줄바꿈으로 처리합니다.
                if (current == '\r' &&
                    i + 1 < csvText.Length &&
                    csvText[i + 1] == '\n')
                {
                    i++;
                }

                currentRow.Add(currentField.ToString());
                currentField.Clear();

                rows.Add(currentRow);
                currentRow = new List<string>();
            }
            else
            {
                currentField.Append(current);
            }
        }

        // 마지막 행에 줄바꿈이 없는 경우 처리
        if (currentField.Length > 0 || currentRow.Count > 0)
        {
            currentRow.Add(currentField.ToString());
            rows.Add(currentRow);
        }

        return rows;
    }

    private static Dictionary<string, int> CreateHeaderMap(List<string> headerRow)
    {
        var headerMap = new Dictionary<string, int>(
            StringComparer.OrdinalIgnoreCase
        );

        for (int i = 0; i < headerRow.Count; i++)
        {
            string header = headerRow[i].Trim();

            // UTF-8 BOM 제거
            if (i == 0)
                header = header.TrimStart('\uFEFF');

            if (!string.IsNullOrEmpty(header))
                headerMap[header] = i;
        }

        return headerMap;
    }

    private static bool ValidateHeaders(Dictionary<string, int> headerMap)
    {
        bool isValid = true;

        foreach (string header in RequiredHeaders)
        {
            if (headerMap.ContainsKey(header))
                continue;

            Debug.LogError($"CSV에 필수 칼럼 '{header}'가 없습니다.");
            isValid = false;
        }

        return isValid;
    }

    private static string GetValue(
        List<string> row,
        Dictionary<string, int> headerMap,
        string header)
    {
        if (!headerMap.TryGetValue(header, out int columnIndex))
            return string.Empty;

        if (columnIndex < 0 || columnIndex >= row.Count)
            return string.Empty;

        return row[columnIndex].Trim();
    }

    private static bool IsEmptyRow(List<string> row)
    {
        foreach (string value in row)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return false;
        }

        return true;
    }
}
