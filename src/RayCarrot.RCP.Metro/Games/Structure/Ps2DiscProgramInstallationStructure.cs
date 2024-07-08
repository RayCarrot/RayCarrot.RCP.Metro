using BinarySerializer.Disk.ISO9660;

namespace RayCarrot.RCP.Metro.Games.Structure;

public class Ps2DiscProgramInstallationStructure : SingleFileProgramInstallationStructure
{
    public Ps2DiscProgramInstallationStructure(Ps2DiscProgramLayout[] layouts) : base(layouts) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override bool SupportGameFileFinder => true;
    public override FileExtension[] SupportedFileExtensions => new[]
    {
        new FileExtension(".iso"),
    };

    public override ProgramLayout? FindMatchingLayout(InstallLocation location)
    {
        if (!location.HasFile)
            throw new InvalidOperationException("Can't get the disc layout for a location without a file");

        DiscImage iso;

        try
        {
            // Read the iso disc image
            using RCPContext context = new(location.Directory);
            context.AddSettings(new ISO9660Settings(isRawData: false));
            iso = context.ReadRequiredFileData<DiscImage>(location.FileName);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Reading disc image");
            return null;
        }

        // Return first matching layout
        return Layouts.OfType<Ps2DiscProgramLayout>().FirstOrDefault(x =>
            x.FileSystem.IsLocationValid(new ISO9660FileSystemSource(iso), false).IsValid);
    }
}