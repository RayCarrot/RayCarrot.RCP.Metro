using BinarySerializer;
using BinarySerializer.Nintendo.GBA;

namespace RayCarrot.RCP.Metro.Games.Structure;

public class GbaRomProgramInstallationStructure : RomProgramInstallationStructure
{
    public GbaRomProgramInstallationStructure(GbaProgramLayout[] layouts) : base(layouts) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override FileExtension[] SupportedFileExtensions => new[]
    {
        new FileExtension(".gba"),
        new FileExtension(".agb"),
    };

    public override ProgramLayout? FindMatchingLayout(InstallLocation location)
    {
        if (!location.HasFile)
            throw new InvalidOperationException("Can't get the ROM layout for a location without a file");

        ROMHeader romHeader;

        try
        {
            using Context context = new RCPContext(location.Directory);
            romHeader = context.ReadRequiredFileData<ROMHeader>(location.FileName);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Reading GBA ROM header");
            return null;
        }

        return Layouts.OfType<GbaProgramLayout>().FirstOrDefault(x => x.GameTitle == romHeader.GameTitle && 
                                                                      x.GameCode == romHeader.GameCode && 
                                                                      x.MakerCode == romHeader.MakerCode);
    }
}