using System;

namespace RayCarrot.RCP.Core
{
    /// <summary>
    /// An API controller for the Rayman Control Panel
    /// </summary>
    public sealed class APIController
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="settings">The settings</param>
        private APIController(APIControllerSettings settings)
        {
            Settings = settings;
        }

        /// <summary>
        /// The settings
        /// </summary>
        public APIControllerSettings Settings { get; }

        /// <summary>
        /// Sets up and creates a new API controller instance. This can only be done.
        /// </summary>
        /// <param name="settings">The settings</param>
        public static void CreateAPI(APIControllerSettings settings)
        {
            // Make sure a controller hasn't already been created
            if (Controller != null)
                throw new InvalidOperationException("An API controller can only be created once");

            // Create the controller
            var controller = new APIController(settings);

            // Set the controller
            Controller = controller;
        }

        /// <summary>
        /// The current controller instance
        /// </summary>
        public static APIController Controller { get; private set; }
    }
}