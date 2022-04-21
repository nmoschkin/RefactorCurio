using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DataTools.CSTools;
using DataTools.Observable;

namespace CSRefactorCurio.Reporting
{
    public interface IReportNode : INotifyPropertyChanged, IProjectElement
    {
        object Element { get; }

        IList AssociatedList { get; }
    }


    public interface IReportNode<T> : IReportNode, INotifyPropertyChanged 
    {
        new T Element { get; }

        new IList<T> AssociatedList { get; }

    }

    public interface IReport : INotifyPropertyChanged
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

    public interface IReport<TRpt> : IReport
    {
        new IList<TRpt> Reports { get; }
    }

    public class ReportNode : ObservableBase, IReportNode
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


    public class ReportNode<T> : ReportNode, IReportNode<T> where T : IProjectElement
    {
        new private T element;
        new private IList<T> associatedList;

        public override string Title
        {
            get => element?.Title ?? base.Title;
            set
            {
                base.Title = value;
            }
        }

        [Browsable(true)]
        new public virtual T Element
        {
            get => element;
            protected internal set
            {
                if (SetProperty(ref element, value)) base.element = value;
            }
        }

        [Browsable(true)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        new public virtual IList<T> AssociatedList
        {
            get => associatedList;
            set
            {
                if (SetProperty(ref associatedList, value)) base.associatedList = (IList)value;
            }
        }

    }

    public abstract class ReportBase : ObservableBase, IReport 
    {
        protected IList reports;

        [Browsable(true)]
        public ISolution Solution { get; }

        [Browsable(true)]
        public abstract int Count { get; }

        [Browsable(true)]
        public abstract string ReportName { get;  }

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

        public abstract void CompileReport<T>(IList<T> context) where T: INamespace;

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

    public abstract class ReportBase<T> : ReportBase, IReport<T> where T : IReportNode, new()
    {
        new private IList<T> reports;

        public override int Count => reports?.Count ?? 0;

        [Browsable(true)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        new public virtual IList<T> Reports 
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
