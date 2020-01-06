using RayCarrot.CarrotFramework.Abstractions;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Extension methods for <see cref="IFrameworkConstruction"/>
    /// </summary>
    public static class FrameworkConstructionExtensions
    {
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
    }
}