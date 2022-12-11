namespace RayCarrot.RCP.Metro;

public abstract class Mod_BaseViewModel : BaseRCPViewModel
{
    public abstract GenericIconKind Icon { get; }
    public abstract LocalizedString Header { get; }
    public abstract object UIContent { get; }

    public abstract Task InitializeAsync();
}