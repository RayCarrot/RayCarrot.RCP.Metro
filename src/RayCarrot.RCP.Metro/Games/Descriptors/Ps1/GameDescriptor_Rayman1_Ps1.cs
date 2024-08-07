﻿using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Structure;
using RayCarrot.RCP.Metro.Games.Tools.RuntimeModifications;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 1 (PS1) game descriptor
/// </summary>
public sealed class GameDescriptor_Rayman1_Ps1 : Ps1GameDescriptor
{
    #region Public Properties

    public override string GameId => "Rayman1_Ps1";
    public override Game Game => Game.Rayman1;
    public override GameCategory Category => GameCategory.Rayman;

    public override LocalizedString DisplayName => new ResourceLocString(nameof(Resources.Rayman1_Ps1_Title));
    public override string[] SearchKeywords => new[] { "r1", "ray1" };
    public override DateTime ReleaseDate => new(1995, 09, 01);

    public override GameIconAsset Icon => GameIconAsset.Rayman1;
    public override GameBannerAsset Banner => GameBannerAsset.Rayman1;

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new ProgressionManagersComponent(x => new GameProgressionManager_Rayman1_Ps1(x, "Rayman 1 - PS1")));
        builder.Register(new RayMapComponent(RayMapComponent.RayMapViewer.Ray1Map, "RaymanPS1US", "r1/ps1_us"));
        builder.Register<BinaryGameModeComponent>(new Ray1GameModeComponent(Ray1GameMode.Rayman1_PS1));

        builder.Register(new RuntimeModificationsGameManagersComponent(EmulatedPlatform.Ps1, _ =>
            new[]
            {
                new Ray1GameManager(
                    displayName: new ResourceLocString(nameof(Resources.Mod_Mem_Game_R1_PS1_US)),
                    getOffsetsFunc: () => Ray1MemoryData.Offsets_PS1_US)
            }));
    }

    protected override ProgramInstallationStructure CreateStructure() => new Ps1DiscProgramInstallationStructure(new[]
    {
        new Ps1DiscProgramLayout("EU", "BE", "SLES-00049", 51, new ProgramFileSystem(new ProgramPath[]
        {
            new ProgramFilePath("SLES_000.49;1", ProgramPathType.PrimaryExe, required: true),
            new ProgramDirectoryPath("RAY", ProgramPathType.Data, required: true)
            {
                new ProgramFilePath("RAY.XXX;1", ProgramPathType.Data, required: true),
            },
        })),
        new Ps1DiscProgramLayout("US", "BI", "SLUS-00005", 51, new ProgramFileSystem(new ProgramPath[]
        {
            new ProgramFilePath("SLUS-000.05;1", ProgramPathType.PrimaryExe, required: true),
            new ProgramDirectoryPath("RAY", ProgramPathType.Data, required: true)
            {
                new ProgramFilePath("RAY.XXX;1", ProgramPathType.Data, required: true),
            },
        })),
        new Ps1DiscProgramLayout("JP", "BI", "SLPS-00026", 48, new ProgramFileSystem(new ProgramPath[]
        {
            new ProgramFilePath("PSX.EXE;1", ProgramPathType.PrimaryExe, required: true),
            new ProgramDirectoryPath("RAY", ProgramPathType.Data, required: true)
            {
                new ProgramFilePath("RAY.XXX;1", ProgramPathType.Data, required: true),
            },
        })),
    });

    #endregion

    #region Public Methods

    public override IEnumerable<GameAddAction> GetAddActions() => new GameAddAction[]
    {
        new LocateFileGameAddAction(this),
    };

    #endregion
}