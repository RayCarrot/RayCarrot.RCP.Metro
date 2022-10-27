using System;
using System.Collections.Generic;
using System.Linq;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Extension methods for <see cref="Games"/>
/// </summary>
public static class GamesExtensions
{
    // TODO-14: Remove most of these

    /// <summary>
    /// Determines if the specified game has been added to the program
    /// </summary>
    /// <param name="game">The game to check if it's added</param>
    /// <returns>True if the game has been added, otherwise false</returns>
    public static bool IsAdded(this Games game)
    {
        return Services.Data.Game_GameInstallations.Any(x => x.Game == game);
    }

    /// <summary>
    /// Gets the install directory for the game or an empty path if not found or if it doesn't exist
    /// </summary>
    /// <param name="game">The game to get the install directory for</param>
    /// <param name="throwIfNotFound">Indicates if an exception should be thrown if the directory is not found</param>
    /// <returns>The install directory or an empty path if not found or if it doesn't exist</returns>
    public static FileSystemPath GetInstallDir(this Games game, bool throwIfNotFound = true)
    {
        // Get the game installation
        GameInstallation? gameInstallation = Services.Data.Game_GameInstallations.FirstOrDefault(x => x.Game == game);

        // Make sure it's not null
        if (gameInstallation == null)
        {
            if (throwIfNotFound)
                throw new Exception($"The data for the requested game '{game}' could not be found");
            else
                return FileSystemPath.EmptyPath;
        }

        // Return the path
        return gameInstallation.InstallLocation;
    }

    public static IEnumerable<GameManager> GetLegacyManagers(this GameDescriptor game) =>
        GetLegacyManager(game).YieldToArray();

