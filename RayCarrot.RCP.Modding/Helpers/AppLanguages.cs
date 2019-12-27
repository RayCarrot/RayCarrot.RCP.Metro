using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using Infralution.Localization.Wpf;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.UI;

namespace RayCarrot.RCP.Modding
{
    // TODO: Instead of copy pasting, move to core

    /// <summary>
    /// Helper class for application languages
    /// </summary>
    public static class AppLanguages
    {
        #region Constructor

        static AppLanguages()
        {
            Languages = new ObservableCollection<CultureInfo>();

            DefaultCulture = new CultureInfo("en-US");
            
            RefreshLanguages(false);
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Refreshes the list of available languages
        /// </summary>
        /// <param name="includeIncomplete">Indicates if languages with incomplete translations should be included</param>
        public static void RefreshLanguages(bool includeIncomplete)
        {
            Languages.Clear();

            Languages.AddRange(new CultureInfo[]
            {
                DefaultCulture,

                // Portuguese
                new CultureInfo("pt-PT"),

                // Dutch
                new CultureInfo("nl-NL"),
            });

            if (includeIncomplete)
            {
                Languages.AddRange(new CultureInfo[]
                {
                    // Swedish
                    new CultureInfo("sv-SE"),

                    // German
                    new CultureInfo("de-DE"),

                    // Polish
                    new CultureInfo("pl-PL"),

                    // Serbian
                    new CultureInfo("sr-Cyrl"), 

                    // Spanish
                    new CultureInfo("es-MX"), 
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
                // TODO: Add for local resources
                //Resources.Culture = ci;
                Core.Resources.Culture = ci;
                UI.Resources.Culture = ci;

                // Update the current thread cultures
                CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.DefaultThreadCurrentCulture = ci;

                // Set the framework culture
                RCFCore.Data.CurrentCulture = ci;

                RCFCore.Logger?.LogInformationSource($"The current culture was set to {ci.EnglishName}");
            }
        }

        #endregion

        #region Public Static Properties

        /// <summary>
        /// The default culture
        /// </summary>
        public static CultureInfo DefaultCulture { get; }

        /// <summary>
        /// The available languages
        /// </summary>
        public static ObservableCollection<CultureInfo> Languages { get; }

        /// <summary>
        /// The path to the resource file
        /// </summary>
        public static string ResourcePath => "RayCarrot.RCP.Modding.Localization.Resources";

        #endregion
    }
}