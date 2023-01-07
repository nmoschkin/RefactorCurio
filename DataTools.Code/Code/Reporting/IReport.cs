using DataTools.Code.Project;

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace DataTools.Code.Reporting
{
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
}