using DataTools.Code.Project;
using DataTools.Essentials.Observable;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace DataTools.Code.Reporting
{


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

        public abstract void Sort();


        protected abstract void CompileReport(IEnumerable context);

        void IReport.CompileReport(IEnumerable context)
        {
            CompileReport(context);
        }

        public ReportBase(ISolution solution)
        {
            Solution = solution;
        }

        public override string ToString()
        {
            return ReportName;
        }
    }


    internal abstract class ReportBase<TContext> : ReportBase, IReport<TContext>
         where TContext : class, ISolutionElement
    {
        public ReportBase(ISolution solution) : base(solution)
        {
        }

        public abstract void CompileReport(IEnumerable<TContext> context);

        protected override sealed void CompileReport(IEnumerable context)
        {
            
            CompileReport((IEnumerable<TContext>)context);
        }
    }
    
    internal abstract class ReportBase<TContext, TReport> : ReportBase<TContext>, IReport<TContext, TReport> 
        where TReport : IReportNode, new()
        where TContext : class, ISolutionElement
    {
        private new IList<TReport> reports;

        public override int Count => reports?.Count ?? 0;

        [Browsable(true)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public virtual new IList<TReport> Reports
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

        /// <summary>
        /// Compile, generate, and return a prepared report.
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        public ObservableCollection<IProjectElement> CompilePreparedReport(IEnumerable<TContext> context) 
        {
            CompileReport(context);
            return GeneratePreparedReport();
        }

        /// <summary>
        /// Gets a report prepared in for the view
        /// </summary>
        /// <returns></returns>
        public virtual ObservableCollection<IProjectElement> GeneratePreparedReport()
        {
            return new ObservableCollection<IProjectElement>(Reports.Select((x) => (IProjectElement)x));
        }

        public ReportBase(ISolution solution) : base(solution)
        {
        }
    }
}