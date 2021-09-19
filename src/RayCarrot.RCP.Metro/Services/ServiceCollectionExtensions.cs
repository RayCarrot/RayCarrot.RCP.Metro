using Microsoft.Extensions.DependencyInjection;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Extension methods for <see cref="IServiceCollection"/>
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a <see cref="IDialogBaseManager"/> to the service collection
        /// </summary>
        /// <typeparam name="D">The dialog base manager to add</typeparam>
        /// <param name="services">The services</param>
        /// <returns>The services</returns>
        public static IServiceCollection AddDialogBaseManager<D>(this IServiceCollection services)
            where D : class, IDialogBaseManager, new()
        {
            // Add the service
            services.AddSingleton<IDialogBaseManager, D>();

            // Return the service collection
            return services;
        }

        /// <summary>
        /// Adds a message UIManager to the service collection
        /// </summary>
        /// <typeparam name="U">The type of message UI Manager to use</typeparam>
        /// <param name="services">The services</param>
        /// <returns>The services</returns>
        public static IServiceCollection AddMessageUIManager<U>(this IServiceCollection services)
            where U : class, IMessageUIManager, new()
        {
            // Add the service
            services.AddTransient<IMessageUIManager, U>();

            // Return the service collection
            return services;
        }

        /// <summary>
        /// Adds a browse UIManager to the service collection
        /// </summary>
        /// <typeparam name="U">The type of browse UI Manager to use</typeparam>
        /// <param name="services">The services</param>
        /// <returns>The services</returns>
        public static IServiceCollection AddBrowseUIManager<U>(this IServiceCollection services)
            where U : class, IBrowseUIManager, new()
        {
            // Add the service
            services.AddTransient<IBrowseUIManager, U>();

            // Return the service collection
            return services;
        }

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