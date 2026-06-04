#nullable enable
using System.Collections.Generic;
using Utils.Engine;

namespace Games.Ring;

public record Level(
    LevelHeading Heading,
    LevelRing Ring,
    LevelBody Body
) : MenuUtils.GenericMenus.ILevel;

public record LevelRing(List<Arc> Arcs);

public record LevelHeading(
    string Id,
    string Name,
    float Difficulty,
    float Length,
    string MusicId
);

public record LevelBody(
    Pattern[] Patterns
);

public record Pattern(float Time, Formation Formation);

public record Formation(
    float AngleStart,
    float AngleEnd,
    float SpeedStart,
    float SpeedEnd,
    float Delay,
    float Duration
);
