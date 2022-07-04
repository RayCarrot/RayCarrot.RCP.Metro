using System.IO;

namespace RayCarrot.RCP.Metro.Archive;

public class PatchFileDataSource : IPatchDataSource
{
    public PatchFileDataSource(Patch patch, bool leaveOpen)
    {
        Patch = patch;
        _leaveOpen = leaveOpen;
    }

    private readonly bool _leaveOpen;

    public Patch Patch { get; }

    public Stream GetResource(string resourceName, bool isNormalized) => Patch.GetPatchResource(resourceName, isNormalized);
    public Stream GetAsset(string assetName) => Patch.GetPatchAsset(assetName);

    public void Dispose()
    {
        if (!_leaveOpen)
            Patch.Dispose();
    }
}