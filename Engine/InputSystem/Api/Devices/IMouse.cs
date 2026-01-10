namespace Gamedev.InputSystem.Api.Devices;

public interface IMouse
{
    delegate void MouseAction( /*TODO*/);

    event MouseAction? Action;
    // TODO
}
