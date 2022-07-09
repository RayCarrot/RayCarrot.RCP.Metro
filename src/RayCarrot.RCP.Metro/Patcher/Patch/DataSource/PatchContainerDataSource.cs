using System.IO;

namespace RayCarrot.RCP.Metro.Patcher;

public class PatchContainerDataSource : IPatchDataSource
{
    public PatchContainerDataSource(PatchContainerFile container, string patchId, bool leaveOpen)
    {
        Container = container;
        PatchID = patchId;
        _leaveOpen = leaveOpen;
    }

    private readonly bool _leaveOpen;

    public PatchContainerFile Container { get; }
    public string PatchID { get; }

    public Stream GetResource(PatchFilePath resourcePath) => Container.GetPatchResource(PatchID, resourcePath);
    public Stream GetAsset(string assetName) => Container.GetPatchAsset(PatchID, assetName);

    public void Dispose()
    {
        if (!_leaveOpen)
            Container.Dispose();
    }
}