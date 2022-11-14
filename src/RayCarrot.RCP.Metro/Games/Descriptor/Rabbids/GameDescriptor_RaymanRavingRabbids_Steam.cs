using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Raving Rabbids (Steam) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanRavingRabbids_Steam : SteamGameDescriptor
{
    #region Public Properties

    public override string Id => "RaymanRavingRabbids_Steam";
    public override Game Game => Game.RaymanRavingRabbids;
    public override GameCategory Category => GameCategory.Rabbids;
    public override Games? LegacyGame => Games.RaymanRavingRabbids;

    public override string DisplayName => "Rayman Raving Rabbids";
    public override string BackupName => "Rayman Raving Rabbids";
    public override string DefaultFileName => "CheckApplication.exe";

    public override string SteamID => "15080";

    #endregion

    #region Public Methods

    public override GameOptionsDialog_ConfigPageViewModel GetConfigPageViewModel(GameInstallation gameInstallation) => 
        new Config_RaymanRavingRabbids_ViewModel(gameInstallation);

    public override GameProgressionManager GetGameProgressionManager(GameInstallation gameInstallation) => 
        new GameProgressionManager_RaymanRavingRabbids(gameInstallation);

    public override IEnumerable<GameUriLink> GetLocalUriLinks(GameInstallation gameInstallation) => new GameUriLink[]
    {
        new(new ResourceLocString(nameof(Resources.GameLink_Setup)), gameInstallation.InstallLocation + "SettingsApplication.exe")
    };

    #endregion
}