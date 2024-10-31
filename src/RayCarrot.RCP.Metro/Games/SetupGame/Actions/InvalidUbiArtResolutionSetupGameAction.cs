using Microsoft.Win32;

namespace RayCarrot.RCP.Metro.Games.SetupGame;

public class InvalidUbiArtResolutionSetupGameAction : SetupGameAction
{
    public InvalidUbiArtResolutionSetupGameAction(string registryKey)
    {
        RegistryKey = registryKey;
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public string RegistryKey { get; }

    public override LocalizedString Header => new ResourceLocString(nameof(Resources.SetupGameAction_InvalidUbiArtResolution_Header));
    public override LocalizedString Info => new ResourceLocString(nameof(Resources.SetupGameAction_InvalidUbiArtResolution_Info));

    public override SetupGameActionType Type => SetupGameActionType.Issue;

    public override GenericIconKind FixActionIcon => GenericIconKind.SetupGame_GameSettings;
    public override LocalizedString FixActionDisplayName => new ResourceLocString(nameof(Resources.SetupGameAction_GameSettingsFix));

    public override bool CheckIsAvailable(GameInstallation gameInstallation)
    {
        try
        {
            string keyPath = RegistryHelpers.CombinePaths(RegistryKey, "Settings");
            using RegistryKey? key = RegistryHelpers.GetKeyFromFullPath(keyPath, RegistryView.Default, writable: false);

            if (key == null)
                return false;

            // The resolution is only an issue if in fullscreen mode
            int fullscreen = getInt("FullScreen");
            if (fullscreen == 0)
                return false;

            int width = getInt("ScreenWidth");
            int height = getInt("ScreenHeight");

            if (width == -1 || height == -1)
                return false;

            Display.DEVMODE vDevMode = new();

            int i = 0;
            while (Display.EnumDisplaySettings(null, i, ref vDevMode))
            {
                // Check if the resolution is valid
                if (vDevMode.dmPelsWidth == width && vDevMode.dmPelsHeight == height)
                    return false;
                i++;
            }

            return true;

            // Helper methods for getting values
            int getInt(string valueName) =>
                Int32.TryParse(key.GetValue(valueName)?.ToString().KeepFirstDigitsOnly(), out int result) ? result : -1;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Checking game resolution");
            return false;
        }
    }

    public override bool CheckIsComplete(GameInstallation gameInstallation)
    {
        return false;
    }

    public override async Task FixAsync(GameInstallation gameInstallation)
    {
        await Services.UI.ShowGameSettingsAsync(gameInstallation);
    }
}