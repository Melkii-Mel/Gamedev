using System;

namespace Utils.Observables;

public class ObservableProperty<T>(T value)
{
    private T _value = value;

    public T Value
    {
        get => _value;
        set
        {
            var old = _value;
            _value = value;
            PropertyChanged?.Invoke(new PropertyChangedEventArgs<T>(old, value));
        }
    }

    public event Action<PropertyChangedEventArgs<T>>? PropertyChanged;
}

public record PropertyChangedEventArgs<T>(T Old, T New);
