using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace SocketNetwork.Example.Utilities {
    /// <summary>
    /// A generic List that supports Multi-Threaded access, waiting on execution of any read/write operations until they can be processed. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ThreadSafeList<T> : IList<T>, IDisposable {

        /// <summary>
        /// A locking object that oversees the List read/write actions.
        /// </summary>
        private readonly ReaderWriterLockSlim _lock;

        private List<T> _list;

        public ThreadSafeList() {
            _list = new List<T>();
            _lock = new ReaderWriterLockSlim();
        }

        public int Count => criticalReadSection(() => _list.Count);

        public bool IsReadOnly => false;

        public T this[int index] {
            get => criticalReadSection(() => _list[index]);
            set => criticalWriteSection(() => _list[index] = value);
        }

        public void Add(T item) => criticalWriteSection(() => _list.Add(item));

        public int IndexOf(T item) => criticalReadSection(() => _list.IndexOf(item));

        public void Insert(int index, T item) => criticalWriteSection(() => _list.Insert(index, item));

        public void RemoveAt(int index) => criticalWriteSection(() => _list.RemoveAt(index));

        public void Clear() => criticalWriteSection(() => _list.Clear());

        public bool Contains(T item) => criticalReadSection(() => _list.Contains(item));

        public void CopyTo(T[] array, int arrayIndex) => criticalWriteSection(() => _list.CopyTo(array, arrayIndex));

        public bool Remove(T item) => criticalWriteSection(() => _list.Remove(item));

        public IEnumerator<T> GetEnumerator() {
            // blocks execution until a Read operation is allowed, and sets a lock on other execitions until released
            _lock.EnterReadLock();

            try {
                foreach (var value in _list) {
                    yield return value;
                }
            } finally {
                // release the lock, allowing other actions to proceed
                _lock.ExitReadLock();
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private void criticalReadSection(Action action) => criticalReadSection(delegate {
            action();
            return true;
        });

        private void criticalWriteSection(Action action) => criticalWriteSection(delegate {
            action();
            return true;
        });

        /// <summary>
        /// Handled read operations on <see cref="_list"/> in a thread-safe way
        /// </summary>
        private U criticalReadSection<U>(Func<U> action) {
            // blocks execution until a Read operation is allowed, and sets a lock on other execitions until released
            _lock.EnterReadLock();

            U result;
            try {
                result = action();
            } finally {
                // release the lock, allowing other actions to proceed
                _lock.ExitReadLock();
            }

            return result;
        }

        /// <summary>
        /// Handled write operations on <see cref="_list"/> in a thread-safe way
        /// </summary>
        private U criticalWriteSection<U>(Func<U> action) {
            // blocks execution until a Write operation is allowed, and sets a lock on other execitions until released
            _lock.EnterWriteLock();

            U result;
            try {
                result = action();
            } finally {
                // release the lock, allowing other actions to proceed
                _lock.ExitWriteLock();
            }

            return result;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        public override string ToString() => _list.ToString();

        private void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: dispose managed state (managed objects).

                    if (_lock.IsReadLockHeld || _lock.IsUpgradeableReadLockHeld || _lock.IsWriteLockHeld)
                        // the lock is still in use, so wait for next dispose cycle to be called from GC
                        return;

                    using (_lock) ;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _list.Clear();
                _list = null;

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ThreadSafeList() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        void IDisposable.Dispose() {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
