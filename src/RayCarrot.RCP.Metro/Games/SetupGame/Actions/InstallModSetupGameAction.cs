using RayCarrot.RCP.Metro.ModLoader.Library;

namespace RayCarrot.RCP.Metro.Games.SetupGame;

public abstract class InstallModSetupGameAction : SetupGameAction
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    protected abstract long GameBananaModId { get; }
    protected abstract string ModId { get; }

    public override GenericIconKind FixActionIcon => GenericIconKind.SetupGame_Mod;
    public override LocalizedString? FixActionDisplayName => "Download mod"; // TODO-LOC

    public override bool CheckIsComplete(GameInstallation gameInstallation)
    {
        try
        {
            ModLibrary library = new(gameInstallation);
            ModManifest modManifest = library.ReadModManifest();
            return modManifest.Mods.TryGetValue(ModId, out ModManifestEntry entry) && entry.IsEnabled;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Checking if mod is installed");
            return false;
        }
    }

    public override async Task FixAsync(GameInstallation gameInstallation)
    {
        await Services.UI.ShowModLoaderAsync(gameInstallation, GameBananaModId);
    }
}