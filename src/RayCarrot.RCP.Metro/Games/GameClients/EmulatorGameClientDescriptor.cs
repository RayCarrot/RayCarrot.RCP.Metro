namespace RayCarrot.RCP.Metro.Games.Clients;

public abstract class EmulatorGameClientDescriptor : GameClientDescriptor
{
    /// <summary>
    /// The game platforms which this emulator supports
    /// </summary>
    public abstract GamePlatform[] SupportedPlatforms { get; }

    public override bool SupportsGame(GameInstallation gameInstallation) =>
        SupportedPlatforms.Contains(gameInstallation.GameDescriptor.Platform);
}