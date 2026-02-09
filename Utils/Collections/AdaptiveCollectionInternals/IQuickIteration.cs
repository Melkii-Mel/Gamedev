using System.Collections.Generic;

namespace Utils.Collections.AdaptiveCollectionInternals;

public interface IQuickIteration<out T>
{
    IEnumerable<T> IterateQuick();
}
