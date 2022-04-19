using CSRefactorCurio.Globalization.Resources;

using CSRefactorCurio.Reporting;
using DataTools.Observable;


using System.Collections.Generic;

namespace CSRefactorCurio.ViewModels
{


    internal class ReportViewModel : ObservableBase
    {
        CurioExplorerSolution solution;

        public string[] ReportTypes { get; }

        public CurioExplorerSolution Solution => solution;

        public ReportViewModel()
        {
            ReportTypes = new string[] { AppResources.REPORT_MOST_INTERDEPENDENT, AppResources.REPORT_MOST_REFERENCED_OBJECTS, AppResources.REPORT_MOST_SPREAD_OUT };
            this.solution = CSRefactorCurioPackage.Instance.CurioSolution;
        }

        public ReportViewModel(CurioExplorerSolution solution) : this()
        {
            this.solution = solution;
        }

    }
}
