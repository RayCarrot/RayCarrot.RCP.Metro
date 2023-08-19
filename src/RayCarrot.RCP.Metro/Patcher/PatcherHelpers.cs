namespace RayCarrot.RCP.Metro.Patcher;

public static class PatcherHelpers
{
    public static string NormalizePath(string path)
    {
        return path.ToLowerInvariant().Replace('\\', '/');
    }
}