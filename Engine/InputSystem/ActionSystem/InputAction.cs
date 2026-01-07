using Utils.DataStructures;
using Utils.Observables;

namespace Gamedev.InputSystem.ActionSystem;

public record InputAction(
    string Name,
    InputValueType ValueType,
    ObservableCollection<InputActionBinding> Bindings
) : IHasId<string>
{
    public string Id => Name;
    // TODO: Create reusable and composable action defaults with nested blacklists and whitelists
    // TODO: Create structure for actions organization via nested categories which can be blacklisted and whitelisted
}

public record InputActionBinding(Device Device, string Key);
