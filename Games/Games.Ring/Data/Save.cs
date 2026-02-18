#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Games.Ring.Data;

public record Save(List<Record> Records)
{
    // TODO: Make it a readonly view
    [JsonIgnore] public Dictionary<string, List<Record>>? LevelRecords;

    public Save() : this([])
    {
    }

    public Dictionary<string, List<Record>> Index()
    {
        return LevelRecords = Records.GroupBy(l => l.LevelId)
            .ToDictionary(g => g.Key, g => g.OrderBy(r => r.DateTime).ToList());
    }

    public void AddRecord(Record record)
    {
        Records.Add(record);
        LevelRecords ??= Index();
        if (!LevelRecords.TryGetValue(record.LevelId, out var records))
        {
            records = [];
            LevelRecords.Add(record.LevelId, records);
        }

        LevelRecords[record.LevelId].Add(record);
    }
}

public record Record(string LevelId, DateTime DateTime, Score Score);
