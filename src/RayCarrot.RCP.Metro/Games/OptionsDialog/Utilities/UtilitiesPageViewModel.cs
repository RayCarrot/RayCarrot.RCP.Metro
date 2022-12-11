namespace RayCarrot.RCP.Metro.Games.OptionsDialog;

public class UtilitiesPageViewModel : GameOptionsDialogPageViewModel
{
    public UtilitiesPageViewModel(IEnumerable<UtilityViewModel> utilities)
    {
        Utilities = new ObservableCollection<UtilityViewModel>(utilities);
    }

    public override LocalizedString PageName => new ResourceLocString(nameof(Resources.GameOptions_Utilities));
    public override GenericIconKind PageIcon => GenericIconKind.GameOptions_Utilities;

    /// <summary>
    /// The utilities for the game
    /// </summary>
    public ObservableCollection<UtilityViewModel> Utilities { get; }

    public override void Dispose()
    {
        base.Dispose();

        Utilities.DisposeAll();
    }
}