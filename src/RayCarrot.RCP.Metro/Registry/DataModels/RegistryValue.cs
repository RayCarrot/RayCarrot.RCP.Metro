using Microsoft.Win32;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Represents a value in the registry
    /// </summary>
    public class RegistryValue
    {
        #region Constructors

        /// <summary>
        /// Creates a new <see cref="RegistryValue"/> from a key path, the value name and view used to get the key
        /// </summary>
        /// <param name="keyPath">The full path of the key</param>
        /// <param name="valueName">The name of the value</param>
        /// <param name="view">The view used to get the key</param>
        public RegistryValue(string keyPath, string valueName, RegistryView view)
        {
            using RegistryKey key = RegistryHelpers.GetKeyFromFullPath(keyPath, view);
            
            Value = key.GetValue(valueName);
            ValueKind = key.GetValueKind(valueName);
            Name = valueName;
            KeyPath = key.Name;
            KeyView = key.View;
        }

        /// <summary>
        /// Creates a new <see cref="RegistryValue"/> from a key and the value name
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="valueName">The name of the value</param>
        public RegistryValue(RegistryKey key, string valueName)
        {
            Value = key.GetValue(valueName);
            ValueKind = key.GetValueKind(valueName);
            Name = valueName;
            KeyPath = key.Name;
            KeyView = key.View;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The value
        /// </summary>
        public object Value { get; private set; }

        /// <summary>
        /// The value kind
        /// </summary>
        public RegistryValueKind ValueKind { get; private set; }

        /// <summary>
        /// The value name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The path to the key containing the value
        /// </summary>
        public string KeyPath { get; }

        /// <summary>
        /// The registry view used to get the key containing the value
        /// </summary>
        public RegistryView KeyView { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the key containing the value
        /// </summary>
        /// <param name="writable">True if the key should be writable</param>
        /// <returns>The key containing the value</returns>
        public RegistryKey GetKey(bool writable = false) =>
            RegistryHelpers.GetKeyFromFullPath(KeyPath, KeyView, writable);

        /// <summary>
        /// Sets the value
        /// </summary>
        /// <param name="value">The new value to set to</param>
        public void SetValue(object value)
        {
            using RegistryKey key = GetKey();
            key.SetValue(Name, value);
            Value = key.GetValue(Name);
            ValueKind = key.GetValueKind(Name);
        }

        /// <summary>
        /// Sets the value
        /// </summary>
        /// <param name="value">The new value to set to</param>
        /// <param name="valueKind">The value kind to use</param>
        public void SetValue(object value, RegistryValueKind valueKind)
        {
            using RegistryKey key = GetKey();
            key.SetValue(Name, value, valueKind);
            Value = key.GetValue(Name);
            ValueKind = key.GetValueKind(Name);
        }

        /// <summary>
        /// Rename the value
        /// </summary>
        /// <param name="newName">The new name of the value</param>
        public void RenameValue(string newName)
        {
            using RegistryKey key = GetKey();
            key.MoveValue(Name, newName);
            Name = newName;
        }

        #endregion
    }
}