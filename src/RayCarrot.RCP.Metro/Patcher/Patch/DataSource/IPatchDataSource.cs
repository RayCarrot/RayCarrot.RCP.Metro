using System;
using System.IO;

namespace RayCarrot.RCP.Metro.Patcher;

public interface IPatchDataSource : IDisposable
{
    Stream GetResource(PatchFilePath resourcePath);
    Stream GetAsset(string assetName);
}