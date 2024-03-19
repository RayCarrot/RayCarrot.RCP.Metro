using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.ModLoader.Modules.Deltas;

namespace RayCarrot.RCP.Metro.Games.Structure;

public abstract class RomProgramInstallationStructure : SingleFileProgramInstallationStructure
{
    protected RomProgramInstallationStructure(IReadOnlyCollection<RomLayout> romLayouts)
    {
        RomLayouts = romLayouts;
    }

    public override bool SupportGameFileFinder => true;

    public IReadOnlyCollection<RomLayout> RomLayouts { get; }

    public override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        // Single ROM-file games only support delta patch mods and can only modify its own file
        builder.Register(new ModModuleComponent(x => new DeltasModule(x.InstallLocation.FilePath)));
    }
}