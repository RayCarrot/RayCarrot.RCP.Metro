using System;
using System.Collections.Generic;
using BinarySerializer;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro;

public class SerializableProgressionSlotViewModel<FileObj> : ProgressionSlotViewModel
    where FileObj : BinarySerializable, new()
{
    public SerializableProgressionSlotViewModel(ProgressionGameViewModel game, LocalizedString? name, int index, int collectiblesCount, int totalCollectiblesCount, IEnumerable<ProgressionDataViewModel> dataItems, Context context, FileObj serializable, string fileName) : base(game, name, index, collectiblesCount, totalCollectiblesCount, dataItems)
    {
        Context = context;
        Serializable = serializable;
        FileName = fileName;
        FilePath = Context.GetAbsoluteFilePath(fileName);

        CanExport = true;
        CanImport = true;
    }

    public SerializableProgressionSlotViewModel(ProgressionGameViewModel game, LocalizedString? name, int index, double percentage, IEnumerable<ProgressionDataViewModel> dataItems, Context context, FileObj serializable, string fileName) : base(game, name, index, percentage, dataItems)
    {
        Context = context;
        Serializable = serializable;
        FileName = fileName;
        FilePath = Context.GetAbsoluteFilePath(fileName);

        CanExport = true;
        CanImport = true;
    }

    public Context Context { get; }
    public FileObj Serializable { get; }
    public string FileName { get; }

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

        // TODO-UPDATE: Keep copy of original file in case of error
        using (Context)
            FileFactory.Write<FileObj>(FileName, data, Context);
    }
}