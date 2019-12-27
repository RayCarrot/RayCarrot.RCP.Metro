using RayCarrot.CarrotFramework.Abstractions;

namespace RayCarrot.RCP.UI
{
    /// <summary>
    /// Extension methods for <see cref="IFrameworkConstruction"/>
    /// </summary>
    public static class FrameworkConstructionExtensions
    {
        /// <summary>
        /// Adds an app view model to the construction
        /// </summary>
        /// <typeparam name="A">The type of app view model to add</typeparam>
        /// <param name="construction">The construction</param>
        /// <returns>The construction</returns>
        public static IFrameworkConstruction AddAppViewModel<A>(this IFrameworkConstruction construction)
            where A : BaseRCPAppViewModel, new()
        {
            // Add the manager
            construction.AddSingleton<BaseRCPAppViewModel>(new A());

            // Return the construction
            return construction;
        }

        /// <summary>
        /// Adds a localization manager to the construction
        /// </summary>
        /// <typeparam name="L">The type of localization manager to add</typeparam>
        /// <param name="construction">The construction</param>
        /// <returns>The construction</returns>
        public static IFrameworkConstruction AddLocalizationManager<L>(this IFrameworkConstruction construction)
            where L : RCPLocalizationManager, new()
        {
            // Add the manager
            construction.AddSingleton<RCPLocalizationManager>(new L());

            // Return the construction
            return construction;
        }
    }
}