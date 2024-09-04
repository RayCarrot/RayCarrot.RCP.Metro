using System.Net.Http;
using RayCarrot.RCP.Metro.ModLoader.Sources.GameBanana;

namespace RayCarrot.RCP.Metro.Games.SetupGame;

public class SetupGameModAction : SetupGameAction
{
    // TODO-LOC
    public SetupGameModAction(LocalizedString header, LocalizedString info, bool isComplete, long gameBananaModId) 
        : base(header, info, isComplete, GenericIconKind.SetupGame_DownloadMod, "Download mod", () => DownloadModAsync(gameBananaModId))
    {

    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private static async Task DownloadModAsync(long gameBananaModId)
    {
        GameBananaModsSource gb = new();
        GameBananaFile? file;

        try
        {
            using HttpClient httpClient = new();

            // Get the mod
            GameBananaMod mod = await httpClient.GetDeserializedAsync<GameBananaMod>(
                $"https://gamebanana.com/apiv11/Mod/{gameBananaModId}?" +
                $"_csvProperties=_aFiles,_aModManagerIntegrations");

            if (mod.Files == null)
            {
                // TODO-LOC
                await Services.MessageUI.DisplayMessageAsync("No valid files were found for the mod", MessageType.Error);
                return;
            }

            // Get the most recent file
            file = gb.GetValidFiles(mod, mod.Files).OrderBy(x => x.DateAdded).LastOrDefault();

            if (file == null)
            {
                // TODO-LOC
                await Services.MessageUI.DisplayMessageAsync("No valid files were found for the mod", MessageType.Error);
                return;
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Getting mod download for setup game action");

            // TODO-LOC
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, "An error occurred when getting the download for the mod");

            return;
        }

        await Services.UI.ShowModLoaderAsync(file.DownloadUrl, file.File, gb.Id, new GameBananaInstallData(gameBananaModId, file.Id));
    }
}