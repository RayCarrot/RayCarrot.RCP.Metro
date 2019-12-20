using MahApps.Metro.IconPacks;
using RayCarrot.CarrotFramework.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using RayCarrot.RCP.Core;

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
        /// <param name="displayNameGenerator">The generator for getting the display name</param>
        /// <param name="iconKind">The category icon</param>
        public GameCategoryViewModel(IEnumerable<Games> games, Func<string> displayNameGenerator, PackIconMaterialKind iconKind)
        {
            // Set properties
            Games = games.ToArray();
            DisplayNameGenerator = displayNameGenerator;
            DisplayName = displayNameGenerator();
            IconKind = iconKind;
            IsMaster = false;
            
            // Create properties
            InstalledGames = new ObservableCollection<GameDisplayViewModel>();
            NotInstalledGames = new ObservableCollection<GameDisplayViewModel>();

            // Enable collection synchronization
            BindingOperations.EnableCollectionSynchronization(InstalledGames, Application.Current);
            BindingOperations.EnableCollectionSynchronization(NotInstalledGames, Application.Current);

            // Subscribe to events
            RCFCore.Data.CultureChanged += Data_CultureChanged;
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

        #region Protected Properties

        /// <summary>
        /// The generator for getting the display name
        /// </summary>
        protected Func<string> DisplayNameGenerator { get; }

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
        public string DisplayName { get; set; }

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

        #region Event Handlers

        private void Data_CultureChanged(object sender, PropertyChangedEventArgs<CultureInfo> e)
        {
            DisplayName = DisplayNameGenerator?.Invoke();
        }

        #endregion

        #region Public Methods

        public void Dispose()
        {
            RCFCore.Data.CultureChanged -= Data_CultureChanged;

            BindingOperations.DisableCollectionSynchronization(InstalledGames);
            BindingOperations.DisableCollectionSynchronization(NotInstalledGames);
        }

        #endregion
    }
}