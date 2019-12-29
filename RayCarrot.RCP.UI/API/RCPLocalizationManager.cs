using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.UI;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;

namespace RayCarrot.RCP.UI
{
    /// <summary>
    /// Base class for a Rayman Control Panel localization manager
    /// </summary>
    public abstract class RCPLocalizationManager
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        protected RCPLocalizationManager()
        {
            // Create the collection of available languages
            Languages = new ObservableCollection<CultureInfo>();

            // Set the default culture to English
            DefaultCulture = new CultureInfo("en-US");
            
            // Refresh the available languages
            RefreshLanguages(false);
        }

        #endregion

        #region Protected Abstract Methods

        /// <summary>
        /// Sets the current culture for the local project
        /// </summary>
        /// <param name="cultureInfo">The culture info to set to</param>
        protected abstract void SetLocalCulture(CultureInfo cultureInfo);

        #endregion

        #region Public Methods

        /// <summary>
        /// Refreshes the list of available languages
        /// </summary>
        /// <param name="includeIncomplete">Indicates if languages with incomplete translations should be included</param>
        public void RefreshLanguages(bool includeIncomplete)
        {
            // Clear the collection
            Languages.Clear();

            // Add complete languages
            Languages.AddRange(new CultureInfo[]
            {
                DefaultCulture,

                // Portuguese (Portugal)
                new CultureInfo("pt-PT"),

                // Dutch (Netherlands)
                new CultureInfo("nl-NL"),
            });

            if (includeIncomplete)
            {
                // Add incomplete languages
                Languages.AddRange(new CultureInfo[]
                {
                    // Swedish (Sweden)
                    new CultureInfo("sv-SE"),

                    // German (Germany)
                    new CultureInfo("de-DE"),

                    // Polish (Poland)
                    new CultureInfo("pl-PL"),

                    // Serbian (Cyrillic)
                    new CultureInfo("sr-Cyrl"), 

                    // Spanish (Mexico)
                    new CultureInfo("es-MX"), 

                    // Spanish (Spain)
                    new CultureInfo("es-ES"), 

                    // Catalan (Catalan)
                    new CultureInfo("ca-ES"), 
                });
            }
        }

        /// <summary>
        /// Sets the current culture to the specified culture and refreshes it for the application
        /// </summary>
        /// <param name="cultureInfo">The culture info to set to</param>
        public void SetCulture(string cultureInfo)
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

                // Set local culture
                SetLocalCulture(ci);

                // Set the resource culture
                Core.Resources.Culture = ci;
                Resources.Culture = ci;

                // Update the current thread cultures
                CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.DefaultThreadCurrentCulture = ci;

                // Set the framework culture
                RCFCore.Data.CurrentCulture = ci;

                RCFCore.Logger?.LogInformationSource($"The current culture was set to {ci.EnglishName}");
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The default culture
        /// </summary>
        public CultureInfo DefaultCulture { get; }

        /// <summary>
        /// The available languages
        /// </summary>
        public ObservableCollection<CultureInfo> Languages { get; }

        #endregion
    }
}