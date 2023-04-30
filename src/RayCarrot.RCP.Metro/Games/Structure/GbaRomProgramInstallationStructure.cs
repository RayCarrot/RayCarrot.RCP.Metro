using BinarySerializer;
using BinarySerializer.Nintendo.GBA;

namespace RayCarrot.RCP.Metro.Games.Structure;

public class GbaRomProgramInstallationStructure : RomProgramInstallationStructure
{
    public GbaRomProgramInstallationStructure(GbaRomLayout[] romLayouts) : base(romLayouts)
    {
        RomLayouts = romLayouts;
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public new GbaRomLayout[] RomLayouts { get; }

    public override FileExtension[] SupportedFileExtensions => new[]
    {
        new FileExtension(".gba"),
        new FileExtension(".agb"),
    };

    private GbaRomLayout? GetLayout(InstallLocation location)
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

        return RomLayouts.FirstOrDefault(x => x.GameTitle == romHeader.GameTitle && 
                                              x.GameCode == romHeader.GameCode && 
                                              x.MakerCode == romHeader.MakerCode);
    }

    public override GameLocationValidationResult IsLocationValid(InstallLocation location)
    {
        if (!location.HasFile ||!location.FilePath.FileExists)
            return new GameLocationValidationResult(false, Resources.Games_ValidationFileMissing);

        bool isValid = GetLayout(location) != null;

        return new GameLocationValidationResult(isValid, Resources.Games_ValidationRomInvalid);
    }
}