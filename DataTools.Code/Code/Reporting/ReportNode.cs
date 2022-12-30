using DataTools.Code.Project;
using DataTools.Essentials.Observable;

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace DataTools.Code.Reporting
{
    internal class ReportNode : ObservableBase, IReportNode
    {
        protected object element;
        protected IList associatedList;
        protected string title;

        public ElementType ElementType => ElementType.ReportNode;

        [Browsable(true)]
        public virtual string Title
        {
            get => title;
            set
            {
                SetProperty(ref title, value);
            }
        }

        [Browsable(true)]
        public object Element
        {
            get => element;
            protected internal set
            {
                SetProperty(ref element, value);
            }
        }

        [Browsable(true)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public IList AssociatedList
        {
            get => associatedList;
            set
            {
                SetProperty(ref associatedList, value);
            }
        }
    }

    internal class ReportNode<T> : ReportNode, IReportNode<T> where T : IProjectElement
    {
        private new T element;
        private new IList<T> associatedList;

        public override string Title
        {
            get => element?.Title ?? base.Title;
            set
            {
                base.Title = value;
            }
        }

        [Browsable(true)]
        public virtual new T Element
        {
            get => element;
            protected internal set
            {
                if (SetProperty(ref element, value)) base.element = value;
            }
        }

        [Browsable(true)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public virtual new IList<T> AssociatedList
        {
            get => associatedList;
            set
            {
                if (SetProperty(ref associatedList, value)) base.associatedList = (IList)value;
            }
        }
    }
}