using System;
using System.Collections.Generic;
using System.Linq;

namespace KekeDataStore.Json
{
    // The Caretaker doesn't depend on the Concrete Memento class. Therefore, it
    // doesn't have access to the originator's state, stored inside the memento.
    // It works with all mementos via the base Memento interface.
    public sealed class CareTaker<T> where T : IBaseEntity
    {
        private readonly Lazy<List<IMemento<T>>> _mementos = new Lazy<List<IMemento<T>>>();

        private readonly JsonDataStore<T> _originator;

        public CareTaker(JsonDataStore<T> originator)
        {
            _originator = originator;
        }

        public void Backup()
        {
            var memento = this._originator.Save();
            this._mementos.Value.Add(memento);
        }

        public void Undo()
        {
            if (this._mementos.Value.Count == 0)
            {
                return;
            }

            var memento = this._mementos.Value.Last();
            this._mementos.Value.Remove(memento);

            try
            {
                this._originator.Restore(memento);
            }
            catch (Exception)
            {
                this.Undo();
            }
        }

        public List<IMemento<T>> ShowHistory()
        {
            Console.WriteLine("Caretaker: Here's the list of mementos:");

            var i = 0;
            foreach (var memento in this._mementos.Value)
            {
                Console.WriteLine($"Backup  {++i} :");
                foreach (var item in memento.Data)
                {
                    Console.WriteLine(item.Value);
                }
            }
            Console.WriteLine();

            return this._mementos.Value;
        }
    }
}
