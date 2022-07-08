using System.IO;

namespace RayCarrot.RCP.Metro.Patcher;

public class PatchFileDataSource : IPatchDataSource
{
    public PatchFileDataSource(PatchFile patchFile, bool leaveOpen)
    {
        PatchFile = patchFile;
        _leaveOpen = leaveOpen;
    }

    private readonly bool _leaveOpen;

    public PatchFile PatchFile { get; }

    public Stream GetResource(string resourceName, bool isNormalized) => PatchFile.GetPatchResource(resourceName, isNormalized);
    public Stream GetAsset(string assetName) => PatchFile.GetPatchAsset(assetName);

    public void Dispose()
    {
        if (!_leaveOpen)
            PatchFile.Dispose();
    }
}