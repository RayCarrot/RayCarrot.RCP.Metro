﻿using System.IO;

namespace RayCarrot.RCP.Metro.ModLoader.Resource;

public class VirtualModFileResource : IModFileResource
{
    public VirtualModFileResource(ModFilePath path, Stream stream)
    {
        Path = path;
        Stream = stream;
    }

    public ModFilePath Path { get; }

    public Stream Stream { get; }

    public void CopyToStream(Stream destinationStream)
    {
        // We always want to copy from the start, so attempt to seek to the beginning of the stream
        if (Stream.CanSeek)
            Stream.Position = 0;

        Stream.CopyToEx(destinationStream);
    }
}