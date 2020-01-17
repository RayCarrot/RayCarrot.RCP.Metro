using MahApps.Metro.IconPacks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for a game category
    /// </summary>
    public class GameCategoryViewModel : BaseRCPViewModel, IDisposable
    {
        #region Constructor

        /// <summary>
        /// Constructor for a category which is visible
        /// </summary>
        /// <param name="games">The games in this category</param>
        /// <param name="displayName">The display name</param>
        /// <param name="iconKind">The category icon</param>
        public GameCategoryViewModel(IEnumerable<Games> games, LocalizedString displayName, PackIconMaterialKind iconKind)
        {
            // Set properties
            Games = games.ToArray();
            DisplayName = displayName;
            IconKind = iconKind;
            IsMaster = false;
            
            // Create properties
            InstalledGames = new ObservableCollection<GameDisplayViewModel>();
            NotInstalledGames = new ObservableCollection<GameDisplayViewModel>();

            // Enable collection synchronization
            BindingOperations.EnableCollectionSynchronization(InstalledGames, Application.Current);
            BindingOperations.EnableCollectionSynchronization(NotInstalledGames, Application.Current);
        }

        /// <summary>
        /// Constructor for a master category
        /// </summary>
        /// <param name="games">The games in this category</param>
        public GameCategoryViewModel(IEnumerable<Games> games)
        {
            // Set properties
            Games = games.ToArray();
            IsMaster = true;
            
            // Create properties
            InstalledGames = new ObservableCollection<GameDisplayViewModel>();
            NotInstalledGames = new ObservableCollection<GameDisplayViewModel>();

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
        public PackIconMaterialKind IconKind { get; }

        /// <summary>
        /// The installed games in this category
        /// </summary>
        public ObservableCollection<GameDisplayViewModel> InstalledGames { get; }

        /// <summary>
        /// The not installed games in this category
        /// </summary>
        public ObservableCollection<GameDisplayViewModel> NotInstalledGames { get; }

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

            BindingOperations.DisableCollectionSynchronization(InstalledGames);
            BindingOperations.DisableCollectionSynchronization(NotInstalledGames);
        }

        #endregion
    }
}