using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DataTools.CSTools;
using DataTools.Observable;

namespace CSRefactorCurio.Reporting
{
    public interface IReportNode<T> : INotifyPropertyChanged where T : IMarker
    {
        T Element { get; }

        IMarkerList<T> AssociatedList { get; }

    }

    public interface IReport<T> : INotifyPropertyChanged where T : IMarker
    {
        int ReportId { get; }

        string ReportName { get; }

        string AssociatedReason { get; }

        IList<IReportNode<T>> Reports { get; }

        void CompileReport(IList<IProjectElement> context);

        void Sort();
    }

    public class ReportNodeBase<T> : ObservableBase, IReportNode<T> where T : IMarker
    {
        protected T element;
        protected IMarkerList<T> associatedList;    
        
        public virtual T Element 
        {
            get => element;
            protected internal set
            {
                SetProperty(ref element, value);
            }
        }

        public virtual IMarkerList<T> AssociatedList
        {
            get => associatedList;
            set
            {
                SetProperty(ref associatedList, value);
            }
        }

        public ReportNodeBase()
        {
            AssociatedList = new ObservableMarkerList<T>();
        }
    }

    public abstract class ReportBase<T> : ObservableBase, IReport<T> where T : IMarker
    {
        
        public abstract string ReportName { get;  }

        public abstract string AssociatedReason { get; }

        public abstract IList<IReportNode<T>> Reports { get; }

        public abstract int ReportId { get; }

        public abstract void CompileReport(IList<IProjectElement> context);

        public abstract void Sort();
    }

    public class CSReportNodeBase : ReportNodeBase<CSMarker>
    {
    }

    public abstract class CSReportBase : ReportBase<CSMarker>
    {
    }
}
