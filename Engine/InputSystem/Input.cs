using System.Collections.Generic;
using Gamedev.InputSystem.ActionSystem;
using Gamedev.InputSystem.Api;

namespace Gamedev.InputSystem;

public class Input(IInput devices)
{
    public IInput Devices { get; } = devices;

    public InputActionDispatcher Dispatcher { get; } = new(devices);
    public void RegisterActions(IEnumerable<InputAction> inputActions)
    {
        Dispatcher.RegisterActions(inputActions);
    }
}
