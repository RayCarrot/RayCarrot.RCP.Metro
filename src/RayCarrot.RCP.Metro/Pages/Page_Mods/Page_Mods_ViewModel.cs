#nullable disable
using System;
using NLog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for the mods page
/// </summary>
public class Page_Mods_ViewModel : BaseRCPViewModel
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

        App.SelectedPageChanged += App_SelectedPageChangedAsync;
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private bool _hasInitialized;

    private async void App_SelectedPageChangedAsync(object sender, PropertyChangedEventArgs<AppPage> e)
    {
        if (e.NewValue != AppPage.Mods)
            return;

        if (_hasInitialized)
            return;

        _hasInitialized = true;

        foreach (var mod in Mods)
        {
            Logger.Info("Initializing mod {0}", mod.Header.Value);
            await mod.InitializeAsync();
        }

        Logger.Info("Initialized mods");
    }

    public Mod_BaseViewModel[] Mods { get; }
}