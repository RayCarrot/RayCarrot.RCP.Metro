using System.Globalization;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Helper class for application languages
    /// </summary>
    public static class AppLanguages
    {
        #region Constructor

        static AppLanguages()
        {
            Languages = new CultureInfo[]
            {
                // English
                new CultureInfo("en-US"),

                // Swedish
                new CultureInfo("sv-SE"),

                // German
                new CultureInfo("de-DE"),

                // Polish
                new CultureInfo("pl-PL"),

                // Portuguese
                new CultureInfo("pt-PT"), 
            };
        }

        #endregion

        #region Public Static Properties

        /// <summary>
        /// The available languages
        /// </summary>
        public static CultureInfo[] Languages { get; }

        /// <summary>
        /// The path to the resource file
        /// </summary>
        public static string ResourcePath => "RayCarrot.RCP.Metro.Localization.Resources";

        #endregion
    }
}