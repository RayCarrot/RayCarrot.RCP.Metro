﻿using System.Collections.Generic;
using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Raving Rabbids (Steam) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanRavingRabbids_Steam : SteamGameDescriptor
{
    #region Public Properties

    public override string GameId => "RaymanRavingRabbids_Steam";
    public override Game Game => Game.RaymanRavingRabbids;
    public override GameCategory Category => GameCategory.Rabbids;
    public override LegacyGame? LegacyGame => Metro.LegacyGame.RaymanRavingRabbids;

    public override string DisplayName => "Rayman Raving Rabbids";
    public override string DefaultFileName => "CheckApplication.exe";

    public override GameIconAsset Icon => GameIconAsset.RaymanRavingRabbids;

    public override string SteamID => "15080";

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(DescriptorComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new ProgressionManagersComponent(x => new GameProgressionManager_RaymanRavingRabbids(x, "Rayman Raving Rabbids")));
    }

    #endregion

    #region Public Methods

    public override GameOptionsDialog_ConfigPageViewModel GetConfigPageViewModel(GameInstallation gameInstallation) => 
        new Config_RaymanRavingRabbids_ViewModel(gameInstallation);

    public override IEnumerable<GameUriLink> GetLocalUriLinks(GameInstallation gameInstallation) => new GameUriLink[]
    {
        new(new ResourceLocString(nameof(Resources.GameLink_Setup)), gameInstallation.InstallLocation + "SettingsApplication.exe")
    };

    #endregion
}