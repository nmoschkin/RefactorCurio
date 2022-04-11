namespace CSRefactorCurio
{
    [Command(PackageIds.ShowCurioExplorerCommand)]
    internal sealed class ShowCurioExplorerCommand : BaseCommand<ShowCurioExplorerCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await CurioExplorerToolWindow.ShowAsync();
        }
    }
}
