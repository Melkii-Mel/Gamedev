namespace Gamedev.InputSystem.Api.Devices;

public interface IController
{
    delegate void ControllerAction( /*TODO*/);

    event ControllerAction? Action;
    // TODO
}
