namespace RayCarrot.RCP.Metro.Pages.Mods;

/// <summary>
/// View model for the mods page
/// </summary>
public class ModsPageViewModel : BasePageViewModel, IDisposable
{
    public ModsPageViewModel(AppViewModel app, IMessageUIManager messageUi) : base(app)
    {
        Mods = new Mod_BaseViewModel[]
        {
            new Mod_Mem_ViewModel(messageUi),
            new Mod_RRR_ViewModel(),
        };
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override AppPage Page => AppPage.Mods;
    public Mod_BaseViewModel[] Mods { get; }

    // IDEA: Have lazy loading for each mod instead of initializing all at once
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