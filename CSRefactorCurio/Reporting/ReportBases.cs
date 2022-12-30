using DataTools.CSTools;
using DataTools.Essentials.Observable;

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace CSRefactorCurio.Reporting
{
    internal interface IReportNode : INotifyPropertyChanged, IProjectElement
    {
        object Element { get; }

        IList AssociatedList { get; }
    }

    internal interface IReportNode<T> : IReportNode, INotifyPropertyChanged
    {
        new T Element { get; }

        new IList<T> AssociatedList { get; }
    }

    internal interface IReport : INotifyPropertyChanged
    {
        ISolution Solution { get; }

        int Count { get; }

        int ReportId { get; }

        string ReportName { get; }

        string AssociatedReason { get; }

        IList Reports { get; }

        void CompileReport<T>(IList<T> context) where T : INamespace;

        void Sort();
    }

    internal interface IReport<TRpt> : IReport
    {
        new IList<TRpt> Reports { get; }
    }

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

    internal abstract class ReportBase : ObservableBase, IReport
    {
        protected IList reports;

        [Browsable(true)]
        public ISolution Solution { get; }

        [Browsable(true)]
        public abstract int Count { get; }

        [Browsable(true)]
        public abstract string ReportName { get; }

        [Browsable(true)]
        public abstract string AssociatedReason { get; }

        [Browsable(true)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public IList Reports
        {
            get => reports;
            set
            {
                SetProperty(ref reports, value);
            }
        }

        [Browsable(true)]
        public abstract int ReportId { get; }

        public abstract void CompileReport<T>(IList<T> context) where T : INamespace;

        public abstract void Sort();

        public ReportBase(ISolution solution)
        {
            Solution = solution;
        }

        public override string ToString()
        {
            return this.ReportName;
        }
    }

    internal abstract class ReportBase<T> : ReportBase, IReport<T> where T : IReportNode, new()
    {
        private new IList<T> reports;

        public override int Count => reports?.Count ?? 0;

        [Browsable(true)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public virtual new IList<T> Reports
        {
            get => reports;
            set
            {
                if (SetProperty(ref reports, value))
                {
                    base.reports = (IList)value;
                    OnPropertyChanged(nameof(Count));
                }
            }
        }

        public ReportBase(ISolution solution) : base(solution)
        {
        }
    }
}