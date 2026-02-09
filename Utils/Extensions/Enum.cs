using System;
using System.Linq;

namespace Utils.Extensions;

public static class EnumExtensions
{
    /// <summary>
    /// In hot paths, prefer using IsSet(EnumValueA | EnumValueB | EnumValueC)
    /// </summary>
    /// <param name="value"></param>
    /// <param name="flags"></param>
    /// <typeparam name="TEnum"></typeparam>
    /// <returns></returns>
    public static bool AreSet<TEnum>(this TEnum value, params TEnum[] flags) where TEnum : struct, Enum
    {
        return flags.All(f => value.IsSet(f));
    }

    public static bool IsNotSet<TEnum>(this TEnum value, TEnum flag) where TEnum : struct, Enum
    {
        return !IsSet(value, flag);
    }

    public static bool IsSet<TEnum>(this TEnum value, TEnum flag)
        where TEnum : struct, Enum
    {
        var intValue = Convert.ToUInt64(value);
        var intFlag = Convert.ToUInt64(flag);
        return (intValue & intFlag) != 0;
    }

    public static TEnum Set<TEnum>(this TEnum value, TEnum flag)
        where TEnum : struct, Enum
    {
        var intValue = Convert.ToUInt64(value);
        var intFlag = Convert.ToUInt64(flag);
        return (TEnum)Enum.ToObject(typeof(TEnum), intValue | intFlag);
    }

    public static TEnum Clear<TEnum>(this TEnum value, TEnum flag)
        where TEnum : struct, Enum
    {
        var intValue = Convert.ToUInt64(value);
        var intFlag = Convert.ToUInt64(flag);
        return (TEnum)Enum.ToObject(typeof(TEnum), intValue & ~intFlag);
    }
}
