﻿using BinarySerializer.Ray1;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.Ray1;
using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Finder;
using RayCarrot.RCP.Metro.Games.OptionsDialog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Designer (MS-DOS) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanDesigner_MSDOS : MsDosGameDescriptor
{
    #region Public Properties

    public override string GameId => "RaymanDesigner_MSDOS";
    public override Game Game => Game.RaymanDesigner;
    public override GameCategory Category => GameCategory.Rayman;
    public override LegacyGame? LegacyGame => Metro.LegacyGame.RaymanDesigner;
    
    public override LocalizedString DisplayName => "Rayman Designer";
    public override string DefaultFileName => "RAYKIT.bat";
    public override DateTime ReleaseDate => new(1997, 01, 01); // Not exact

    public override GameIconAsset Icon => GameIconAsset.RaymanDesigner;

    public override bool HasArchives => true;

    public override string ExecutableName => "RAYKIT.EXE";

    #endregion

    #region Private Methods

    private static IEnumerable<GameLinksComponent.GameUriLink> GetLocalGameLinks(GameInstallation gameInstallation) => new[]
    {
        new GameLinksComponent.GameUriLink(
            Header: new ResourceLocString(nameof(Resources.GameLink_RDMapper)), 
            Uri: gameInstallation.InstallLocation + "MAPPER.EXE")
    };

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new ProgressionManagersComponent(x => new GameProgressionManager_RaymanDesigner(x, "Rayman Designer")));
        builder.Register(new GameConfigComponent(x => new RaymanDesignerConfigViewModel(this, x)));
        builder.Register<OnGameAddedComponent, AddToJumpListOnGameAddedComponent>();
        builder.Register<OnGameAddedComponent, FindRaymanForeverFilesOnGameAddedComponent>();
        builder.Register<MsDosGameRequiresDiscComponent>();
        builder.Register(new LocalGameLinksComponent(GetLocalGameLinks));
        builder.Register(new RayMapComponent(RayMapComponent.RayMapViewer.Ray1Map, "RaymanDesignerPC", "r1/pc_kit"));

        builder.Register(new UtilityComponent(x => new Utility_RaymanDesigner_ReplaceFiles(x)));
        builder.Register(new UtilityComponent(x => new Utility_RaymanDesigner_CreateConfig(x)));
    }

    #endregion

    #region Public Methods

    public override IEnumerable<GameAddAction> GetAddActions() => new GameAddAction[]
    {
        new LocateRayman1MSDOSGameAddAction(this),
    };

    public override IArchiveDataManager GetArchiveDataManager(GameInstallation? gameInstallation) => 
        new Ray1PCArchiveDataManager(new Ray1Settings(Ray1EngineVersion.PC_Kit));

    public override IEnumerable<string> GetArchiveFilePaths(GameInstallation? gameInstallation) => new[]
    {
        @"PCMAP\COMMON.DAT",
        @"PCMAP\SNDD8B.DAT",
        @"PCMAP\SNDH8B.DAT",
        @"PCMAP\VIGNET.DAT",

        @"PCMAP\AL\SNDSMP.DAT",
        @"PCMAP\AL\SPECIAL.DAT",

        @"PCMAP\FR\SNDSMP.DAT",
        @"PCMAP\FR\SPECIAL.DAT",

        @"PCMAP\USA\SNDSMP.DAT",
        @"PCMAP\USA\SPECIAL.DAT",
    };

    public override IEnumerable<GamePurchaseLink> GetPurchaseLinks() => new GamePurchaseLink[]
    {
        new(new ResourceLocString(nameof(Resources.GameDisplay_PurchaseGOG)), "https://www.gog.com/game/rayman_forever"),
        new(new ResourceLocString(nameof(Resources.GameDisplay_PurchaseUplay)), "https://store.ubi.com/eu/rayman--forever/5800d3fc4e016524248b4567.html")
    };

    public override FinderQuery[] GetFinderQueries()
    {
        static FileSystemPath validateLocation(FileSystemPath location)
        {
            const string gameName = "RayKit";

            if (location.Name.Equals("DOSBOX", StringComparison.OrdinalIgnoreCase))
                return location.Parent + gameName;
            else
                return location + gameName;
        }

        return new FinderQuery[]
        {
            new UninstallProgramFinderQuery("Rayman Forever") { ValidateLocationFunc = validateLocation },
            new Win32ShortcutFinderQuery("Rayman Forever") { ValidateLocationFunc = validateLocation },
        };
    }

    #endregion
}