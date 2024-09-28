using BinarySerializer;
using BinarySerializer.UbiArt;

namespace RayCarrot.RCP.Metro.Pages.Utilities;

public class ConvertersUtilityUbiArtLocTypeViewModel : ConvertersUtilityTypeViewModel
{
    public ConvertersUtilityUbiArtLocTypeViewModel(LocalizedString name, ObservableCollection<SerializableUtilityTypeModeViewModel> modes) : base(name, modes) { }

    public override FileExtension SourceFileExtension => ((UbiArtSettings)SelectedMode.GetSettings()!).Game switch
    {
        BinarySerializer.UbiArt.Game.RaymanOrigins or BinarySerializer.UbiArt.Game.RaymanFiestaRun => new FileExtension(".loc"),
        _ => new FileExtension(".loc8"),
    };

    public override string[] ConvertFormats => new[] { ".json" };

    public override void Convert(Context context, string inputFileName, FileSystemPath outputFilePath)
    {
        UbiArtSettings settings = context.GetRequiredSettings<UbiArtSettings>();

        if (settings.Game == BinarySerializer.UbiArt.Game.RaymanFiestaRun)
            defaultConvert<UbiArtObjKeyObjValuePair<String16, String16>, String16>();
        else if (settings.Game == BinarySerializer.UbiArt.Game.RaymanOrigins)
            defaultConvert<String16, String16>();
        else
            defaultConvert<String8, String8>();

        void defaultConvert<LocString, UAString>()
            where LocString : BinarySerializable, new()
            where UAString : UbiArtString, new()
        {
            Localisation_Template<LocString, UAString>? obj = ReadFile<Localisation_Template<LocString, UAString>>(context, settings.Endian, inputFileName);

            if (obj is null)
                return;

            ConvertedLocData<LocString, UAString> jsonObj = new()
            {
                Strings = obj.Strings.ToDictionary(x => x.Key, x => x.Value.ToDictionary(y => y.Key, y => y.Value)),
                Audio = obj.Audio.ToDictionary(x => x.Key, x => x.Value),
                Paths = obj.Paths,
                Unknown = obj.Unknown,
            };

            JsonHelpers.SerializeToFile(jsonObj, outputFilePath);
        }
    }

    public override void ConvertBack(Context context, FileSystemPath inputFilePath, string outputFileName, object state)
    {
        UbiArtSettings settings = context.GetRequiredSettings<UbiArtSettings>();

        if (settings.Game == BinarySerializer.UbiArt.Game.RaymanFiestaRun)
            defaultConvert<UbiArtObjKeyObjValuePair<String16, String16>, String16>();
        else if (settings.Game == BinarySerializer.UbiArt.Game.RaymanOrigins)
            defaultConvert<String16, String16>();
        else
            defaultConvert<String8, String8>();

        void defaultConvert<LocString, UAString>()
            where LocString : BinarySerializable, new()
            where UAString : UbiArtString, new()
        {
            ConvertedLocData<LocString, UAString> obj = JsonHelpers.DeserializeFromFile<ConvertedLocData<LocString, UAString>>(inputFilePath);

            WriteFile(context, settings.Endian, outputFileName, new Localisation_Template<LocString, UAString>
            {
                Strings = obj.Strings?.Select(x => new LocTextValuePair<LocString>
                {
                    Key = x.Key,
                    Value = x.Value.Select(y => new UbiArtKeyObjValuePair<int, LocString>
                    {
                        Key = y.Key,
                        Value = y.Value
                    }).ToArray()
                }).ToArray(),
                Audio = obj.Audio?.Select(y => new UbiArtKeyObjValuePair<int, LocAudio<UAString>>
                {
                    Key = y.Key,
                    Value = y.Value
                }).ToArray(),
                Paths = obj.Paths,
                Unknown = obj.Unknown,
            });
        }
    }

    private class ConvertedLocData<LocString, UAString>
        where LocString : BinarySerializable, new()
        where UAString : UbiArtString, new()
    {
        public Dictionary<int, Dictionary<int, LocString>>? Strings { get; init; }
        public Dictionary<int, LocAudio<UAString>>? Audio { get; init; }
        public UAString[]? Paths { get; init; }
        public uint[]? Unknown { get; init; }
    }
}