using System;
using Utils.DataStructures;

namespace Utils;

public class WeakAction<TArg>
{
    private Tree<WeakReference<Action<TArg>>> _actions = [];

    public WeakAction() { }
    public WeakAction(Action<TArg> value)
    {
        _actions.Value = new WeakReference<Action<TArg>>(value);
    }

    public static WeakAction<TArg> operator +(WeakAction<TArg> a, WeakAction<TArg> b)
    {
        a._actions.Add(b._actions);
        return a;
    }

    public void Invoke(TArg arg)
    {
        foreach (var action in _actions)
        {
            if (action.TryGetTarget(out var target))
            {
                target?.Invoke(arg);
            }
        }
    }
}
