using System.Collections.Generic;

namespace KekeDataStore.Json
{
    // The Memento interface provides a way to retrieve the memento's metadata,
    // such as creation date or name. However, it doesn't expose the
    // Originator's state.
    public interface IMemento<T> where T : IBaseEntity
    {
        Dictionary<string, T> Data { get; }
    }
}
