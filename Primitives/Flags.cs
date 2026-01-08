using System;

namespace Primitives;

public class Flags(uint raw = 1)
{
    public event Action<FlagChangedEventArgs>? FlagChanged;

    private uint _raw = raw;

    public void Set(byte index, bool state = true)
    {
        CheckIndex(index);
        if ((_raw & (1 << index)) != 0 == state) return;

        if (state)
        {
            _raw |= (uint)(1 << index);
        }
        else
        {
            _raw &= (uint)~(1 << index);
        }

        FlagChanged?.Invoke(new FlagChangedEventArgs(index, state));
    }

    public void Clear(byte index)
    {
        Set(index, false);
    }

    public bool Get(byte index)
    {
        CheckIndex(index);
        return (_raw & (1 << index)) != 0;
    }

    public override string ToString()
    {
        return Convert.ToString(_raw & 0xFFFFFFFF, 2).PadLeft(32, '0');
    }

    private static void CheckIndex(byte index)
    {
        if (index > 31)
            throw new ArgumentOutOfRangeException(nameof(index), "Index must be 0-31");
    }
}

public record FlagChangedEventArgs(byte Index, bool NewValue);

