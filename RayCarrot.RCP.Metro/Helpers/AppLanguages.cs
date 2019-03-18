using System.Collections.Generic;

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
            Languages = new Dictionary<string, string>()
            {
                {
                    "en-US",
                    "English (US)"
                },
                {
                    "sv-SE",
                    "Swedish (SV)"
                }
            };
        }

        #endregion

        #region Public Static Properties

        /// <summary>
        /// The available languages
        /// </summary>
        public static Dictionary<string, string> Languages { get; }

        /// <summary>
        /// The path to the resource file
        /// </summary>
        public static string ResourcePath => "RayCarrot.RCP.Metro.Localization.Resources";

        #endregion
    }
}