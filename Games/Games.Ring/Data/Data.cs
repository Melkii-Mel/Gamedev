#nullable enable
using Primitives;

namespace Games.Ring.Data;

public record Data(
    string AppName,
    Rules Rules,
    string[] LevelIds,
    Sprites Sprites,
    UiConfig Ui,
    string LevelsDirectory,
    string MusicDirectory
);

public record UiConfig(
    ScoreScene ScoreScene,
    string FontFamilyHeading,
    string FontFamilyBody,
    float FontSizeHeading,
    float FontSizeBody,
    Color[] DifficultyColors
);

public record ScoreScene(
    int PerfectFontSize
);

public record Sprites(string Bullet);

public record Rules(
    int TopComboCount,
    float PerfectValue,
    float GoodValue,
    float MissedValue,
    float MaxScore,
    float RingDistance,
    float GoodTolerance
);
