using BinarySerializer;
using BinarySerializer.Nintendo.GB;

namespace RayCarrot.RCP.Metro.Games.Structure;

public class GbcRomProgramInstallationStructure : RomProgramInstallationStructure
{
    public GbcRomProgramInstallationStructure(GbcProgramLayout[] layouts) : base(layouts) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override FileExtension[] SupportedFileExtensions => new[]
    {
        new FileExtension(".gbc"),
        new FileExtension(".cgb"),
    };

    protected override ProgramLayout? FindMatchingLayout(InstallLocation location)
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
            Logger.Error(ex, "Reading GBC ROM header");
            return null;
        }

        return Layouts.OfType<GbcProgramLayout>().FirstOrDefault(x => x.GameTitle == romHeader.GameTitle && 
                                                                      x.ManufacturerCode == romHeader.ManufacturerCode && 
                                                                      x.LicenseeCode == romHeader.NewLicenseeCode);
    }

    public override GameLocationValidationResult IsLocationValid(InstallLocation location)
    {
        if (!location.HasFile ||!location.FilePath.FileExists)
            return new GameLocationValidationResult(false, Resources.Games_ValidationFileMissing);

        bool isValid = FindMatchingLayout(location) != null;

        return new GameLocationValidationResult(isValid, Resources.Games_ValidationRomInvalid);
    }
}