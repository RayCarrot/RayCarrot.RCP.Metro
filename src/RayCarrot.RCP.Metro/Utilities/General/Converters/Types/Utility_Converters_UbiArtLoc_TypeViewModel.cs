using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BinarySerializer;
using BinarySerializer.UbiArt;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro;

public class Utility_Converters_UbiArtLoc_TypeViewModel : Utility_Converters_TypeViewModel
{
    public Utility_Converters_UbiArtLoc_TypeViewModel(LocalizedString name, ObservableCollection<Utility_SerializableTypeModeViewModel> modes) : base(name, modes) { }

    public override FileExtension SourceFileExtension => ((UbiArtSettings)SelectedMode.Data.GetSettings!()).EngineVersion switch
    {
        EngineVersion.RaymanOrigins or EngineVersion.RaymanFiestaRun => new FileExtension(".loc"),
        _ => new FileExtension(".loc8"),
    };

    public override string[] ConvertFormats => new[] { ".json" };

    public override void Convert(Context context, string inputFileName, FileSystemPath outputFilePath)
    {
        UbiArtSettings settings = context.GetSettings<UbiArtSettings>();

        if (settings.EngineVersion == EngineVersion.RaymanFiestaRun)
            defaultConvert<UbiArtObjKeyObjValuePair<String16, String16>, String16>();
        else if (settings.EngineVersion == EngineVersion.RaymanOrigins)
            defaultConvert<String16, String16>();
        else
            defaultConvert<String8, String8>();

        void defaultConvert<LocString, UAString>()
            where LocString : BinarySerializable, new()
            where UAString : UbiArtString, new()
        {
            LocalizationData<LocString, UAString>? obj = ReadFile<LocalizationData<LocString, UAString>>(context, inputFileName);

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

    public override void ConvertBack(Context context, FileSystemPath inputFilePath, string outputFileName)
    {
        UbiArtSettings settings = context.GetSettings<UbiArtSettings>();

        if (settings.EngineVersion == EngineVersion.RaymanFiestaRun)
            defaultConvert<UbiArtObjKeyObjValuePair<String16, String16>, String16>();
        else if (settings.EngineVersion == EngineVersion.RaymanOrigins)
            defaultConvert<String16, String16>();
        else
            defaultConvert<String8, String8>();

        void defaultConvert<LocString, UAString>()
            where LocString : BinarySerializable, new()
            where UAString : UbiArtString, new()
        {
            ConvertedLocData<LocString, UAString> obj = JsonHelpers.DeserializeFromFile<ConvertedLocData<LocString, UAString>>(inputFilePath);

            WriteFile(context, outputFileName, new LocalizationData<LocString, UAString>
            {
                Strings = obj.Strings?.Select(x => new LocStringValuePair<LocString>
                {
                    Key = x.Key,
                    Value = x.Value.Select(y => new UbiArtKeyObjValuePair<int, LocString>
                    {
                        Key = y.Key,
                        Value = y.Value
                    }).ToArray()
                }).ToArray(),
                Audio = obj.Audio?.Select(y => new UbiArtKeyObjValuePair<int, LocalizationAudio<UAString>>
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
        public Dictionary<int, LocalizationAudio<UAString>>? Audio { get; init; }
        public UAString[]? Paths { get; init; }
        public uint[]? Unknown { get; init; }
    }
}