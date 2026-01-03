using System;

namespace Primitives;

public class Flags
{
    public event EventHandler<FlagChangedEventArgs>? FlagChanged;

    private ushort _raw;

    public void Set(byte index, bool state = true)
    {
        CheckIndex(index);
        if ((_raw & (1 << index)) != 0 == state) return;

        if (state)
        {
            _raw |= (ushort)(1 << index);
        }
        else
        {
            _raw &= (ushort)~(1 << index);
        }

        FlagChanged?.Invoke(this, new FlagChangedEventArgs(index, state));
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
        return Convert.ToString(_raw & 0xFFFF, 2).PadLeft(16, '0');
    }

    private static void CheckIndex(byte index)
    {
        if (index > 15)
            throw new ArgumentOutOfRangeException(nameof(index), "Index must be 0-15");
    }
}

public class FlagChangedEventArgs : EventArgs
{
    public byte Index { get; }
    public bool NewValue { get; }

    public FlagChangedEventArgs(byte index, bool newValue)
    {
        Index = index;
        NewValue = newValue;
    }
}
