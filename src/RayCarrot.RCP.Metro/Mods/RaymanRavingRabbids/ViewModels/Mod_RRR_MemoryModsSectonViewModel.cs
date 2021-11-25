#nullable disable
using System;
using System.Collections.ObjectModel;

namespace RayCarrot.RCP.Metro;

public class Mod_RRR_MemoryModsSectonViewModel : BaseRCPViewModel, IDisposable
{
    public Mod_RRR_MemoryModsSectonViewModel(LocalizedString header, LocalizedString info, ObservableCollection<Mod_RRR_MemoryModToggleViewModel> modToggles)
    {
        Header = header;
        Info = info;
        ModToggles = modToggles;
        IsEnabled = true;
    }

    public LocalizedString Header { get; }
    public LocalizedString Info { get; }
    public bool IsEnabled { get; set; }

    public ObservableCollection<Mod_RRR_MemoryModToggleViewModel> ModToggles { get; }

    public void Dispose()
    {
        Header?.Dispose();
        ModToggles?.DisposeAll();
    }
}