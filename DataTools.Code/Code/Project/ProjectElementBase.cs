using DataTools.Essentials.Observable;

using System;

namespace DataTools.Code.Project
{
    /// <summary>
    /// Base class element for project elements
    /// </summary>
    internal abstract class ProjectElementBase : ObservableBase, IProjectElement, IDisposable
    {
        protected string title;
        private WeakReference<ISolutionElement> parent;
        private bool disposedValue;

        /// <summary>
        /// True if the object has been disposed
        /// </summary>
        protected bool IsDisposed => disposedValue;

        /// <summary>
        /// Creates a new project element
        /// </summary>
        /// <param name="parentElement">Optional parent</param>
        public ProjectElementBase(ISolutionElement parentElement = null)
        {
            if (parentElement != null)
            {
                ParentElement = parentElement;
            }
        }

        /// <summary>
        /// Gets or sets the parent element for this project node.
        /// </summary>
        public ISolutionElement ParentElement
        {
            get
            {
                if (disposedValue) return null;
                ISolutionElement node = null;
                parent?.TryGetTarget(out node);
                return node;
            }
            protected set
            {
                if (disposedValue) throw new ObjectDisposedException(GetType().FullName);
                ISolutionElement node = null;

                parent?.TryGetTarget(out node);

                if (node != value)
                {
                    if (value == null)
                    {
                        parent = null;
                    }
                    else if (parent == null)
                    {
                        parent = new WeakReference<ISolutionElement>(value);
                    }
                    else
                    {
                        parent.SetTarget(value);
                    }
                }
            }
        }

        public abstract ElementType ElementType { get; }

        public virtual string Title
        {
            get => title;
            protected set
            {
                SetProperty(ref title, value);
            }
        }

        #region IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                parent = null;

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~ProjectElementBase()
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
        #endregion IDisposable
    }
}