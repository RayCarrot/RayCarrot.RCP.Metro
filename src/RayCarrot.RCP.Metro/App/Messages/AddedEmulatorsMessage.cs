using System.Collections.Generic;
using RayCarrot.RCP.Metro.Games.Emulators;

namespace RayCarrot.RCP.Metro;

public record AddedEmulatorsMessage(IList<EmulatorInstallation> EmulatorInstallations)
{
    public AddedEmulatorsMessage(params EmulatorInstallation[] emulatorInstallations) 
        : this((IList<EmulatorInstallation>)emulatorInstallations) { }
};