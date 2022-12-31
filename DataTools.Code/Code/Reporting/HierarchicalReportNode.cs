using DataTools.Code.Project;

using System.Collections.Generic;

namespace DataTools.Code.Reporting
{
    internal class HierarchicalReportNode<T> : ReportNode<T> where T : IProjectElement
    {
        private IList<ReportNode<T>> children = new List<ReportNode<T>>();

        public virtual IList<ReportNode<T>> Children
        {
            get => children;
            set
            {
                SetProperty(ref children, value);
            }
        }
    }
}