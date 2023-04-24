using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public class SerializabeEmulatedGameProgressionSlot<FileObj> : EmulatedGameProgressionSlot
    where FileObj : BinarySerializable, new()
{
    public SerializabeEmulatedGameProgressionSlot(
        LocalizedString? name, 
        int index, 
        int collectiblesCount, 
        int totalCollectiblesCount,
        EmulatedSave emulatedSave, 
        IReadOnlyList<GameProgressionDataItem> dataItems,
        FileObj serializable) 
        : base(name, index, collectiblesCount, totalCollectiblesCount, emulatedSave, dataItems)
    {
        Serializable = serializable;
    }

    public SerializabeEmulatedGameProgressionSlot(
        LocalizedString? name, 
        int index, 
        double percentage, 
        EmulatedSave emulatedSave, 
        IReadOnlyList<GameProgressionDataItem> dataItems,
        FileObj serializable) 
        : base(name, index, percentage, emulatedSave, dataItems)
    {
        Serializable = serializable;
    }

    public override bool CanExport => true;
    public override bool CanImport => true;

    public FileObj Serializable { get; }

    public Func<FileObj, object>? GetExportObject { get; init; }
    public Action<FileObj, object>? SetImportObject { get; init; }
    public Type? ExportedType { get; init; }

    public override void ExportSlot(FileSystemPath filePath)
    {
        bool customObj = GetExportObject is not null && ExportedType is not null;
        object obj = customObj ? GetExportObject!(Serializable) : Serializable;

        // Export the data
        JsonHelpers.SerializeToFile(obj, filePath);
    }

    public override void ImportSlot(FileSystemPath filePath)
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

        EmulatedSave.WriteAsync(data);
    }
}