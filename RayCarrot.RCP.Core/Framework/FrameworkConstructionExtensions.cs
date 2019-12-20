using RayCarrot.CarrotFramework.Abstractions;

namespace RayCarrot.RCP.Core
{
    /// <summary>
    /// Extension methods for <see cref="IFrameworkConstruction"/>
    /// </summary>
    public static class FrameworkConstructionExtensions
    {
        /// <summary>
        /// Sets up and creates a new API controller instance during a framework construction. This can only be done.
        /// </summary>
        /// <param name="construction">The construction</param>
        /// <param name="settings">The settings</param>
        /// <returns>The construction</returns>
        public static IFrameworkConstruction AddRCPAPI<A>(this IFrameworkConstruction construction, APIControllerSettings settings)
            where A : class, IAPIControllerManager, new()
        {
            // Add the manager
            construction.AddTransient<IAPIControllerManager, A>();

            // Create the API
            APIController.CreateAPI(settings);

            // Return the construction
            return construction;
        }
    }
}