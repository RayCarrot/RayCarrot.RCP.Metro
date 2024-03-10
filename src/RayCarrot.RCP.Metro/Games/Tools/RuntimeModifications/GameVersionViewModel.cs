namespace RayCarrot.RCP.Metro.Games.Tools.RuntimeModifications;

public class GameVersionViewModel : BaseViewModel
{
    public GameVersionViewModel(RuntimeModificationsManager manager)
    {
        Manager = manager;
        DisplayName = manager.DisplayName;
    }

    public RuntimeModificationsManager Manager { get; }
    public LocalizedString DisplayName { get; }
}