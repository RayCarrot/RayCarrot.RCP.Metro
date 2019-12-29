using RayCarrot.CarrotFramework.Abstractions;

namespace RayCarrot.RCP.Core
{
    /// <summary>
    /// Extension methods for <see cref="IFrameworkConstruction"/>
    /// </summary>
    public static class FrameworkConstructionExtensions
    {
        /// <summary>
        /// Adds an API controller manager
        /// </summary>
        /// <typeparam name="A">The type of API controller manager to add</typeparam>
        /// <param name="construction">The construction</param>
        /// <returns>The construction</returns>
        public static IFrameworkConstruction AddAPIControllerManager<A>(this IFrameworkConstruction construction)
            where A : class, IAPIControllerManager, new()
        {
            // Add the manager
            construction.AddTransient<IAPIControllerManager, A>();

            // Return the construction
            return construction;
        }

        /// <summary>
        /// Adds an update manager to the construction
        /// </summary>
        /// <typeparam name="U">The type of update manager to add</typeparam>
        /// <param name="construction">The construction</param>
        /// <returns>The construction</returns>
        public static IFrameworkConstruction AddUpdateManager<U>(this IFrameworkConstruction construction)
            where U : class, IUpdaterManager, new()
        {
            // Add the manager
            construction.AddTransient<IUpdaterManager, U>();

            // Return the construction
            return construction;
        }

        /// <summary>
        /// Adds a file manager to the construction
        /// </summary>
        /// <typeparam name="F">The type of file manager to add</typeparam>
        /// <param name="construction">The construction</param>
        /// <returns>The construction</returns>
        public static IFrameworkConstruction AddFileManager<F>(this IFrameworkConstruction construction)
            where F : class, IFileManager, new()
        {
            // Add the manager
            construction.AddTransient<IFileManager, F>();

            // Return the construction
            return construction;
        }

        /// <summary>
        /// Adds the application paths to the construction
        /// </summary>
        /// <typeparam name="P">The type of application paths to add</typeparam>
        /// <param name="construction">The construction</param>
        /// <returns>The construction</returns>
        public static IFrameworkConstruction AddApplicationPaths<P>(this IFrameworkConstruction construction)
            where P : RCPApplicationPaths, new()
        {
            // Add the manager
            construction.AddTransient<RCPApplicationPaths, P>();

            // Return the construction
            return construction;
        }

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