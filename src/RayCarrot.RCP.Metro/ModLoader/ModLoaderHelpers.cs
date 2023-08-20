namespace RayCarrot.RCP.Metro.ModLoader;

public static class ModLoaderHelpers
{
    public static string NormalizePath(string path)
    {
        return path.ToLowerInvariant().Replace('\\', '/');
    }
}