﻿using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman The Dark Magician's Reign of Terror (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_TheDarkMagiciansReignofTerror_Win32 : Win32GameDescriptor
{
    #region Constant Fields

    private const string GameJoltUrl = "https://gamejolt.com/games/Rayman_The_Dark_Magicians_Reign_of_terror/237701";

    #endregion

    #region Public Properties

    public override string GameId => "TheDarkMagiciansReignofTerror_Win32";
    public override Game Game => Game.TheDarkMagiciansReignofTerror;
    public override GameCategory Category => GameCategory.Fan;
    public override LegacyGame? LegacyGame => Metro.LegacyGame.TheDarkMagiciansReignofTerror;

    public override LocalizedString DisplayName => "Rayman: The Dark Magician's Reign of Terror";
    public override DateTime ReleaseDate => new(2015, 07, 13); // A bit unclear what the actual date is

    public override GameIconAsset Icon => GameIconAsset.TheDarkMagiciansReignofTerror;

    // TODO-14: Should we be removing this?
    public override IEnumerable<FileSystemPath> UninstallDirectories => new FileSystemPath[]
    {
        Environment.SpecialFolder.LocalApplicationData.GetFolderPath() + "Rayman__Dark_Magician_s_reign_of_terror_"
    };

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new ProgressionManagersComponent(x => new GameProgressionManager_TheDarkMagiciansReignofTerror(x, "Rayman The Dark Magicians Reign of Terror")));
        builder.Register<ExternalGameLinksComponent>(new GameJoltExternalGameLinksComponent(GameJoltUrl));
    }

    protected override GameInstallationStructure GetStructure() => new(new GameInstallationPath[]
    {
        // Files
        new GameInstallationFilePath("Rayman! Dark Magician's reign of terror!.exe", GameInstallationPathType.PrimaryExe, required: true),
    });

    #endregion

    #region Public Methods

    public override IEnumerable<GamePurchaseLink> GetPurchaseLinks() => new GamePurchaseLink[]
    {
        new(new ResourceLocString(nameof(Resources.GameDisplay_GameJolt)), GameJoltUrl, GenericIconKind.GameAction_Web),
    };

    #endregion
}