namespace RayCarrot.RCP.Metro.Games.Structure;

// TODO: This might need to change in the future. Maybe use constant strings instead of an enum to increase flexibility?
//       The purpose of this is to be able to get common types of paths from a game in an abstract way.
public enum GameInstallationPathType
{
    PrimaryExe,
    ConfigExe,
    OtherExe,
    Data,
    Save,
    Other,
}