using System.IO;

namespace RayCarrot.RCP.Metro.Games.Components;

public class BigPEmuEmulatedSaveFilesComponent : EmulatedSaveFilesComponent
{
    public BigPEmuEmulatedSaveFilesComponent() : base(GetEmulatedSaveFiles) { }

    private static ulong CalculateFnv1a64Hash(ReadOnlySpan<byte> buffer)
    {
        const ulong prime = 0x100000001B3;

        ulong hash = 0xCBF29CE484222325;

        foreach (byte b in buffer)
        {
            hash ^= b;
            hash *= prime;
        }
        
        return hash;
    }

    public static IEnumerable<EmulatedSaveFile> GetEmulatedSaveFiles(GameInstallation gameInstallation)
    {
        // Calculate the ROM hash
        byte[] rom = File.ReadAllBytes(gameInstallation.InstallLocation.FilePath);
        ulong hash = CalculateFnv1a64Hash(rom);

        // Get the path
        string fileName = $"game{hash:X16}_eeprom.bigpeep";
        FileSystemPath saveFilePath = Environment.SpecialFolder.ApplicationData.GetFolderPath() + "BigPEmu" + fileName;

        if (saveFilePath.FileExists)
            yield return new EmulatedJaguarSaveFile(saveFilePath);
    }
}