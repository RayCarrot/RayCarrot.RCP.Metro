using System;
using System.ComponentModel;
using MahApps.Metro.IconPacks;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Common archive explorer file/directory entry view model
/// </summary>
public interface IArchiveExplorerEntryViewModel : INotifyPropertyChanged, IDisposable
{
    /// <summary>
    /// Indicates if the item is selected
    /// </summary>
    bool IsSelected { get; set; }

    /// <summary>
    /// The name of the item to display
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// The full path for the item
    /// </summary>
    string FullPath { get; }

    /// <summary>
    /// The generic icon kind to use for the item
    /// </summary>
    PackIconMaterialKind GenericIconKind { get; }

    /// <summary>
    /// Indicates if the entry is a file, otherwise it's a directory or archive
    /// </summary>
    bool IsFile { get; }
}