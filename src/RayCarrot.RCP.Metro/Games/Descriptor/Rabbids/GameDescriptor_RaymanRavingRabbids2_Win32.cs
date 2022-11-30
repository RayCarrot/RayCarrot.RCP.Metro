using System.Collections.Generic;
using System.Windows;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Raving Rabbids 2 (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanRavingRabbids2_Win32 : Win32GameDescriptor
{
    #region Public Overrides

    public override string Id => "RaymanRavingRabbids2_Win32";
    public override Game Game => Game.RaymanRavingRabbids2;
    public override GameCategory Category => GameCategory.Rabbids;
    public override Games? LegacyGame => Games.RaymanRavingRabbids2;

    public override string DisplayName => "Rayman Raving Rabbids 2";
    public override string DefaultFileName => "Jade.exe";

    public override GameIconAsset Icon => GameIconAsset.RaymanRavingRabbids2;

    #endregion

    #region Protected Methods

    protected override string GetLaunchArgs(GameInstallation gameInstallation)
    {
        UserData_RRR2LaunchMode launchMode = gameInstallation.GetValue(GameDataKey.RRR2LaunchMode, UserData_RRR2LaunchMode.AllGames);
        return $"/{launchMode.ToString().ToLower()} /B Rrr2.bf";
    }

    #endregion

    #region Public Methods

    public override FrameworkElement GetOptionsUI(GameInstallation gameInstallation) => 
        new GameOptions_RavingRabbids2_Control(new GameOptions_RavingRabbids2_ViewModel(gameInstallation));

    public override GameOptionsDialog_ConfigPageViewModel GetConfigPageViewModel(GameInstallation gameInstallation) =>
        new Config_RaymanRavingRabbids2_ViewModel(gameInstallation);

    public override IEnumerable<GameProgressionManager> GetGameProgressionManagers(GameInstallation gameInstallation) => 
        new GameProgressionManager_RaymanRavingRabbids2(gameInstallation, "Rayman Raving Rabbids 2").Yield();

    public override IEnumerable<GameUriLink> GetLocalUriLinks(GameInstallation gameInstallation)
    {
        UserData_RRR2LaunchMode launchMode = gameInstallation.GetValue(GameDataKey.RRR2LaunchMode, UserData_RRR2LaunchMode.AllGames);

        return new GameUriLink[]
        {
            new(new ResourceLocString(nameof(Resources.GameLink_Setup)), gameInstallation.InstallLocation + "SettingsApplication.exe",
                Arguments: $"/{launchMode.ToString().ToLower()}")
        };
    }

    public override GameFinder_GameItem GetGameFinderItem() => new(null, "Rayman Raving Rabbids 2", new[]
    {
        "Rayman Raving Rabbids 2",
        "Rayman: Raving Rabbids 2",
        "Rayman Raving Rabbids 2 Orange",
        "Rayman: Raving Rabbids 2 Orange",
        "RRR2",
    });

    #endregion
}