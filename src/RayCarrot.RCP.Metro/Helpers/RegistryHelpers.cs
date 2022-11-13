using Microsoft.Win32;
using System;
using System.Linq;
using System.Security;

namespace RayCarrot.RCP.Metro;

public static class RegistryHelpers
{
    /// <summary>
    /// Checks if the specified key path exists in the registry
    /// </summary>
    /// <param name="keyPath">The key path to check</param>
    /// <returns></returns>
    public static bool KeyExists(string? keyPath)
    {
        return KeyExists(keyPath, RegistryView.Default);
    }

    /// <summary>
    /// Checks if the specified key path exists in the registry
    /// </summary>
    /// <param name="keyPath">The key path to check</param>
    /// <param name="view">The view to use</param>
    /// <returns></returns>
    public static bool KeyExists(string? keyPath, RegistryView view)
    {
        if (keyPath == null)
            return false;

        try
        {
            using RegistryKey? key = GetKeyFromFullPath(keyPath, view);
            return key is not null;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (SecurityException)
        {
            return true;
        }
    }

    /// <summary>
    /// Checks if the specified value exists in the registry
    /// </summary>
    /// <param name="keyPath">The key path of the value</param>
    /// <param name="value">The value name</param>
    /// <returns></returns>
    public static bool ValueExists(string keyPath, string value)
    {
        if (ValueExists(keyPath, value, RegistryView.Registry32))
            return true;
        else if (Environment.Is64BitOperatingSystem && ValueExists(keyPath, value, RegistryView.Registry64))
            return true;
        else
            return false;
    }

    /// <summary>
    /// Checks if the specified value exists in the registry
    /// </summary>
    /// <param name="keyPath">The key path of the value</param>
    /// <param name="value">The value name</param>
    /// <param name="view">The view to use</param>
    /// <returns></returns>
    public static bool ValueExists(string keyPath, string value, RegistryView view)
    {
        try
        {
            using var key = GetKeyFromFullPath(keyPath, view);
            return key?.GetValueNames().Contains(value) ?? false;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    /// <summary>
    /// Normalizes the given registry path
    /// </summary>
    /// <param name="path">The path to normalize</param>
    /// <returns></returns>
    public static string NormalizePath(string path)
    {
        path = path.TrimEnd(KeySeparatorCharacter);

        if (path.StartsWith($"Computer{KeySeparatorCharacter}", StringComparison.CurrentCultureIgnoreCase))
            path = path.Substring(9);

        return path;
    }

    /// <summary>
    /// Returns a key from a registry key path
    /// </summary>
    /// <param name="fullPath">The full path of the key</param>
    /// <param name="registryView">The view to use when opening the key</param>
    /// <param name="writable">True if the key should be writable</param>
    /// <returns>The registry key</returns>
    public static RegistryKey? GetKeyFromFullPath(string fullPath, RegistryView registryView, bool writable = false)
    {
        // Split the keys
        string[] keys = NormalizePath(fullPath).Split(KeySeparatorCharacter);

        RegistryKey? key = null;

        try
        {
            // Open the base key
            key = RegistryKey.OpenBaseKey(GetHiveFromName(keys[0]), registryView);

            // If the path is only the base key, return it
            if (keys.Length == 1)
                return key;

            // Get the sub key
            RegistryKey? returnValue = key.OpenSubKey(String.Join(KeySeparatorCharacter.ToString(), keys.Skip(1)), writable);

            // Return the sub key
            return returnValue;
        }
        finally
        {
            // Dispose the base key
            key?.Dispose();
        }
    }

    /// <summary>
    /// Gets the full name of a hive and returns null if not found
    /// </summary>
    /// <param name="hive">The hive to get the name from</param>
    /// <returns>The name of the hive</returns>
    public static string? GetFullName(RegistryHive hive)
    {
        return hive switch
        {
            RegistryHive.ClassesRoot => "HKEY_CLASSES_ROOT",
            RegistryHive.CurrentUser => "HKEY_CURRENT_USER",
            RegistryHive.LocalMachine => "HKEY_LOCAL_MACHINE",
            RegistryHive.Users => "HKEY_USERS",
            RegistryHive.PerformanceData => "HKEY_PERFORMANCE_DATA",
            RegistryHive.CurrentConfig => "HKEY_CURRENT_CONFIG",
#if NETFRAMEWORK
            RegistryHive.DynData => "HKEY_DYN_DATA",
#endif
            _ => null
        };
    }

    /// <summary>
    /// Gets the hive from a full name or shorthand version of it
    /// </summary>
    /// <param name="name">The name of the hive</param>
    /// <returns>The hive</returns>
    /// <exception cref="ArgumentException"/>
    public static RegistryHive GetHiveFromName(string? name)
    {
        return name switch
        {
            "HKEY_LOCAL_MACHINE" or "HKLM" => RegistryHive.LocalMachine,
            "HKEY_CURRENT_CONFIG" or "HKCC" => RegistryHive.CurrentConfig,
            "HKEY_CLASSES_ROOT" or "HKCR" => RegistryHive.ClassesRoot,
            "HKEY_CURRENT_USER" or "HKCU" => RegistryHive.CurrentUser,
            "HKEY_USERS" or "HKU" => RegistryHive.Users,
            "HKEY_PERFORMANCE_DATA" => RegistryHive.PerformanceData,
#if NETFRAMEWORK
            "HKEY_DYN_DATA" => RegistryHive.DynData,
#endif
            _ => throw new ArgumentException("The provided name is not valid")
        };
    }

    /// <summary>
    /// Combines the key paths into one absolute path
    /// </summary>
    /// <param name="keyPaths">The paths to combine</param>
    /// <returns>The combined path</returns>
    public static string CombinePaths(params string[] keyPaths)
    {
        // Normalize the paths and return as absolute path
        return String.Join(KeySeparatorCharacter.ToString(), keyPaths.Select(x => x.Trim(KeySeparatorCharacter)));
    }

    /// <summary>
    /// Creates a new Registry key in a specified path
    /// </summary>
    /// <param name="keyPath">The path to create the key</param>
    /// <param name="registryView">The view to use when creating the key</param>
    /// <param name="writable">True if the key should be writable</param>
    /// <returns>The registry key</returns>
    /// <exception cref="ArgumentNullException">keyPath is null</exception>
    /// <exception cref="ArgumentException">keyPath is not a valid key path</exception>
    public static RegistryKey? CreateRegistryKey(string keyPath, RegistryView registryView, bool writable = false)
    {
        if (keyPath == null)
            throw new ArgumentNullException(nameof(keyPath));

        RegistryHive hive = GetHiveFromName(keyPath.Split(KeySeparatorCharacter).FirstOrDefault());

        using RegistryKey key = RegistryKey.OpenBaseKey(hive, registryView);
        return key.CreateSubKey(keyPath.Remove(0, GetFullName(hive)!.Length + KeySeparatorCharacter.ToString().Length), writable);
    }

    /// <summary>
    /// The separator character used for key paths
    /// </summary>
    public const char KeySeparatorCharacter = '\\';
}