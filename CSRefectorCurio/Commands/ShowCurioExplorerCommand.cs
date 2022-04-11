namespace CSRefectorCurio
{
    [Command(PackageIds.ShowCurioExplorerCommand)]
    internal sealed class ShowCurioExplorerCommand : BaseCommand<ShowCurioExplorerCommand>
    {
        protected override Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            return CurioExplorerToolWindow.ShowAsync();
        }
    }



}
