using System.IO;
using System.IO.Compression;
using BinarySerializer;

namespace RayCarrot.RCP.Metro.Patcher;

/// <summary>
/// Extension methods for <see cref="IPackageFile"/>
/// </summary>
public static class PackageFileExtensions
{
    /// <summary>
    /// Writes the package file and packs its resources
    /// </summary>
    /// <typeparam name="T">The package file type</typeparam>
    /// <param name="package">The package file</param>
    /// <param name="progressCallback">An optional progress callback</param>
    /// <param name="cancellationToken">An optional cancellation token</param>
    public static void WriteAndPackResources<T>(this T package, Action<Progress>? progressCallback = null, CancellationToken cancellationToken = default)
        where T : BinarySerializable, IPackageFile, new()
    {
        if (package.Offset == null)
            throw new InvalidOperationException("Can't repack an uninitialized file");

        if (package.Offset.File is not PhysicalFile p)
            throw new InvalidOperationException($"Can't repack if the file is not of type {nameof(PhysicalFile)}");

        if (package.Offset.FileOffset != 0)
            throw new InvalidOperationException("Can't repack if the file offset is not 0");

        // Calculate the header size to get the start offset for packing the resources
        package.RecalculateSize();
        long dataOffset = package.SerializedSize;

        // Create a temporary file to write to
        using TempFile tempFile = new(true);

        // Set the temp file as the destination
        string prevDestination = p.DestinationPath;
        p.DestinationPath = tempFile.TempPath;

        // Do not recreate on write as we will be first writing the resources and then the header
        bool prevRecreateOnWrite = p.RecreateOnWrite;
        p.RecreateOnWrite = false;

        try
        {
            // TODO: Potentially find a better solution to avoid opening the stream twice

            // Pack the files. We leave the header empty for now.
            using (Stream outputStream = File.OpenWrite(tempFile.TempPath))
            {
                outputStream.Position = dataOffset;

                PackagedResourceEntry[] resources = package.Resources.ToArray();

                int resourceIndex = 0;

                foreach (PackagedResourceEntry file in resources)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    progressCallback?.Invoke(new Progress(resourceIndex, resources.Length));
                    resourceIndex++;

                    // Get the stream to pack
                    (Stream? resourceStream, bool isCompressed) = file.GetPendingImport();

                    try
                    {
                        // Fall back to getting the original resource data
                        if (resourceStream == null)
                        {
                            resourceStream = file.ReadData(package.Context, false);
                            isCompressed = true;
                        }

                        // Compress the file if not already compressed
                        if (isCompressed)
                        {
                            // Copy to output directly
                            resourceStream.CopyTo(outputStream);
                        }
                        else
                        {
                            // Compress to the output
                            using DeflateStream deflateStream = new(outputStream, CompressionLevel.Fastest, leaveOpen: true);
                            resourceStream.CopyTo(deflateStream);
                            file.DecompressedDataLength = resourceStream.Length;
                        }

                        // Get the compressed data length
                        long compressedLength = outputStream.Position - dataOffset;

                        // Set the resource entry values
                        file.DataOffset = dataOffset;
                        file.CompressedDataLength = compressedLength;

                        // Update the data offset for the next resource
                        dataOffset += compressedLength;
                    }
                    finally
                    {
                        resourceStream?.Dispose();
                    }
                }

                progressCallback?.Invoke(new Progress(resourceIndex, resources.Length));
            }

            // Write the header
            FileFactory.Write<T>(package.Context, p.FilePath, package);

            // Dispose the file so we can move it from temp
            package.Context.Serializer.DisposeFile(p);

            string? parentDir = Path.GetDirectoryName(p.SourcePath);
            
            if (parentDir != null)
                Directory.CreateDirectory(parentDir);
            
            if (File.Exists(p.SourcePath))
                File.Delete(p.SourcePath);
            
            // Move the file from temp
            File.Move(tempFile.TempPath, p.SourcePath);
        }
        finally
        {
            p.DestinationPath = prevDestination;
            p.RecreateOnWrite = prevRecreateOnWrite;
        }
    }
}