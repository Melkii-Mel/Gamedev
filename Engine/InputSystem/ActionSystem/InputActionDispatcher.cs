using System;
using System.Collections.Generic;
using Gamedev.InputSystem.Api;
using Utils;
using Utils.DataStructures;

namespace Gamedev.InputSystem.ActionSystem;

public class InputActionDispatcher
{
    private readonly MultiIndexStore<string, InputAction> _actionsStore = new();
    private readonly TypeMap _listenersMap = new();

    public InputActionDispatcher(IInput devices)
    {
        var mtmIndex = _actionsStore.AddManyToManyIndex(action => action.Bindings, binding => (binding.Key, binding.Device));

        BindBool();
        BindVec();
        BindFloat();
        BindVec3();
        return;

        void BindBool()
        {
            devices.Keyboard.Action += (key, isKeyDown) =>
            {
                var boolInputListeners = GetOrInitInputListeners<bool>();
                foreach (var action in mtmIndex[(key, Device.Keyboard)])
                    if (action.ValueType == InputValueType.Bool)
                        boolInputListeners.Invoke(new InputActionEventArgs<bool>(action.Name, isKeyDown));
            };
            // TODO
        }

        void BindVec()
        {
            // TODO
        }

        void BindFloat()
        {
            // TODO
        }

        void BindVec3()
        {
            // TODO
        }
    }

    public void AddListener<TValue>(Action<InputActionEventArgs<TValue>> listener)
    {
        var listeners = GetOrInitInputListeners<TValue>();
        listeners += new WeakAction<InputActionEventArgs<TValue>>(listener);
        _listenersMap.Set(listeners);
    }

    public void RegisterActions(IEnumerable<InputAction> inputActions)
    {
        foreach (var action in inputActions) _actionsStore.Add(action);
    }

    private WeakAction<InputActionEventArgs<TValue>> GetOrInitInputListeners<TValue>()
    {
        return _listenersMap.GetOrCreate<WeakAction<InputActionEventArgs<TValue>>>();
    }
}

public record InputActionEventArgs<TValue>(string Name, TValue Value);