    public static GameManager GetLegacyManager(this GameDescriptor gameDescriptor)
    {
        return gameDescriptor switch
        {
            GameDescriptor_Rayman1_MSDOS => new GameManager_Rayman1_DOSBox(),
            GameDescriptor_RaymanDesigner_MSDOS => new GameManager_RaymanDesigner_DOSBox(),
            GameDescriptor_RaymanByHisFans_MSDOS => new GameManager_RaymanByHisFans_DOSBox(),
            GameDescriptor_Rayman60Levels_MSDOS => new GameManager_Rayman60Levels_DOSBox(),
            GameDescriptor_Rayman2_Win32 => new GameManager_Rayman2_Win32(),
            GameDescriptor_Rayman2_Steam => new GameManager_Rayman2_Steam(),
            GameDescriptor_RaymanM_Win32 => new GameManager_RaymanM_Win32(),
            GameDescriptor_RaymanArena_Win32 => new GameManager_RaymanArena_Win32(),
            GameDescriptor_Rayman3_Win32 => new GameManager_Rayman3_Win32(),
            GameDescriptor_RaymanOrigins_Win32 => new GameManager_RaymanOrigins_Win32(),
            // GameManager_RaymanOrigins_Steam
            GameDescriptor_RaymanLegends_Win32 => new GameManager_RaymanLegends_Win32(),
            // GameManager_RaymanLegends_Steam
            GameDescriptor_RaymanJungleRun_WindowsPackage => new GameManager_RaymanJungleRun_WinStore(),
            GameDescriptor_RaymanFiestaRun_WindowsPackage => new GameManager_RaymanFiestaRun_WinStore(),
            GameDescriptor_RaymanRavingRabbids_Win32 => new GameManager_RaymanRavingRabbids_Win32(),
            // GameManager_RaymanRavingRabbids_Steam
            GameDescriptor_RaymanRavingRabbids2_Win32 => new GameManager_RaymanRavingRabbids2_Win32(),
            GameDescriptor_RabbidsGoHome_Win32 => new GameManager_RabbidsGoHome_Win32(),
            GameDescriptor_RabbidsBigBang_WindowsPackage => new GameManager_RabbidsBigBang_WinStore(),
            GameDescriptor_RabbidsCoding_Win32 => new GameManager_RabbidsCoding_Win32(),
            GameDescriptor_Rayman1_Demo_19951207_MSDOS => new GameManager_Rayman1Demo1_DOSBox(),
            GameDescriptor_Rayman1_Demo_19960215_MSDOS => new GameManager_Rayman1Demo2_DOSBox(),
            GameDescriptor_Rayman1_Demo_19951204_MSDOS => new GameManager_Rayman1Demo3_DOSBox(),
            GameDescriptor_RaymanGold_Demo_19970930_MSDOS => new GameManager_RaymanGoldDemo_DOSBox(),
            GameDescriptor_Rayman2_Demo_19990818_Win32 => new GameManager_Rayman2Demo1_Win32(),
            GameDescriptor_Rayman2_Demo_19990904_Win32 => new GameManager_Rayman2Demo2_Win32(),
            GameDescriptor_RaymanM_Demo_20020627_Win32 => new GameManager_RaymanMDemo_Win32(),
            GameDescriptor_Rayman3_Demo_20021004_Win32 => new GameManager_Rayman3Demo1_Win32(),
            GameDescriptor_Rayman3_Demo_20021021_Win32 => new GameManager_Rayman3Demo2_Win32(),
            GameDescriptor_Rayman3_Demo_20021210_Win32 => new GameManager_Rayman3Demo3_Win32(),
            GameDescriptor_Rayman3_Demo_20030129_Win32 => new GameManager_Rayman3Demo4_Win32(),
            GameDescriptor_Rayman3_Demo_20030108_Win32 => new GameManager_Rayman3Demo5_Win32(),
            GameDescriptor_RaymanRavingRabbids_Demo_20061106_Win32 => new GameManager_RaymanRavingRabbidsDemo_Win32(),
            GameDescriptor_Ray1Minigames_Win32 => new GameManager_Ray1Minigames_Win32(),
            GameDescriptor_EducationalDos_MSDOS => new GameManager_EducationalDos_EducationalDOSBox(),
            GameDescriptor_TonicTrouble_Win32 => new GameManager_TonicTrouble_Win32(),
            GameDescriptor_TonicTroubleSpecialEdition_Win32 => new GameManager_TonicTroubleSpecialEdition_Win32(),
            GameDescriptor_RaymanDictées_Win32 => new GameManager_RaymanDictées_Win32(),
            GameDescriptor_RaymanPremiersClics_Win32 => new GameManager_RaymanPremiersClics_Win32(),
            GameDescriptor_PrintStudio_Win32 => new GameManager_PrintStudio_Win32(),
            GameDescriptor_RaymanActivityCenter_Win32 => new GameManager_RaymanActivityCenter_Win32(),
            GameDescriptor_RaymanRavingRabbidsActivityCenter_Win32 => new GameManager_RaymanRavingRabbidsActivityCenter_Win32(),
            GameDescriptor_TheDarkMagiciansReignofTerror_Win32 => new GameManager_TheDarkMagiciansReignofTerror_Win32(),
            GameDescriptor_RaymanRedemption_Win32 => new GameManager_RaymanRedemption_Win32(),
            GameDescriptor_RaymanRedesigner_Win32 => new GameManager_RaymanRedesigner_Win32(),
            GameDescriptor_RaymanBowling2_Win32 => new GameManager_RaymanBowling2_Win32(),
            GameDescriptor_RaymanGardenPLUS_Win32 => new GameManager_RaymanGardenPLUS_Win32(),
            GameDescriptor_GloboxMoment_Win32 => new GameManager_GloboxMoment_Win32(),
            _ => throw new Exception($"Invalid descriptor type {gameDescriptor.GetType()}"),
        };
    }
    public static T GetLegacyManager<T>(this GameDescriptor game) 
        where T : GameManager => (T)GetLegacyManager(game);

    /// <summary>
    /// Gets the game info for the specified game
    /// </summary>
    /// <param name="game">The game to get the info for</param>
    /// <returns>The info</returns>
    public static GameDescriptor GetGameDescriptor(this Games game) => GetGameDescriptor<GameDescriptor>(game);

    /// <summary>
    /// Gets the game info for the specified game
    /// </summary>
    /// <param name="game">The game to get the info for</param>
    /// <returns>The info</returns>
    public static T GetGameDescriptor<T>(this Games game)
        where T : GameDescriptor
    {
        var g = Services.Games;
        return (T)g.EnumerateGameDescriptors().First(x => x.LegacyGame == game);
    }

    // TODO-14: Remove once no longer needed
    public static GameInstallation GetInstallation(this Games game) => Services.Data.Game_GameInstallations.First(x => x.Game == game);
}