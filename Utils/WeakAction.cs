using System;
using System.Collections.Generic;
using Utils.DataStructures;

namespace Utils;

public class WeakAction<TArg>
{
    private readonly Tree<WeakReference<Action<TArg>>> _actions = [];
    private uint _isIterating;
    private readonly List<WeakAction<TArg>> _pending = [];

    public WeakAction()
    {
    }

    public WeakAction(Action<TArg> value)
    {
        _actions.Value = new WeakReference<Action<TArg>>(value);
    }

    public static WeakAction<TArg> operator +(WeakAction<TArg> a, WeakAction<TArg> b)
    {
        if (a._isIterating > 0) a._pending.Add(b);
        else a._actions.Add(b._actions);
        return a;
    }

    public void Invoke(TArg arg)
    {
        _isIterating++;
        foreach (var action in _actions)
            if (action.TryGetTarget(out var target))
                target?.Invoke(arg);
        _isIterating--;
        if (_isIterating == 0) FlushPended();
    }

    private void FlushPended()
    {
        foreach (var weakAction in _pending) _actions.Add(weakAction._actions);
        _pending.Clear();
    }
}
