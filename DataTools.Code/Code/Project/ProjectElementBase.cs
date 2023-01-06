using DataTools.Essentials.Observable;

using System;

namespace DataTools.Code.Project
{
    /// <summary>
    /// Base class element for project elements
    /// </summary>
    internal abstract class ProjectElementBase : ObservableBase, IProjectElement
    {
        protected string title;
        private WeakReference<ISolutionElement> parent;

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
                ISolutionElement node = null;
                parent?.TryGetTarget(out node);
                return node;
            }
            protected set
            {
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
    }
}