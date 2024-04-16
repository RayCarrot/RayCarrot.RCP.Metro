using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.ModLoader.Modules.Deltas;

namespace RayCarrot.RCP.Metro.Games.Structure;

public class ExeProgramInstallationStructure : SingleFileProgramInstallationStructure
{
    public override bool SupportGameFileFinder => false;
    public override FileExtension[] SupportedFileExtensions => new[]
    {
        new FileExtension(".exe"),
    };

    public override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        // Single exe-file games only support delta patch mods and can only modify its own file
        builder.Register(new ModModuleComponent(x => new DeltasModule(x.InstallLocation.FilePath)));
    }
}