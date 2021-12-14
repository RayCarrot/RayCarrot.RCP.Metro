using System;
using System.Collections.Generic;
using System.IO;
using RayCarrot.Binary;
using RayCarrot.IO;
using RayCarrot.Rayman;

namespace RayCarrot.RCP.Metro;

public class SerializableProgressionSlotViewModel<FileObj> : ProgressionSlotViewModel
    where FileObj : IBinarySerializable, new()
{
    public SerializableProgressionSlotViewModel(ProgressionGameViewModel game, LocalizedString? name, int index, int collectiblesCount, int totalCollectiblesCount, IEnumerable<ProgressionDataViewModel> dataItems, FileObj serializable, IBinarySerializerSettings settings) : base(game, name, index, collectiblesCount, totalCollectiblesCount, dataItems)
    {
        Serializable = serializable;
        Settings = settings;

        CanExport = true;
        CanImport = true;
    }

    public SerializableProgressionSlotViewModel(ProgressionGameViewModel game, LocalizedString? name, int index, double percentage, IEnumerable<ProgressionDataViewModel> dataItems, FileObj serializable, IBinarySerializerSettings settings) : base(game, name, index, percentage, dataItems)
    {
        Serializable = serializable;
        Settings = settings;

        CanExport = true;
        CanImport = true;
    }

    public FileObj Serializable { get; }
    public IBinarySerializerSettings Settings { get; }
    public IDataEncoder? ImportEncoder { get; init; }

    public Func<FileObj, object>? GetExportObject { get; init; }
    public Action<FileObj, object>? SetImportObject { get; init; }
    public Type? ExportedType { get; init; }

    protected override void ExportSlot(FileSystemPath filePath)
    {
        bool customObj = GetExportObject is not null && ExportedType is not null;
        object obj = customObj ? GetExportObject!(Serializable) : Serializable;

        // Export the data
        JsonHelpers.SerializeToFile(obj, filePath);
    }

    protected override void ImportSlot(FileSystemPath filePath)
    {
        bool customObj = SetImportObject is not null && ExportedType is not null;

        FileObj data;

        if (customObj)
        {
            object fileData = JsonHelpers.DeserializeFromFile(filePath, ExportedType);
            SetImportObject!(Serializable, fileData);
            data = Serializable;
        }
        else
        {
            data = JsonHelpers.DeserializeFromFile<FileObj>(filePath);
        }

        // TODO-UPDATE: Rather than OpenWrite we backup file to temp, delete and restore if error - look at WriteToFile as well
        using Stream serializeStream = ImportEncoder == null ? File.OpenWrite(FilePath) : new MemoryStream();

        // Import the data
        BinarySerializableHelpers.WriteToStream(data, serializeStream, Settings, Services.App.GetBinarySerializerLogger(FilePath.Name));

        // If there is an encoder we need to encode the data
        if (ImportEncoder != null)
        {
            serializeStream.Position = 0;
            using Stream fileStream = File.OpenWrite(FilePath);
            ImportEncoder.Encode(serializeStream, fileStream);
        }
    }
}