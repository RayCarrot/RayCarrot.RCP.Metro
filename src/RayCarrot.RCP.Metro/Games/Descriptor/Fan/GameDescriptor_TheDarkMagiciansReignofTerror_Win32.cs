using System;
using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman The Dark Magician's Reign of Terror (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_TheDarkMagiciansReignofTerror_Win32 : Win32GameDescriptor
{
    #region Public Properties

    public override string Id => "TheDarkMagiciansReignofTerror_Win32";
    public override Game Game => Game.TheDarkMagiciansReignofTerror;
    public override GameCategory Category => GameCategory.Fan;
    public override Games? LegacyGame => Games.TheDarkMagiciansReignofTerror;

    public override string DisplayName => "Rayman: The Dark Magician's Reign of Terror";
    public override string BackupName => "Rayman The Dark Magicians Reign of Terror";
    public override string DefaultFileName => "Rayman! Dark Magician's reign of terror!.exe";

    public override GameIconAsset Icon => GameIconAsset.TheDarkMagiciansReignofTerror;

    // TODO-14: Should we be removing this?
    public override IEnumerable<FileSystemPath> UninstallDirectories => new FileSystemPath[]
    {
        Environment.SpecialFolder.LocalApplicationData.GetFolderPath() + "Rayman__Dark_Magician_s_reign_of_terror_"
    };

    #endregion

    #region Public Methods

    public override GameProgressionManager GetGameProgressionManager(GameInstallation gameInstallation) =>
        new GameProgressionManager_TheDarkMagiciansReignofTerror(gameInstallation);

    public override IEnumerable<GameUriLink> GetExternalUriLinks(GameInstallation gameInstallation) => new[]
    {
        new GameUriLink(
            Header: new ResourceLocString(nameof(Resources.GameDisplay_OpenGameJoltPage)),
            Uri: "https://gamejolt.com/games/Rayman_The_Dark_Magicians_Reign_of_terror/237701",
            Icon: GenericIconKind.GameDisplay_Web)
    };

    public override IEnumerable<GamePurchaseLink> GetPurchaseLinks() => new GamePurchaseLink[]
    {
        new(new ResourceLocString(nameof(Resources.GameDisplay_GameJolt)), "https://gamejolt.com/games/Rayman_The_Dark_Magicians_Reign_of_terror/237701", GenericIconKind.GameDisplay_Web),
    };

    #endregion
}