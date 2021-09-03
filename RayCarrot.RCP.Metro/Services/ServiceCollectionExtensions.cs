using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RayCarrot.Logging;
using System;

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
            services.AddTransient<IDialogBaseManager, D>();

            // Return the service collection
            return services;
        }

        /// <summary>
        /// Add a <see cref="IExceptionHandler"/> to the service collection
        /// </summary>
        /// <typeparam name="E">The type of exception handler to add</typeparam>
        /// <param name="services">The services</param>
        /// <param name="setupExceptionHandling">Indicates if exception handling should be set up through RayCarrot.Logging</param>
        /// <returns>The services</returns>
        public static IServiceCollection AddExceptionHandler<E>(this IServiceCollection services, bool setupExceptionHandling = true)
            where E : class, IExceptionHandler, new()
        {
            // Add the exception handler
            services.AddTransient<IExceptionHandler, E>();

            // Setup the exception handler
            if (setupExceptionHandling)
                RL.Setup(() => BaseApp.Current.GetService<IExceptionHandler>());

            // Return the service collection
            return services;
        }

        /// <summary>
        /// Adds loggers to the service collection
        /// </summary>
        /// <param name="services">The services</param>
        /// <param name="defaultLoggers">Indicates the loggers to add</param>
        /// <param name="minLogLevel">The minimum log level to log</param>
        /// <param name="loggingInjection">An action for injecting custom loggers</param>
        /// <param name="setupLogging">Indicates if logging should be set up through RayCarrot.Logging</param>
        /// <returns>The services</returns>
        public static IServiceCollection AddLoggers(this IServiceCollection services, DefaultLoggers defaultLoggers, LogLevel minLogLevel = LogLevel.Information, Action<ILoggingBuilder> loggingInjection = null, bool setupLogging = true)
        {
            // Add the loggers
            services.AddLogging(options =>
            {
                // Add filter
                options.AddFilter("RL", minLogLevel);

                // Check the flags
                if (defaultLoggers.HasFlag(DefaultLoggers.Console))
                    // Add console logger
                    options.AddConsole();

                // Check the flags
                if (defaultLoggers.HasFlag(DefaultLoggers.Debug))
                    // Add debug logger
                    options.AddDebug();

                // Check the flags
                if (defaultLoggers.HasFlag(DefaultLoggers.Session))
                {
                    // Add session logger
                    options.AddSession();

                    // Add the session logger collection
                    services.AddSingleton<ISessionLoggerCollection>(new DefaultSessionLoggerCollection());
                }

                // Add custom loggers
                loggingInjection?.Invoke(options);
            });

            // Adds a default logger so that we can get a non-generic ILogger that will have the category name of "RL"
            services.AddTransient(provider => provider.GetService<ILoggerFactory>().CreateLogger("RL"));

            // Setup the logger
            if (setupLogging)
                RL.Setup(() => BaseApp.Current.GetService<ILogger>());

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