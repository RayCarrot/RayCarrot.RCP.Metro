using RayCarrot.RCP.Metro.ModLoader.Dialogs.ModLoader;
using RayCarrot.RCP.Metro.ModLoader.Library;
using RayCarrot.RCP.Metro.ModLoader.Sources.GameBanana;

namespace RayCarrot.RCP.Metro.Games.SetupGame;

public abstract class InstallModSetupGameAction : SetupGameAction
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    protected abstract long GameBananaModId { get; }
    protected abstract string[] ModIds { get; }

    public override GenericIconKind FixActionIcon => GenericIconKind.SetupGame_Mod;
    public override LocalizedString FixActionDisplayName => new ResourceLocString(nameof(Resources.SetupGameAction_DownloadModFix));

    public override bool CheckIsComplete(GameInstallation gameInstallation)
    {
        try
        {
            ModLibrary library = new(gameInstallation);
            ModManifest modManifest = library.ReadModManifest();
            return ModIds.Any(x => modManifest.Mods.TryGetValue(x, out ModManifestEntry entry) && entry.IsEnabled);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Checking if mod is installed");
            return false;
        }
    }

    public override async Task FixAsync(GameInstallation gameInstallation)
    {
        await Services.UI.ShowModLoaderAsync(gameInstallation, _ =>
        {
            Services.Messenger.Send(new OpenModDownloadPageMessage(gameInstallation, new GameBananaInstallData(GameBananaModId, -1)));
            return Task.CompletedTask;
        });
    }
}