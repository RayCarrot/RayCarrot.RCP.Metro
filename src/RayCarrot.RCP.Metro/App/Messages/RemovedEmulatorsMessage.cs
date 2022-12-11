using RayCarrot.RCP.Metro.Games.Emulators;

namespace RayCarrot.RCP.Metro;

public record RemovedEmulatorsMessage(IList<EmulatorInstallation> EmulatorInstallations)
{
    public RemovedEmulatorsMessage(params EmulatorInstallation[] emulatorInstallations) 
        : this((IList<EmulatorInstallation>)emulatorInstallations) { }
};