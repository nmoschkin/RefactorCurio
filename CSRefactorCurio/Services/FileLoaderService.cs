using DataTools.CSTools;
using DataTools.Essentials.Observable;
using System.Collections.Generic;
using System.Linq;

namespace CSRefactorCurio.Services
{
    internal class FileLoaderObservation : IDisposable
    {
        private WeakReference<FileLoaderService> _loaderService;
        private bool disposedValue;
        private IObserver<CSCodeFile> observer;

        public FileLoaderObservation(IObserver<CSCodeFile> observer, FileLoaderService loaderService)
        {
            _loaderService = new WeakReference<FileLoaderService>(loaderService);
            this.observer = observer;
        }

        public FileLoaderService Observed
        {
            get
            {
                if ((!disposedValue) && (_loaderService?.TryGetTarget(out var t) ?? false))
                {
                    return t;
                }

                return null;
            }
        }

        public IObserver<CSCodeFile> Observer => disposedValue ? null : observer;

        #region IDisposable

        ~FileLoaderObservation()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_loaderService?.TryGetTarget(out var d) ?? false)
                    {
                        d.RemoveSubscription(this);
                    }

                    _loaderService = null;
                }

                disposedValue = true;
            }
        }

        #endregion IDisposable
    }

    internal class FileLoaderService : ObservableBase, IObservable<CSCodeFile>, IDisposable
    {
        private readonly object lockObj = new object();
        private readonly List<FileLoaderObservation> observers = new List<FileLoaderObservation>();
        private bool disposedValue;
        private int count;
        private int pos;
        private bool waitNext;

        /// <summary>
        /// The main service that all components register with.
        /// </summary>
        public static readonly FileLoaderService Instance = new FileLoaderService();

        /// <summary>
        /// The WaitNext mode for this object instance (immutable)
        /// <br/><br/>
        /// This parameter is true if each subscriber is run synchronously and waited on, false if subscribers are spun off as tasks without waiting.
        /// </summary>
        public bool WaitNext => waitNext;

        /// <summary>
        /// Create a new <see cref="FileLoaderService"/>
        /// </summary>
        private FileLoaderService() : this(true)
        {
        }

        /// <summary>
        /// Create a new <see cref="FileLoaderService"/>
        /// </summary>
        /// <param name="waitNext">True to run each subscriber synchronously and wait, false to spin off tasks without waiting.</param>
        public FileLoaderService(bool waitNext)
        {
            this.waitNext = waitNext;
        }

        public virtual int Count
        {
            get => count;
            protected set
            {
                SetProperty(ref count, value);
            }
        }

        public virtual int Position
        {
            get => pos;
            protected set
            {
                SetProperty(ref pos, value);
            }
        }

        /// <summary>
        /// Load the requested files from disk
        /// </summary>
        /// <param name="files">The files to load</param>
        /// <param name="callback">Optional function to call when a file is finished loading</param>
        /// <param name="sleep">Optional sleep interval in milliseconds</param>
        /// <param name="conditional">True to only load files that are not already loaded</param>
        /// <param name="waitNext">True to run each subscriber synchronously and wait, false to spin off tasks without waiting.</param>
        /// <exception cref="ObjectDisposedException"></exception>
        public virtual void LoadFiles(IEnumerable<CSCodeFile> files, Action<CSCodeFile> callback = null, int sleep = 0, bool conditional = false)
        {
            if (disposedValue) throw new ObjectDisposedException(GetType().Name);

            var sln = CSRefactorCurioPackage.Instance.CurioSolution;
            
            FileLoaderObservation[] t;

            lock (lockObj)
            {
                t = observers.ToArray();
            }
            
            Count = files.Count();
            Position = 0;

            foreach (CSCodeFile file in files)
            {
                if (!conditional || !file.IsLazyLoad)
                {
                    file.Refresh(sleep);
                }

                if (waitNext)
                {
                    if (callback != null) callback(file);
                }
                else
                {
                    if (callback != null) _ = Task.Run(() => callback(file));
                }

                foreach (var obs in t)
                {
                    if (waitNext)
                    {
                        obs.Observer?.OnNext(file);
                    }
                    else
                    {
                        _ = Task.Run(() => obs.Observer?.OnNext(file));
                    }
                }

                Position++;
            }

            foreach (var obs in t)
            {
                if (waitNext)
                {
                    obs.Observer?.OnCompleted();
                }
                else
                {
                    _ = Task.Run(() => obs.Observer?.OnCompleted());
                }
                
            }
        }

        public IDisposable Subscribe(IObserver<CSCodeFile> observer)
        {
            if (disposedValue) throw new ObjectDisposedException(GetType().Name);

            foreach (var itero in observers)
            {
                if (itero.Observer?.Equals(observer) ?? false) return itero;
            }

            var obs = new FileLoaderObservation(observer, this);
            observers.Add(obs);

            return obs;
        }

        protected internal virtual void RemoveSubscription(FileLoaderObservation observation)
        {
            if (disposedValue) return;

            lock (lockObj)
            {
                observers.Remove(observation);
            }
        }

        #region IDisposable

        ~FileLoaderService()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                disposedValue = true;

                if (disposing)
                {
                    lock (lockObj)
                    {
                        observers.Clear();
                    }
                }
            }
        }

        #endregion IDisposable
    }
}