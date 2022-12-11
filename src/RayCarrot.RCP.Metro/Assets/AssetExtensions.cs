using System.Reflection;

namespace RayCarrot.RCP.Metro;

public static class AssetExtensions
{
    public static string GetAssetPath(this Enum asset)
    {
        AssetDirectoryAttribute? dirAttr = asset.GetType().GetCustomAttribute<AssetDirectoryAttribute>();
        
        if (dirAttr == null)
            throw new ArgumentException("The enum type does not have an asset directory path associated with it", nameof(asset));

        string dirPath = asset.GetType().GetCustomAttribute<AssetDirectoryAttribute>().DirectoryPath;
        string fileName = asset.GetAttribute<AssetFileNameAttribute>()?.FileName 
                          ?? dirAttr.DefaultFileName 
                          ?? $"{asset}{dirAttr.DefaultFileExtension}";

        return $"{dirPath}/{fileName}";
    }
}