using System;
using System.Threading.Tasks;
using NLog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for the mods page
/// </summary>
public class Page_Mods_ViewModel : BasePageViewModel, IDisposable
{
    public Page_Mods_ViewModel(AppViewModel app) : base(app)
    {
        Mods = new Mod_BaseViewModel[]
        {
            new Mod_R1_ViewModel(),
            new Mod_RRR_ViewModel(),
        };
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override AppPage Page => AppPage.Mods;
    public Mod_BaseViewModel[] Mods { get; }

    protected override async Task InitializeAsync()
    {
        foreach (var mod in Mods)
        {
            Logger.Info("Initializing mod {0}", mod.Header.Value);
            await mod.InitializeAsync();
        }
    }

    public void Dispose()
    {
        foreach (Mod_BaseViewModel mod in Mods)
            if (mod is IDisposable d)
                d.Dispose();
    }
}