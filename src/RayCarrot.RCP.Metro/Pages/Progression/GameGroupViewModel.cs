namespace RayCarrot.RCP.Metro.Pages.Progression;

public class GameGroupViewModel : BaseViewModel
{
    public GameGroupViewModel(GameIconAsset icon, LocalizedString displayName)
    {
        Icon = icon;
        DisplayName = displayName;
    }

    public GameIconAsset Icon { get; }
    public LocalizedString DisplayName { get; }
}