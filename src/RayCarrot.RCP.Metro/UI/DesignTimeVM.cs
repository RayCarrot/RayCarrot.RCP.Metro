namespace RayCarrot.RCP.Metro;

/// <summary>
/// Design time view models
/// </summary>
public static class DesignTimeVM
{
    public static DriveSelectionViewModel DriveSelectionViewModel
    {
        get
        {
            var vm = new DriveSelectionViewModel();

            _ = vm.RefreshAsync();

            return vm;
        }
    }
}