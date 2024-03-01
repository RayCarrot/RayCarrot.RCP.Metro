namespace RayCarrot.RCP.Metro.Games.Tools.PrototypeRestoration;

public class MemoryModsSectonViewModel : BaseViewModel, IDisposable
{
    public MemoryModsSectonViewModel(LocalizedString header, LocalizedString? info, ObservableCollection<MemoryModToggleViewModel> modToggles)
    {
        Header = header;
        Info = info;
        ModToggles = modToggles;
        IsEnabled = true;
    }

    public LocalizedString Header { get; }
    public LocalizedString? Info { get; }
    public bool IsEnabled { get; set; }

    public ObservableCollection<MemoryModToggleViewModel> ModToggles { get; }

    public void Dispose()
    {
        Header.Dispose();
        Info?.Dispose();
        ModToggles.DisposeAll();
    }
}