using System;
using System.IO;

namespace RayCarrot.RCP.Metro.Archive;

public interface IPatchDataSource : IDisposable
{
    Stream GetResource(string resourceName, bool isNormalized);
    Stream GetAsset(string assetName);
}