using System.Collections.Generic;

namespace RayCarrot.RCP.Core
{
    /// <summary>
    /// The settings for the API controller
    /// </summary>
    public class APIControllerSettings
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public APIControllerSettings()
        {
            // Create custom settings collection
            CustomSettings = new Dictionary<string, object>();
        }

        /// <summary>
        /// Custom settings where the key is the assembly name and the object is the settings instance
        /// </summary>
        public Dictionary<string, object> CustomSettings { get; }
    }
}