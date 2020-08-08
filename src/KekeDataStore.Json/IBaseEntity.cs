using System;

namespace KekeDataStore.Json
{
    /// <summary>
    /// Base entity contract. All entities/models should implement this interface.
    /// </summary>
    public interface IBaseEntity
    {
        /// <summary>
        /// Key of the Entity
        /// </summary>
        Guid Id { get; set; }
    }
}
