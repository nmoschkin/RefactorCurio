using Microsoft.VisualStudio.Imaging;

using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace CSRefactorCurio
{
    public class CurioExplorerToolWindow : BaseToolWindow<CurioExplorerToolWindow>
    {
        public override string GetTitle(int toolWindowId) => "CS Refactor Curio";

        public override Type PaneType => typeof(Pane);

        public override Task<FrameworkElement> CreateAsync(int toolWindowId, CancellationToken cancellationToken)
        {
            return Task.FromResult<FrameworkElement>(new CurioExplorer());
        }

        [Guid("dd72a1c2-bbc9-43ce-a17f-e9609876669b")]
        internal class Pane : ToolWindowPane
        {
            public Pane()
            {
                BitmapImageMoniker = KnownMonikers.ToolWindow;
            }
        }
    }
}