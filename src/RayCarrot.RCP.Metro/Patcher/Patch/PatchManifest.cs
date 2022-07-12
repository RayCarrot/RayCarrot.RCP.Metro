using System;
using System.Linq;
using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.Patcher;

public record PatchManifest(
    [property: JsonProperty(Required = Required.Always)]
    string ID,

    // The version of the patch file format. This is different from revision which is the revision of the user-created patch.
    [property: JsonProperty(Required = Required.Always)]
    int PatchVersion,

    [property: JsonProperty(Required = Required.Always)]
    Games Game,

    string? Name, 
    string? Description, 
    string? Author, 

    long TotalSize, 
    DateTime ModifiedDate, 

    int Revision,

    PatchFilePath[]? AddedFiles,
    string[]? AddedFileChecksums,
    PatchFilePath[]? RemovedFiles,

    // Optional asset files, such as a thumbnail
    string[]? Assets) 
{
    public bool HasAsset(string assetName) => Assets?.Contains(assetName) == true;
}