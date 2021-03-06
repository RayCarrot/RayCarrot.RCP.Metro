﻿using RayCarrot.UI;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using Infralution.Localization.Wpf;
using RayCarrot.Logging;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Rayman Control Panel localization manager
    /// </summary>
    public static class LocalizationManager
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        static LocalizationManager()
        {
            // Create the collection of available languages
            Languages = new ObservableCollection<CultureInfo>();

            // Set the default culture to English
            DefaultCulture = new CultureInfo("en-US");
            
            // Refresh the available languages
            RefreshLanguages(false);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refreshes the list of available languages
        /// </summary>
        /// <param name="includeIncomplete">Indicates if languages with incomplete translations should be included</param>
        public static void RefreshLanguages(bool includeIncomplete)
        {
            // Clear the collection
            Languages.Clear();

            // Add complete languages
            Languages.AddRange(new CultureInfo[]
            {
                DefaultCulture,

                // Dutch (Netherlands)
                new CultureInfo("nl-NL"),

                // Italian (Italy)
                new CultureInfo("it-IT"),

                // Polish (Poland)
                new CultureInfo("pl-PL"),

                // Portuguese (Portugal)
                new CultureInfo("pt-PT"),

                // Russian (Russia)
                new CultureInfo("ru-RU"),
            });

            if (includeIncomplete)
            {
                // Add incomplete languages
                Languages.AddRange(new CultureInfo[]
                {
                    // Catalan (Catalan)
                    new CultureInfo("ca-ES"),

                    // Serbian (Cyrillic)
                    new CultureInfo("sr-Cyrl"), 

                    // Spanish (Mexico)
                    new CultureInfo("es-MX"), 

                    // Spanish (Spain)
                    new CultureInfo("es-ES"),

                    // Swedish (Sweden)
                    new CultureInfo("sv-SE"),
                });
            }
        }

        /// <summary>
        /// Sets the current culture to the specified culture and refreshes it for the application
        /// </summary>
        /// <param name="cultureInfo">The culture info to set to</param>
        public static void SetCulture(string cultureInfo)
        {
            // Lock to the current application
            lock (Application.Current)
            {
                // Store the culture info
                CultureInfo ci;

                try
                {
                    // Attempt to get the culture info
                    ci = CultureInfo.GetCultureInfo(cultureInfo);
                }
                catch (Exception ex)
                {
                    ex.HandleUnexpected("Getting culture info from setter string value");
                    ci = DefaultCulture;
                }

                // Set the UI culture
                CultureManager.UICulture = ci;

                // Set the resource culture
                Resources.Culture = ci;

                // Update the current thread cultures
                CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = ci;
                
                // Set the framework culture
                WPF.Services.Data.CurrentCulture = ci;

                RL.Logger?.LogInformationSource($"The current culture was set to {ci.EnglishName}");
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The default culture
        /// </summary>
        public static CultureInfo DefaultCulture { get; }

        /// <summary>
        /// The available languages
        /// </summary>
        public static ObservableCollection<CultureInfo> Languages { get; }

        #endregion
    }
}