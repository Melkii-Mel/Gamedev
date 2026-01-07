using Gamedev.InputSystem.Api.Devices;

namespace Gamedev.InputSystem.Api;

public interface IInput
{
    IKeyboard Keyboard { get; }
    IMouse Mouse { get; }
    IController Controller { get; }
}
