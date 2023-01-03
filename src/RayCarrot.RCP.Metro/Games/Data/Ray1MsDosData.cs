using System.IO;

namespace RayCarrot.RCP.Metro.Games.Data;

public class Ray1MsDosData
{
    public Ray1MsDosData(string[] availableGameModes, string selectedGameMode)
    {
        AvailableGameModes = availableGameModes;
        SelectedGameMode = selectedGameMode;
    }

    /// <summary>
    /// The available game modes for the game. This gets set once when the game is added.
    /// </summary>
    public string[] AvailableGameModes { get; }

    /// <summary>
    /// The currently selected game mode. This determines how the game should launch by default.
    /// </summary>
    public string SelectedGameMode { get; set; }

    /// <summary>
    /// Creates a new <see cref="Ray1MsDosData"/> instance from a newly added game installation
    /// </summary>
    /// <param name="gameInstallation">The game installation to create the data for</param>
    /// <returns>The data instance</returns>
    public static Ray1MsDosData Create(GameInstallation gameInstallation)
    {
        // Find the available game modes. This might throw an exception, but we let
        // it do that if so since the game requires this to be added in order to work.
        string[] gameModes = Directory.
            GetDirectories(gameInstallation.InstallLocation + "PCMAP", "*", SearchOption.TopDirectoryOnly).
            Select(x => new FileSystemPath(x).Name).
            ToArray();

        // Make sure at least one game mode was found. The game requires at least one
        // in order to work and have one be selected.
        if (gameModes.Length == 0)
            throw new Exception("No game modes were found");

        return new Ray1MsDosData(gameModes, gameModes.First());
    }
}