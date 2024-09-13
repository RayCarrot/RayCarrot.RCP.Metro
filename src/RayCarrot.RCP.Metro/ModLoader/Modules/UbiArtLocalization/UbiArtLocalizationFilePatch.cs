using System.IO;
using BinarySerializer;
using BinarySerializer.UbiArt;

namespace RayCarrot.RCP.Metro.ModLoader.Modules.UbiArtLocalization;

public class UbiArtLocalizationFilePatch<UAString> : IFilePatch
    where UAString : UbiArtString, new()
{
    public UbiArtLocalizationFilePatch(GameInstallation gameInstallation, ModFilePath path, IReadOnlyCollection<LocaleFile> localeFiles)
    {
        GameInstallation = gameInstallation;
        Path = path;
        LocaleFiles = localeFiles;
    }

    public GameInstallation GameInstallation { get; }
    public ModFilePath Path { get; }
    public IReadOnlyCollection<LocaleFile> LocaleFiles { get; }

    public void PatchFile(Stream stream)
    {
        using Context context = new RCPContext(String.Empty);
        context.Initialize(GameInstallation);

        Localisation_Template<UAString> loc = context.ReadStreamData<Localisation_Template<UAString>>(stream, name: Path.FilePath, endian: Endian.Big, mode: VirtualFileMode.DoNotClose);

        foreach (LocaleFile localeFile in LocaleFiles)
        {
            List<UbiArtKeyObjValuePair<int, UAString>>? stringTable = loc.Strings.FirstOrDefault(x => x.Key == localeFile.Id)?.Value.ToList();

            if (stringTable == null)
                continue;

            string[] lines = File.ReadAllLines(localeFile.FilePath);

            foreach (string line in lines)
            {
                if (line.IsNullOrWhiteSpace())
                    continue;

                int separatorIndex = line.IndexOf('=');

                if (separatorIndex == -1)
                    continue;

                string locIdString = line.Substring(0, separatorIndex);
                if (!Int32.TryParse(locIdString, out int locId))
                    continue;

                string locValue = line.Substring(separatorIndex + 1);

                if (stringTable.FirstOrDefault(x => x.Key == locId) is { } pair)
                {
                    pair.Value = new UAString { Value = locValue };
                }
                else
                {
                    stringTable.Add(new UbiArtKeyObjValuePair<int, UAString>()
                    {
                        Key = locId,
                        Value = new UAString { Value = locValue },
                    });
                }
            }

            loc.Strings.First(x => x.Key == localeFile.Id).Value = stringTable.ToArray();
        }

        context.WriteStreamData(stream, loc, name: Path.FilePath, endian: Endian.Big, mode: VirtualFileMode.DoNotClose);
        stream.TrimEnd();
    }
}