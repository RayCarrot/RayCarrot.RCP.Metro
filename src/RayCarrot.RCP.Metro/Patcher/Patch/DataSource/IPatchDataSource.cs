using System;
using System.IO;

namespace RayCarrot.RCP.Metro.Patcher;

public interface IPatchDataSource : IDisposable
{
    Stream GetResource(string resourceName, bool isNormalized);
    Stream GetAsset(string assetName);
}