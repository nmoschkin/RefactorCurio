using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Threading;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace CSRefectorCurio
{
    public class CurioExplorerToolWindow : BaseToolWindow<CurioExplorerToolWindow>
    {
        public override string GetTitle(int toolWindowId) => "CS Refactor Curio";

        public override Type PaneType => typeof(Pane);

        public override Task<FrameworkElement> CreateAsync(int toolWindowId, CancellationToken cancellationToken)
        {           
            return Task.FromResult<FrameworkElement>(new CurioExplorer());
        }

        [Guid("f1e4c1ba-fb8d-45bd-a3df-7acd4e13754a")]
        internal class Pane : ToolWindowPane
        {            
            public Pane()
            {
                BitmapImageMoniker = KnownMonikers.Namespace;      
            }
        }
    }
}