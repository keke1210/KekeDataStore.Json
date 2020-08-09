using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace KekeDataStore.Json
{
    /// <summary>
    /// Thread safe Json Data Store that saves data into binary files.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class JsonDataStore<T> : IDataStore<T> where T : IBaseEntity
    {
        #region Private Fields
        private readonly string _fileName;
        private readonly JsonFile<T> _file;

        private Lazy<Dictionary<string, T>> _data;

        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a file at the at specified directory with the specified filename
        /// </summary>
        /// <param name="filepath">Directory of the folder</param>
        /// <param name="dbName">The name of the binary file</param>
        public JsonDataStore(string directoryPath, string fileName)
        {
            _fileName = fileName;

            _file = new JsonFile<T>(directoryPath, $"{_fileName}_Json");

            _data = new Lazy<Dictionary<string, T>>(() => _file.LoadFromFile());
        }

        public JsonDataStore(string fileName)
        {
            _fileName = fileName;

            _file = new JsonFile<T>($"{_fileName}_Json");

            _data = new Lazy<Dictionary<string, T>>(() => _file.LoadFromFile());
        }

        public JsonDataStore()
        {
            _fileName = $"{typeof(T).Name}s";

            _file = new JsonFile<T>($"{_fileName}_Json");

            _data = new Lazy<Dictionary<string, T>>(() => _file.LoadFromFile());
        }
        #endregion

        #region Properties
        public int Count
        {
            get => ReadLocked(() => _data.Value.Count);
        }
        #endregion

        #region Public Methods
        public IQueryable<T> AsQueryable()
        {
            IQueryable<T> ReadFunc() => _data.Value.Select(x => x.Value).AsQueryable();

            var query = ReadLocked(ReadFunc);

            return query;
        }

        public IEnumerable<T> GetAll()
        {
            IEnumerable<T> ReadFunc() => _data.Value.Select(x => x.Value);

            var elements = ReadLocked(ReadFunc);

            return elements;
        }

        //public IEnumerable<T> GetAll() => ReadLocked(() => _data.Value.Select(x => x.Value));

        public IEnumerable<T> Get(Predicate<T> predicate)
        {
            IEnumerable<T> ReadFunc() => _data.Value.Where(t => predicate(t.Value)).Select(x => x.Value);

            var elements = ReadLocked(ReadFunc);

            return elements;
        }

        public T GetSingle(Predicate<T> predicate)
        {
            T ReadFunc() => _data.Value.Where(t => predicate(t.Value)).Select(x => x.Value).SingleOrDefault();

            var item = ReadLocked(ReadFunc);

            return item;
        }

        public T Create(T entity)
        {
            void ReadAction()
            {
                if (entity == null) throw new ArgumentNullException(nameof(entity));

                if (_data.Value.ContainsKey(entity.Id.ToString()))
                    throw new KekeDataStoreException($"Id: {entity.Id.ToString()}, already exists on the collection!");
            }


            T WriteFunc()
            {
                if (entity.Id.ToString().IsEmptyGuid())
                    entity.Id = Guid.NewGuid();

                // Sets entity id as key and object as value
                _data.Value.Add(entity.Id.ToString(), entity);

                return entity;
            }

            var createdItem = UpgradeableReadLocked<T>(ReadAction, WriteFunc);

            return createdItem;
        }

        public bool Delete(string id)
        {
            void ReadAction()
            {
                if (id.IsEmptyGuid()) throw new ArgumentNullException(nameof(id));

                T element;
                var elementExists = _data.Value.TryGetValue(id, out element);

                if (!elementExists) throw new KekeDataStoreException("Element doesn't exists on the collection!");
            }

            bool WriteFunc() => _data.Value.Remove(id);

            var deleted = UpgradeableReadLocked<bool>(ReadAction, WriteFunc);

            return deleted;
        }

        public T GetById(string id)
        {
            T ReadFunc()
            {
                if (id.IsEmptyGuid()) throw new ArgumentNullException(nameof(id));

                T element;
                _data.Value.TryGetValue(id, out element);

                return element;
            }

            var item = ReadLocked(ReadFunc);

            return item;
        }

        public T Update(string id, T entity)
        {
            void ReadAction()
            {
                if (entity == null) throw new ArgumentNullException(nameof(entity));

                T item;
                var userExists = _data.Value.TryGetValue(id, out item);

                if (!userExists)
                    throw new KekeDataStoreException($"Object of type '{typeof(T).Name}' with id: '{id}', doesn't exists on collection!");
            }

            T WriteFunc()
            {
                entity.Id = new Guid(id);
                _data.Value[id] = entity;
                return entity;
            }

            var updatedItem = UpgradeableReadLocked(ReadAction, WriteFunc);

            return updatedItem;
        }

        public bool Truncate()
        {
            bool WriteFunc()
            {
                _data.Value.Clear();
                return true;
            }

            var truncated = WriteLocked<bool>(WriteFunc);

            return truncated;
        }

        public bool SaveChanges()
        {
            bool WriteFunc()
            {
                _file.WriteToFile(_data.Value);
                return true;
            }

            var changesSaved = (bool?)WriteLocked(WriteFunc) ?? false;

            return changesSaved;
        }


        internal IMemento<T> Save()
        {
            return new ConcreteMemento<T>(_data.Value);
        }

        internal void Restore(IMemento<T> memento)
        {
            if (!(memento is ConcreteMemento<T>))
            {
                throw new Exception("Unknown memento class " + memento.ToString());
            }

            _data = new Lazy<Dictionary<string,T>> (() => memento.Data);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Wrapper for Enter/ExitReadLock
        /// </summary>
        /// <typeparam name="TResp">Type of object you want to return</typeparam>
        /// <param name="func">callback function which will be invoked inside ReaderLockSlim</param>
        /// <returns>Response object we read.</returns>
        private TResult ReadLocked<TResult>(Func<TResult> ReadFunc)
        {
            _lock.EnterReadLock();
            try
            {
                return ReadFunc.Invoke();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Wrapper for Enter/ExitUpgradeableReadLock. readAction is invoked first then the writeFunc.
        /// </summary>
        /// <typeparam name="TResp">Type of object you want to return after writting.</typeparam>
        /// <param name="readAction">void callback function for read</param>
        /// <param name="writeFunc">callback function that writes and returns value</param>
        /// <returns>Response object we write.</returns>
        private TResult UpgradeableReadLocked<TResult>(Action ReadAction, Func<TResult> WriteFunc)
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                ReadAction.Invoke();

                _lock.EnterWriteLock();
                try
                {
                    return WriteFunc.Invoke();
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>
        /// Wrapper for Enter/ExitWriteLock
        /// </summary>
        /// <typeparam name="TResp">Type of object you want to return after writting.</typeparam>
        /// <param name="func">callback function that writes and returns value</param>
        /// <returns>Response object we write.</returns>
        private TResult WriteLocked<TResult>(Func<TResult> WriteFunc)
        {
            _lock.EnterWriteLock();
            try
            {
                return WriteFunc.Invoke();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
            if (_lock != null) _lock.Dispose();
        }

        ~JsonDataStore()
        {
            if (_lock != null) _lock.Dispose();
        }
        #endregion
    }
}