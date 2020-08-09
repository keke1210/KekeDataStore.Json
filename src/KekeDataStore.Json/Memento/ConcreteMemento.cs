using System.Collections.Generic;
using System.Linq;

namespace KekeDataStore.Json
{
    public class ConcreteMemento<T> : IMemento<T> where T : IBaseEntity
    {
        public ConcreteMemento(Dictionary<string, T> data)
        {
            // Initialize the Data with a Cloned object to avoid type references
            Data = data.ToDictionary(entry => entry.Key, entry => entry.Value); 
        }

        public Dictionary<string, T> Data { get; }
    }
}
