using System.IO;
using System.Text;
using BinarySerializer;

namespace RayCarrot.RCP.Metro.Games.Structure;

public class GameCubeDiscProgramInstallationStructure : SingleFileProgramInstallationStructure
{
    public GameCubeDiscProgramInstallationStructure(GameCubeProgramLayout[] layouts) : base(layouts) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override bool SupportGameFileFinder => true;
    public override FileExtension[] SupportedFileExtensions => new[]
    {
        new FileExtension(".iso"),
        new FileExtension(".gcm"),
        new FileExtension(".ciso"),
        new FileExtension(".rvz"),
    };

    public override ProgramLayout? FindMatchingLayout(InstallLocation location)
    {
        if (!location.HasFile)
            throw new InvalidOperationException("Can't get the disc layout for a location without a file");

        try
        {
            // Documentation/references
            // https://www.gc-forever.com/yagcd/chap13.html#sec13
            // https://github.com/Julgodis/picori/blob/master/src/ciso.rs
            // https://github.com/dolphin-emu/dolphin/blob/master/docs/WiaAndRvz.md#rvz-file-format-description

            using FileStream stream = File.OpenRead(location.FilePath);
            using Reader reader = new(stream, isLittleEndian: false);

            Encoding encoding = Encoding.ASCII;

            // Skip the block map for compact ISO files
            if (location.FilePath.FileExtension == new FileExtension(".ciso"))
                stream.Position = 0x8000;
            // Skip the custom disc header for RVZ files
            else if (location.FilePath.FileExtension == new FileExtension(".rvz"))
                stream.Position = 0x58;

            long headerBasePosition = stream.Position;

            // Read the game code and maker code
            string gameCode = reader.ReadString(4, encoding);
            string makerCode = reader.ReadString(4, encoding);

            // Validate the format
            stream.Position = headerBasePosition + 28;
            if (reader.ReadUInt32() != 0xc2339f3d)
                return null;

            string gameTitle = reader.ReadString(992, encoding);

            // Return the first matching layout
            return Layouts.OfType<GameCubeProgramLayout>().FirstOrDefault(x => 
                x.GameCode == gameCode && x.MakerCode == makerCode && x.GameTitle == gameTitle);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Reading GameCube disc image");
            return null;
        }
    }
}