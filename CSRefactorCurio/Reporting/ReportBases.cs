using System;
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
    public interface IReportNode<T> : INotifyPropertyChanged where T : INamespace
    {
        T Element { get; }

        IList<T> AssociatedList { get; }

    }

    public interface IReport<T> : INotifyPropertyChanged where T : INamespace
    {
        ISolution Solution { get; }

        int Count { get; }
        
        int ReportId { get; }

        string ReportName { get; }

        string AssociatedReason { get; }

        IList<IReportNode<T>> Reports { get; }

        void CompileReport(IList<T> context);

        void Sort();
    }

    public class ReportNodeBase<T> : ObservableBase, IReportNode<T> where T : INamespace
    {
        protected T element;
        protected IList<T> associatedList;

        [Browsable(true)]
        public virtual T Element 
        {
            get => element;
            protected internal set
            {
                SetProperty(ref element, value);
            }
        }

        [Browsable(true)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public virtual IList<T> AssociatedList
        {
            get => associatedList;
            set
            {
                SetProperty(ref associatedList, value);
            }
        }

        public ReportNodeBase()
        {
            AssociatedList = new ObservableCollection<T>();
        }

        public override string ToString()
        {
            return element?.FullyQualifiedName ?? base.ToString();
        }
    }

    public abstract class ReportBase<T> : ObservableBase, IReport<T> where T : INamespace
    {
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
        public abstract IList<IReportNode<T>> Reports { get; }

        [Browsable(true)]
        public abstract int ReportId { get; }

        public abstract void CompileReport(IList<T> context);

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

    public class CSReportNode : ReportNodeBase<INamespace>
    {
    }

    public abstract class CSReportBase : ReportBase<INamespace>
    {
        public CSReportBase(ISolution solution) : base(solution)
        {
        }

    }
}
