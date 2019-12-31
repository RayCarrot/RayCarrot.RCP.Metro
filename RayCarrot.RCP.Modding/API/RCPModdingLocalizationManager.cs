using Infralution.Localization.Wpf;
using System.Globalization;
using RayCarrot.RCP.Core;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The localization manager for the Rayman Control Panel Modding
    /// </summary>
    public class RCPModdingLocalizationManager : RCPLocalizationManager
    {
        /// <summary>
        /// Sets the current culture for the local project
        /// </summary>
        /// <param name="cultureInfo">The culture info to set to</param>
        protected override void SetLocalCulture(CultureInfo cultureInfo)
        {
            // Set the UI culture
            CultureManager.UICulture = cultureInfo;

            // Set local culture
            Resources.Culture = cultureInfo;
        }
    }
}