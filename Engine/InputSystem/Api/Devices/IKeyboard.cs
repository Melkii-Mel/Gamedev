namespace Gamedev.InputSystem.Api.Devices;

public interface IKeyboard
{
    delegate void KeyboardAction(string key, bool isKeyDown);
    event KeyboardAction? Action;

    bool IsKeyDown(string key);
}
