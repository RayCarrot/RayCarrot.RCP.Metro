using System;
using System.Collections.Generic;
using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public class SerializableGameProgressionSlot<FileObj> : GameProgressionSlot
    where FileObj : BinarySerializable, new()
{
    public SerializableGameProgressionSlot(
        LocalizedString? name, 
        int index, 
        int collectiblesCount, 
        int totalCollectiblesCount, 
        IReadOnlyList<GameProgressionDataItem> dataItems, 
        Context context, 
        FileObj serializable, 
        string fileName,
        bool canExport = true,
        bool canImport = true) 
        : base(name, index, collectiblesCount, totalCollectiblesCount, context.GetAbsoluteFilePath(fileName), dataItems)
    {
        Context = context;
        Serializable = serializable;
        FileName = fileName;
        CanExport = canExport;
        CanImport = canImport;
    }

    public SerializableGameProgressionSlot(
        LocalizedString? name, 
        int index, 
        double percentage, 
        IReadOnlyList<GameProgressionDataItem> dataItems, 
        Context context, 
        FileObj serializable, 
        string fileName,
        bool canExport = true,
        bool canImport = true) 
        : base(name, index, percentage, context.GetAbsoluteFilePath(fileName), dataItems)
    {
        Context = context;
        Serializable = serializable;
        FileName = fileName;
        CanExport = canExport;
        CanImport = canImport;
    }

    public override bool CanExport { get; }
    public override bool CanImport { get; }

    public Context Context { get; }
    public FileObj Serializable { get; }
    public string FileName { get; }

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

        // TODO: Find solution
        // Commented out first writing to temp due to the file moving at the end not keeping file attributes which causes
        // saves for packages games not to load (for example Jungle Run)

        // Write to a temp file first to avoid corrupting the file in case there is an error during the serialization
        //using var tempFile = new TempFile(false);

        //var file = (PhysicalFile)Context.GetFile(FileName);
        //file.DestinationPath = tempFile.TempPath;

        using (Context)
            FileFactory.Write<FileObj>(Context, FileName, data);

        // Replace the original file with the temp file
        //Services.File.MoveFile(tempFile.TempPath, file.SourcePath, true);
    }
}