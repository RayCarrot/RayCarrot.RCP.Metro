﻿using BinarySerializer.Ray1;
using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Data;
using RayCarrot.RCP.Metro.Games.Options;
using RayCarrot.RCP.Metro.Games.OptionsDialog;
using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Edutainment Qui (MS-DOS) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanEdutainmentQuiz_MsDos : MsDosGameDescriptor
{
    #region Constant Fields

    private const string PrimaryName = "QUI";

    #endregion

    #region Public Properties

    public override string GameId => "RaymanEdutainmentQui_MsDos";
    public override string LegacyGameId => "EducationalDos";
    public override Game Game => Game.RaymanEdutainment;
    public override GameCategory Category => GameCategory.Other;

    public override LocalizedString DisplayName => "Rayman Edutainment (Quiz)";
    public override DateTime ReleaseDate => new(1996, 01, 01); // Not exact

    public override GameIconAsset Icon => GameIconAsset.RaymanEdutainment;
    public override GameBannerAsset Banner => GameBannerAsset.Rayman1;

    #endregion

    #region Private Methods

    private static IEnumerable<GameProgressionManager> GetGameProgressionManagers(GameInstallation gameInstallation)
    {
        Ray1MsDosData data = gameInstallation.GetRequiredObject<Ray1MsDosData>(GameDataKey.Ray1_MsDosData);
        return data.AvailableVersions.Select(x => new GameProgressionManager_RaymanEdutainment(
            gameInstallation: gameInstallation,
            backupName: $"Educational Games - {x.Id}",
            primaryName: PrimaryName,
            version: x));
    }

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register<AdditionalLaunchActionsComponent, Ray1MsDosAdditionalLaunchActionsComponent>();
        builder.Register(new ProgressionManagersComponent(GetGameProgressionManagers));
        builder.Register<GameValidationCheckComponent, Ray1MsDosGameDataGameValidationCheckComponent>();
        builder.Register(new GameConfigComponent(x => new RaymanEdutainmentConfigViewModel(this, x, PrimaryName)));
        builder.Register<OnGameAddedComponent, SetRay1MsDosDataOnGameAddedComponent>(ComponentPriority.High);
        builder.Register<LaunchArgumentsComponent, Ray1LaunchArgumentsComponent>();
        builder.Register<MsDosGameRequiresDiscComponent>();
        builder.Register(new GameOptionsComponent(x => new Ray1MsDosGameOptionsViewModel(x)));
        builder.Register(new RayMapComponent(RayMapComponent.RayMapViewer.Ray1Map, "RaymanQuizPC", "r1/quiz/pc_gf", "GF"));
        builder.Register<BinaryGameModeComponent>(new Ray1GameModeComponent(Ray1GameMode.RaymanEducational_PC));
        builder.Register<ArchiveComponent, Ray1MsDosArchiveComponent>();
    }

    protected override ProgramInstallationStructure GetStructure() => new(new GameInstallationPath[]
    {
        // Files
        new GameInstallationFilePath($"RAY{PrimaryName}.EXE", GameInstallationPathType.PrimaryExe, required: true),

        // Directories
        new GameInstallationDirectoryPath("PCMAP", GameInstallationPathType.Data, required: true),
    });

    #endregion
}