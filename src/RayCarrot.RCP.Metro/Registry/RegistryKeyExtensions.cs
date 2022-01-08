#nullable disable
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Extension methods for <see cref="RegistryKey"/>
    /// </summary>
    public static class RegistryKeyExtensions
    {
        /// <summary>
        /// Checks if the specified value exists within the specified key
        /// </summary>
        /// <param name="registryKey">The registry key</param>
        /// <param name="valueName">The name of the value to search for</param>
        /// <returns>True if the value was found, false if not</returns>
        /// <exception cref="ArgumentNullException"/>
        public static bool HasValue(this RegistryKey registryKey, string valueName)
        {
            if (valueName == null)
                throw new ArgumentNullException(nameof(valueName));

            return registryKey.GetValueNames().Contains(valueName);
        }

        /// <summary>
        /// Gets an enumerable of the available sub keys full paths
        /// </summary>
        /// <param name="registryKey">The registry key</param>
        /// <returns>The enumerable of the available sub keys full paths</returns>
        public static IEnumerable<string> GetSubKeyFullPaths(this RegistryKey registryKey)
        {
            // Get the sub key names
            var names = registryKey.GetSubKeyNames();

            // Combines their names with the current key path
            return names.Select(x => RegistryHelpers.CombinePaths(registryKey.Name, x));
        }

        /// <summary>
        /// Gets the parent key
        /// </summary>
        /// <param name="registryKey">The registry key</param>
        /// <param name="writable">True if the key should be writable</param>
        /// <returns>The parent key</returns>
        public static RegistryKey GetParentKey(this RegistryKey registryKey, bool writable = false)
        {
            return RegistryHelpers.GetKeyFromFullPath(RegistryHelpers.GetParentKeyPath(registryKey.Name), registryKey.View, writable);
        }

        /// <summary>
        /// Deletes a registry key and disposes it
        /// </summary>
        /// <param name="registryKey">The key to delete</param>
        /// <param name="recursive">True if all sub-keys and values should be deleted</param>
        public static void DeleteKey(this RegistryKey registryKey, bool recursive)
        {
            // Save the name of the key
            string name = RegistryHelpers.GetSubKeyName(registryKey.Name);

            // Get the parent key
            using RegistryKey parent = registryKey.GetParentKey(true);
            
            // Dispose the key
            registryKey.Dispose();

            if (recursive)
                // Delete the key recursively
                parent.DeleteSubKeyTree(name);
            else
                // Delete the key
                parent.DeleteSubKey(name);
        }

        /// <summary>
        /// Renames the specified sub key
        /// </summary>
        /// <param name="registryKey">The parent registry key</param>
        /// <param name="subKeyName">The name of the sub key to rename</param>
        /// <param name="newSubKeyName">The new name of the key</param>
        public static void MoveSubKey(this RegistryKey registryKey, string subKeyName, string newSubKeyName)
        {
            // Make sure the new name doesn't exist
            if (RegistryHelpers.KeyExists(RegistryHelpers.CombinePaths(registryKey.Name, newSubKeyName), registryKey.View))
                throw new Exception($"The key {newSubKeyName} already exists");

            // Make sure the original key exists
            if (!RegistryHelpers.KeyExists(RegistryHelpers.CombinePaths(registryKey.Name, subKeyName), registryKey.View))
                throw new Exception($"The key {subKeyName} does not exist");

            try
            {
                // Copy the key
                registryKey.CopySubKey(subKeyName, newSubKeyName);
            }
            catch (Exception ex)
            {
                // Delete copied key
                registryKey.DeleteSubKeyTree(newSubKeyName, false);

                throw new Exception("Failed to copy sub key tree. Operation failed.", ex);
            }

            try
            {
                // Delete the old key
                registryKey.DeleteSubKeyTree(subKeyName);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to fully delete old key", ex);
            }
        }

        /// <summary>
        /// Renames the specified value
        /// </summary>
        /// <param name="registryKey">The parent registry key</param>
        /// <param name="valueName">The name of the value to rename</param>
        /// <param name="newValueName">The new name of the value</param>
        public static void MoveValue(this RegistryKey registryKey, string valueName, string newValueName)
        {
            if (String.IsNullOrEmpty(valueName))
                throw new ArgumentException();

            if (String.IsNullOrEmpty(newValueName))
                throw new ArgumentException();

            // Copy the value
            registryKey.CopyValue(valueName, newValueName);

            // Delete the old value
            registryKey.DeleteValue(valueName);
        }

        /// <summary>
        /// Copies a registry key
        /// </summary>
        /// <param name="registryKey">The parent registry key</param>
        /// <param name="keyNameToCopy">The name of the sub key to copy</param>
        /// <param name="newKeyName">The new name of the key</param>
        public static void CopySubKey(this RegistryKey registryKey, string keyNameToCopy, string newKeyName)
        {
            // Create new key
            RegistryKey destinationKey = registryKey.CreateSubKey(newKeyName);

            // Open the sourceKey we are copying from
            RegistryKey sourceKey = registryKey.OpenSubKey(keyNameToCopy);

            // Copy the key
            RecurseCopyKey(sourceKey, destinationKey);

            // Recursive method for copying a key
            void RecurseCopyKey(RegistryKey source, RegistryKey destination)
            {
                // Copy all values
                foreach (string valueName in source.GetValueNames())
                {
                    object objValue = source.GetValue(valueName);
                    RegistryValueKind valKind = source.GetValueKind(valueName);
                    destination.SetValue(valueName, objValue, valKind);
                }

                // Copy all sub keys
                foreach (string sourceSubKeyName in source.GetSubKeyNames())
                {
                    RegistryKey sourceSubKey = source.OpenSubKey(sourceSubKeyName);
                    RegistryKey destSubKey = destination.CreateSubKey(sourceSubKeyName);
                    RecurseCopyKey(sourceSubKey, destSubKey);
                }
            }
        }

        /// <summary>
        /// Copies a registry value
        /// </summary>
        /// <param name="registryKey">The parent registry key</param>
        /// <param name="valueNameToCopy">The name of the value to copy</param>
        /// <param name="newValueName">The new name of the value</param>
        public static void CopyValue(this RegistryKey registryKey, string valueNameToCopy, string newValueName)
        {
            // Create new value
            registryKey.SetValue(newValueName, registryKey.GetValue(valueNameToCopy), registryKey.GetValueKind(newValueName));
        }

        /// <summary>
        /// Returns all values in the key
        /// </summary>
        /// <param name="registryKey">The registry key</param>
        /// <returns>The values in the key</returns>
        public static IEnumerable<RegistryValue> GetValues(this RegistryKey registryKey)
        {
            return registryKey.GetValueNames().Select(x => new RegistryValue(registryKey, x));
        }

        /// <summary>
        /// Gets and opens all sub keys in the specified key
        /// </summary>
        /// <param name="registryKey">The registry key</param>
        /// <param name="writable">Indicates if the keys should be writable</param>
        /// <returns>The sub keys in the key</returns>
        public static List<RegistryKey> GetSubKeys(this RegistryKey registryKey, bool writable = false)
        {
            // Create the output
            List<RegistryKey> output = new List<RegistryKey>();

            try
            {
                // Open every sub key
                foreach (var subKeyName in registryKey.GetSubKeyNames())
                    output.Add(registryKey.OpenSubKey(subKeyName, writable));

                return output;
            }
            catch
            {
                // Dispose all opened sub keys
                output.ForEach(x => x.Dispose());
                throw;
            }
        }

        /// <summary>
        /// Checks if the specified sub key can be opened with read permissions
        /// </summary>
        /// <param name="registryKey">The parent key</param>
        /// <param name="subKeyName">The name of the sub key to check</param>
        /// <returns>True if it can be opened with read permissions, otherwise false</returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ObjectDisposedException">The <see cref="RegistryKey"/> is closed (closed keys cannot be accessed)</exception>
        public static bool HasSubKeyReadPermission(this RegistryKey registryKey, string subKeyName)
        {
            try
            {
                registryKey.OpenSubKey(subKeyName, false)?.Dispose();
                return true;
            }
            catch (SecurityException)
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if the specified sub key can be opened with write permissions
        /// </summary>
        /// <param name="registryKey">The parent key</param>
        /// <param name="subKeyName">The name of the sub key to check</param>
        /// <returns>True if it can be opened with write permissions, otherwise false</returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ObjectDisposedException">The <see cref="RegistryKey"/> is closed (closed keys cannot be accessed)</exception>
        public static bool HasSubKeyWritePermission(this RegistryKey registryKey, string subKeyName)
        {
            try
            {
                registryKey.OpenSubKey(subKeyName, true)?.Dispose();
                return true;
            }
            catch (SecurityException)
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if the specified key can be opened with write permissions
        /// </summary>
        /// <param name="registryKey">The key to check</param>
        /// <returns>True if it can be opened with write permissions, otherwise false</returns>
        /// <exception cref="ArgumentException">The parent key could not be opened from the Registry key</exception>
        /// <exception cref="ObjectDisposedException">The <see cref="RegistryKey"/> is closed (closed keys cannot be accessed)</exception>
        public static bool HasWritePermission(this RegistryKey registryKey)
        {
            try
            {
                using (var key = registryKey.GetParentKey())
                    key.OpenSubKey(RegistryHelpers.GetSubKeyName(registryKey.Name))?.Dispose();

                return true;
            }
            catch (SecurityException)
            {
                return false;
            }
            catch (ObjectDisposedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ArgumentException("The parent key could not be opened from the Registry key", ex);
            }
        }

        /// <summary>
        /// Checks if all sub keys from the specified key can be opened with read permissions
        /// </summary>
        /// <param name="registryKey">The key from which the sub keys should be checked</param>
        /// <returns>True if they can all can be opened with read permissions, otherwise false</returns>
        /// <exception cref="ObjectDisposedException">The <see cref="RegistryKey"/> is closed (closed keys cannot be accessed)</exception>
        public static bool HasSubKeyTreeReadPermissions(this RegistryKey registryKey)
        {
            try
            {
                TryOpenSubKeys(registryKey, false);
                return true;
            }
            catch (SecurityException)
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if all sub keys from the specified key can be opened with write permissions
        /// </summary>
        /// <param name="registryKey">The key from which the sub keys should be checked</param>
        /// <returns>True if they can all can be opened with write permissions, otherwise false</returns>
        /// <exception cref="ObjectDisposedException">The <see cref="RegistryKey"/> is closed (closed keys cannot be accessed)</exception>
        public static bool HasSubKeyTreeWritePermissions(this RegistryKey registryKey)
        {
            try
            {
                TryOpenSubKeys(registryKey, true);
                return true;
            }
            catch (SecurityException)
            {
                return false;
            }
        }

        /// <summary>
        /// Attempts to open the sub keys recursively for the specified key 
        /// </summary>
        /// <param name="key">The key to open the sub keys from</param>
        /// <param name="writable">True if they should be writable, otherwise false</param>
        private static void TryOpenSubKeys(RegistryKey key, bool writable)
        {
            foreach (var subKey in key.GetSubKeys(writable))
                using (subKey)
                    TryOpenSubKeys(subKey, writable);
        }
    }
}