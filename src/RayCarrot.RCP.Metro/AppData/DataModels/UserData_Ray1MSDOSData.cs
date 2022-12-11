using System.IO;

namespace RayCarrot.RCP.Metro;

public class UserData_Ray1MsDosData
{
    public UserData_Ray1MsDosData(string[] availableGameModes, string selectedGameMode)
    {
        AvailableGameModes = availableGameModes;
        SelectedGameMode = selectedGameMode;
    }

    public string[] AvailableGameModes { get; }
    public string SelectedGameMode { get; set; }

    public static UserData_Ray1MsDosData Create(GameInstallation gameInstallation)
    {
        // TODO-14: Try/catch and handle? Or let it fail?
        // Find the available game modes
        string[] gameModes = Directory.
            GetDirectories(gameInstallation.InstallLocation + "PCMAP", "*", SearchOption.TopDirectoryOnly).
            Select(x => new FileSystemPath(x).Name).
            ToArray();

        // TODO-14: Verify collection is not empty
        return new UserData_Ray1MsDosData(gameModes, gameModes.First());
    }
}