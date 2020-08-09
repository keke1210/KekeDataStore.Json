using System;
using System.Collections.Generic;
using System.Text;

namespace KekeDataStore.Json
{
    public interface IJsonDataStore<T> : IDataStore<T> where T : IBaseEntity
    {
        void Restore();
        void Commit();
    }
}
