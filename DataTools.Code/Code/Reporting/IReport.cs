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

        void Sort();

        void CompileReport(IEnumerable context);
    }

    public interface IReport<TContext> : IReport
         where TContext : class, ISolutionElement
    {
        void CompileReport(IEnumerable<TContext> context);
    }

    public interface IReport<TContext, TReport> : IReport<TContext>
         where TContext : class, ISolutionElement
         where TReport : IReportNode, new()
    {
        new IList<TReport> Reports { get; }
    }
}