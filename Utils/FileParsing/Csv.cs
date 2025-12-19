using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Utils.FileParsing;

public static class Csv
{
    public static Dictionary<string, Dictionary<string, string>> Load(string filePath, char delimiter = ',')
    {
        string[] lines;
        try
        {
            lines = File.ReadAllLines(filePath);
        }
        catch (Exception e)
        {
            throw new Exception($"Failed to read and parse a csv: {e.Message}");
        }

        if (lines.Length == 0) return new Dictionary<string, Dictionary<string, string>>();

        var headers = lines[0].Split(delimiter);
        var result = new Dictionary<string, Dictionary<string, string>>();

        foreach (var line in lines.Skip(1))
        {
            var values = line.Split(delimiter);
            if (values.Length == 0) continue;

            var rowKey = values[0];
            var rowDict = new Dictionary<string, string>();

            for (var i = 1; i < headers.Length && i < values.Length; i++) rowDict[headers[i]] = values[i];

            result[rowKey] = rowDict;
        }

        return result;
    }
}
