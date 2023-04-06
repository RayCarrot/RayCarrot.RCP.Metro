namespace RayCarrot.RCP.Metro.Games.Structure;

public abstract class RomProgramInstallationStructure : SingleFileProgramInstallationStructure
{
    protected RomProgramInstallationStructure(IReadOnlyCollection<RomLayout> romLayouts)
    {
        RomLayouts = romLayouts;
    }

    public IReadOnlyCollection<RomLayout> RomLayouts { get; }
}