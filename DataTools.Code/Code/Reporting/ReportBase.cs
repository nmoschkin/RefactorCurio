using DataTools.Code.Project;
using DataTools.Essentials.Observable;

using System;
using System.Collections;
using System.Collections.Generic;
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

        public abstract void CompileReport<T>(IList<T> context) where T : INamespace;

        public abstract void Sort();

        public ReportBase(ISolution solution)
        {
            Solution = solution;
        }

        public override string ToString()
        {
            return ReportName;
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