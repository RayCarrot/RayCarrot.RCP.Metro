using System.Threading.Tasks;
using NLog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for the mods page
/// </summary>
public class Page_Mods_ViewModel : BasePageViewModel
{ 
    /// <summary>
    /// Default constructor
    /// </summary>
    public Page_Mods_ViewModel()
    {
        Mods = new Mod_BaseViewModel[]
        {
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
}