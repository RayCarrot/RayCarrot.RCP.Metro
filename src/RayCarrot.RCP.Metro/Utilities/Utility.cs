using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The base class for a utility
/// </summary>
public abstract class Utility
{
    /// <summary>
    /// The header for the utility. This property is retrieved again when the current culture is changed.
    /// </summary>
    public abstract string DisplayHeader { get; } // TODO-UPDATE: Change to a localized string to avoid capturing 'this' when used

    /// <summary>
    /// The utility icon
    /// </summary>
    public abstract GenericIconKind Icon { get; }

    /// <summary>
    /// The utility information text (optional). This property is retrieved again when the current culture is changed.
    /// </summary>
    public virtual string? InfoText => null; // TODO-UPDATE: Change to a localized string to avoid capturing 'this' when used

    /// <summary>
    /// The utility warning text (optional). This property is retrieved again when the current culture is changed.
    /// </summary>
    public virtual string? WarningText => null; // TODO-UPDATE: Change to a localized string to avoid capturing 'this' when used

    /// <summary>
    /// Indicates if the utility requires additional files to be downloaded remotely
    /// </summary>
    public virtual bool RequiresAdditionalFiles => false;

    /// <summary>
    /// Indicates if the utility is work in process
    /// </summary>
    public virtual bool IsWorkInProcess => false;

    /// <summary>
    /// The utility UI content
    /// </summary>
    public abstract object UIContent { get; }

    /// <summary>
    /// Indicates if the utility is currently loading and the UI can't be interacted with
    /// </summary>
    public virtual bool IsLoading => false;

    /// <summary>
    /// Indicates if the utility requires administration privileges
    /// </summary>
    public virtual bool RequiresAdmin => false;

    /// <summary>
    /// Indicates if the utility is available to the user
    /// </summary>
    public virtual bool IsAvailable => true;

    /// <summary>
    /// Optional text information to show if the utility is not available
    /// </summary>
    public virtual LocalizedString? NotAvailableInfo => null;

    /// <summary>
    /// Retrieves a list of applied utilities from this utility
    /// </summary>
    /// <returns>The applied utilities</returns>
    public virtual IEnumerable<string> GetAppliedUtilities() => Enumerable.Empty<string>();

    public event EventHandler? IsLoadingChanged;

    protected virtual void OnIsLoadingChanged() => IsLoadingChanged?.Invoke(this, EventArgs.Empty);
}

public abstract class Utility<UI, VM> : Utility
    where UI : FrameworkElement, new()
    where VM : INotifyPropertyChanged
{
    protected Utility() : this(Activator.CreateInstance<VM>()) { }
    protected Utility(VM viewModel) => ViewModel = viewModel;

    public sealed override object UIContent => new UI()
    {
        DataContext = ViewModel
    };

    /// <summary>
    /// The utility view model
    /// </summary>
    public VM ViewModel { get; }
}