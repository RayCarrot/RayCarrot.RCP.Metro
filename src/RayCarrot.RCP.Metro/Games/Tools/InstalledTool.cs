using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RayCarrot.RCP.Metro.Games.Tools;

public class InstalledTool
{
    public InstalledTool(string toolId, FileSystemPath path, long size, DateTime downloadDateTime, Version version)
    {
        ToolId = toolId;
        Path = path;
        Size = size;
        DownloadDateTime = downloadDateTime;
        Version = version;
    }

    public string ToolId { get; }
    public FileSystemPath Path { get; }
    public long Size { get; }
    public DateTime DownloadDateTime { get; }

    [JsonConverter(typeof(VersionConverter))]
    public Version Version { get; }
}