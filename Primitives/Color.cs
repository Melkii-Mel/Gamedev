using System;
using System.Runtime.CompilerServices;

namespace Primitives;

public readonly record struct Color(float R, float G, float B, float A)
{
    #region parsing

    public static Color ParseRgba(string s) => ParseInternal(s, true);

    public static Color ParseRgb(string s) => ParseInternal(s, false);

    public static Color ParseSmart(string s) => ParseInternal(s, s.Length > 7);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Color ParseInternal(string s, bool isA)
    {
        var i = s[0] == '#' ? 1 : 0;

        if (s.Length - i != (isA ? 8 : 6))
        {
            throw new FormatException(isA ? "Expected RRGGBBAA" : "Expected RRGGBB");
        }


        var r = Channel(s, i, 0);
        var g = Channel(s, i, 1);
        var b = Channel(s, i, 2);
        var a = isA ? Channel(s, i, 3) : 255;

        const float inv = 1f / 255f;

        return new Color(
            r * inv,
            g * inv,
            b * inv,
            a * inv
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte Channel(string s, int i, int j)
    {
        return (byte)((Hex(s[i + j * 2]) << 4) | Hex(s[i + j * 2 + 1]));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Hex(char c)
    {
        return c switch
        {
            >= '0' and <= '9' => c - '0',
            >= 'A' and <= 'F' => c - 'A' + 10,
            >= 'a' and <= 'f' => c - 'a' + 10,
            _ => throw new FormatException($"Invalid hex character: {c}"),
        };
    }

    #endregion
}