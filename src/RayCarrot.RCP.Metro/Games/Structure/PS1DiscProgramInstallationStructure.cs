using BinarySerializer.Disk.Cue;

namespace RayCarrot.RCP.Metro.Games.Structure;

public class PS1DiscProgramInstallationStructure : SingleFileProgramInstallationStructure
{
    public PS1DiscProgramInstallationStructure(Ps1DiscProgramLayout[] layouts) : base(layouts)
    {
        Layouts = layouts;
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public new Ps1DiscProgramLayout[] Layouts { get; }

    public override bool SupportGameFileFinder => true;

    public override FileExtension[] SupportedFileExtensions => new[]
    {
        new FileExtension(".cue"),
    };

    // TODO-UPDATE: Cache the current layout. Store in game installation with id? Avoid calling this after added.
    public Ps1DiscProgramLayout? GetLayout(InstallLocation location)
    {
        if (!location.HasFile)
            throw new InvalidOperationException("Can't get the disc layout for a location without a file");

        CueSheet cue;
        PS1ISO iso;

        try
        {
            // Read the cue sheet
            cue = CueSheet.FromFile(location.FilePath);

            // Verify tracks count so we don't unnecessarily have to read the data file
            if (Layouts.All(x => x.TracksCount != cue.Tracks.Count))
                return null;

            // Get the file name for the data track
            string? dataTrackFileName = cue.Tracks[0].DataFile?.Filename;

            if (dataTrackFileName == null) 
                return null;

            // Read the data track
            using RCPContext context = new(location.Directory);
            iso = context.ReadRequiredFileData<PS1ISO>(dataTrackFileName);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Reading disc bin");
            return null;
        }

        // Return first matching layout
        return Layouts.FirstOrDefault(x => 
            cue.Tracks.Count == x.TracksCount && 
            x.FileSystem.IsLocationValid(new ISO9960FileSystemSource(iso), false).IsValid);
    }

    public override GameLocationValidationResult IsLocationValid(InstallLocation location)
    {
        if (!location.HasFile || !location.FilePath.FileExists)
            return new GameLocationValidationResult(false, Resources.Games_ValidationFileMissing);

        bool isValid = GetLayout(location) != null;

        return new GameLocationValidationResult(isValid, Resources.Games_ValidationRomInvalid);
    }
}