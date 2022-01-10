#nullable disable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for a game category
/// </summary>
public class Page_Games_CategoryViewModel : BaseRCPViewModel, IDisposable
{
    #region Constructor

    /// <summary>
    /// Constructor for a category which is visible
    /// </summary>
    /// <param name="games">The games in this category</param>
    /// <param name="displayName">The display name</param>
    /// <param name="icon">The category icon</param>
    public Page_Games_CategoryViewModel(IEnumerable<Games> games, LocalizedString displayName, GenericIconKind icon)
    {
        // Set properties
        Games = games.ToArray();
        DisplayName = displayName;
        Icon = icon;
        IsMaster = false;
            
        // Create properties
        InstalledGames = new ObservableCollection<Page_Games_GameViewModel>();
        NotInstalledGames = new ObservableCollection<Page_Games_GameViewModel>();

        // Enable collection synchronization
        BindingOperations.EnableCollectionSynchronization(InstalledGames, Application.Current);
        BindingOperations.EnableCollectionSynchronization(NotInstalledGames, Application.Current);
    }

    /// <summary>
    /// Constructor for a master category
    /// </summary>
    /// <param name="games">The games in this category</param>
    public Page_Games_CategoryViewModel(IEnumerable<Games> games)
    {
        // Set properties
        Games = games.ToArray();
        DisplayName = new ConstLocString("(master)");
        IsMaster = true;

        // Create properties
        InstalledGames = new ObservableCollection<Page_Games_GameViewModel>();
        NotInstalledGames = new ObservableCollection<Page_Games_GameViewModel>();

        // Enable collection synchronization
        BindingOperations.EnableCollectionSynchronization(InstalledGames, Application.Current);
        BindingOperations.EnableCollectionSynchronization(NotInstalledGames, Application.Current);
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// Indicates if the category is a master category
    /// </summary>
    public bool IsMaster { get; }

    /// <summary>
    /// Indicates if the category is visible
    /// </summary>
    public bool IsVisible { get; set; }

    /// <summary>
    /// The games in this category
    /// </summary>
    public Games[] Games { get; }

    /// <summary>
    /// The category display name
    /// </summary>
    public LocalizedString DisplayName { get; }

    /// <summary>
    /// The category icon
    /// </summary>
    public GenericIconKind Icon { get; }

    /// <summary>
    /// The installed games in this category
    /// </summary>
    public ObservableCollection<Page_Games_GameViewModel> InstalledGames { get; }

    /// <summary>
    /// The not installed games in this category
    /// </summary>
    public ObservableCollection<Page_Games_GameViewModel> NotInstalledGames { get; }

    /// <summary>
    /// Indicates if there are any installed games in this category
    /// </summary>
    public bool AnyInstalledGames { get; set; }

    /// <summary>
    /// Indicates if there are any not installed games in this category
    /// </summary>
    public bool AnyNotInstalledGames { get; set; }

    #endregion

    #region Public Methods

    public void Dispose()
    {
        DisplayName?.Dispose();

        // Disable collection synchronization
        BindingOperations.DisableCollectionSynchronization(InstalledGames);
        BindingOperations.DisableCollectionSynchronization(NotInstalledGames);
    }

    #endregion
}