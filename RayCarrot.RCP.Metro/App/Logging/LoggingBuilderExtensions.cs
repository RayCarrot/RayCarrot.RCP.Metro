using Microsoft.Extensions.Logging;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Extension methods for <see cref="LoggingBuilder"/>
    /// </summary>
    public static class LoggingBuilderExtensions
    {
        /// <summary>
        /// Adds a <see cref="SessionLogger"/>
        /// </summary>
        /// <param name="builder">The builder</param>
        /// <returns></returns>
        public static ILoggingBuilder AddSession(this ILoggingBuilder builder)
        {
            builder.AddProvider(new BaseLogProvider<SessionLogger>());
            return builder;
        }
    }
}