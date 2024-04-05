﻿using BinarySerializer;
using BinarySerializer.UbiArt;
using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro.Archive.UbiArt;

public class UbiArtGlobalFatManager
{
    public UbiArtGlobalFatManager(GameInstallation gameInstallation, string gameDataDir, string fileName)
    {
        GameInstallation = gameInstallation;
        GameDataDir = gameDataDir;
        FileName = fileName;
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public GameInstallation GameInstallation { get; }
    public string GameDataDir { get; }
    public string FileName { get; }

    public void CreateFileAllocationTable(string[] bundleNames, CancellationToken cancellationToken, Action<Progress>? progressCallback = null)
    {
        if (bundleNames.Length > Byte.MaxValue)
            throw new ArgumentException("Too many bundles", nameof(bundleNames));

        Logger.Info("Creating file allocation table for {0} bundles", bundleNames.Length);

        using Context context = new RCPContext(GameInstallation.InstallLocation.Directory + GameDataDir);
        GameInstallation.GetComponents<InitializeContextComponent>().InvokeAll(context);
        
        UbiArtSettings settings = context.GetRequiredSettings<UbiArtSettings>();
        string platformString = settings.PlatformString;

        // Lookup tables
        Dictionary<string, FolderDescriptor> foldersLookup = new();
        Dictionary<uint, FileDescriptor> filesLookup = new();
        Dictionary<FileDescriptor, HashSet<byte>> fileBundles = new();

        // Data for the file table
        List<BundleDescriptor> bundles = new();
        List<FolderDescriptor> folders = new();
        List<FileDescriptor> files = new();
        List<FileAdditionalDescriptor> filesAdditional = new();

        progressCallback?.Invoke(Progress.Empty);

        // Read files from each bundle
        for (byte bundleId = 0; bundleId < bundleNames.Length; bundleId++)
        {
            string bundleName = bundleNames[bundleId];

            cancellationToken.ThrowIfCancellationRequested();

            // Read the bundle
            BundleFile bundle = context.ReadRequiredFileData<BundleFile>($"{bundleName}_{platformString}.ipk", endian: settings.Endian);

            // Create a descriptor
            BundleDescriptor bundleDescriptor = new()
            {
                Id = bundleId,
                Name = bundleName.ToLowerInvariant()
            };
            bundles.Add(bundleDescriptor);

            // Process every file
            foreach (BundleFile_FileEntry fileEntry in bundle.FilePack.Files)
            {
                Path path = fileEntry.Path;

                if (!filesLookup.TryGetValue(path.StringID.ID, out FileDescriptor fileDescriptor))
                {
                    if (!foldersLookup.TryGetValue(path.DirectoryPath, out FolderDescriptor folderDescriptor))
                    {
                        folderDescriptor = new FolderDescriptor()
                        {
                            Id = (ushort)folders.Count,
                            Path = path.DirectoryPath
                        };
                        folders.Add(folderDescriptor);
                        foldersLookup[path.DirectoryPath] = folderDescriptor;
                    }

                    fileDescriptor = new FileDescriptor()
                    {
                        Id = path.StringID,
                        FileName = path.FileName,
                        Folder = folderDescriptor.Id
                    };
                    files.Add(fileDescriptor);
                    filesAdditional.Add(new FileAdditionalDescriptor
                    {
                        Id = fileDescriptor.Id,
                        Folder = fileDescriptor.Folder,
                        FileName = fileDescriptor.FileName
                    });
                    filesLookup[path.StringID.ID] = fileDescriptor;
                    fileBundles[fileDescriptor] = new HashSet<byte>();
                }

                fileBundles[fileDescriptor].Add(bundleId);
            }

            progressCallback?.Invoke(new Progress((bundleId + 1) / (double)bundleNames.Length, 2));

            Logger.Info("Processed bundle {0} with {1} files", bundleName, bundle.FilePack.Files.Length);
        }

        // Link files to bundles
        foreach (var file in fileBundles)
            file.Key.Bundles = file.Value.ToArray();

        // Create the file allocation table
        GlobalFat fat = new()
        {
            Bundles = bundles.ToArray(),
            Files = files.OrderBy(x => x.Id.ID).ToArray(),
            Folders = folders.ToArray(),
            FilesAdditional = filesAdditional.ToArray(),
        };

        cancellationToken.ThrowIfCancellationRequested();

        // Write the file
        context.WriteFileData(FileName, fat, endian: settings.Endian);

        Logger.Info("Created file allocation table");

        progressCallback?.Invoke(Progress.Complete);
    }
}