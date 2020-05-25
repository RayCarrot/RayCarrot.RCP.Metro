using Microsoft.Extensions.DependencyInjection;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Extension methods for <see cref="IServiceCollection"/>
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds an update manager to the service collection
        /// </summary>
        /// <typeparam name="U">The type of update manager to add</typeparam>
        /// <param name="construction">The services</param>
        /// <returns>The services</returns>
        public static IServiceCollection AddUpdateManager<U>(this IServiceCollection construction)
            where U : class, IUpdaterManager, new()
        {
            // Add the manager
            construction.AddTransient<IUpdaterManager, U>();

            // Return the service collection
            return construction;
        }

        /// <summary>
        /// Adds a file manager to the service collection
        /// </summary>
        /// <typeparam name="F">The type of file manager to add</typeparam>
        /// <param name="construction">The services</param>
        /// <returns>The services</returns>
        public static IServiceCollection AddFileManager<F>(this IServiceCollection construction)
            where F : class, IFileManager, new()
        {
            // Add the manager
            construction.AddTransient<IFileManager, F>();

            // Return the service collection
            return construction;
        }
    }
}