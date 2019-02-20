using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace SumoLogic.Logging.AspNetCore
{
    public static class LoggerFactoryExtensions
    {
        /// <summary>
        /// Adds the log4net logging provider.
        /// </summary>
        /// <param name="factory">The <see cref="ILoggerFactory"/> to use.</param>
        /// <param name="options">Configure an instance of the <see cref="LoggerOptions" /> to set logging options</param>
        public static ILoggerFactory AddSumoLogic(this ILoggerFactory factory, LoggerOptions options)
        {
            factory.AddProvider(new LoggerProvider(options));
            return factory;
        }

        /// <summary>
        /// Adds a Sumo Logic logger named 'SumoLogic' to the factory.
        /// </summary>
        /// <param name="factory">The <see cref="ILoggerFactory"/> to use.</param>
        public static ILoggerFactory AddSumoLogic(this ILoggerFactory factory)
            => factory.AddSumoLogic(new LoggerOptions());

        /// <summary>
        /// Adds a Sumo Logic logger named 'SumoLogic' to the factory.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="uri">Sets the uri of Sumo Logic ingesting endpoint/param>
        public static ILoggerFactory AddSumoLogic(this ILoggerFactory factory, string uri)
            => factory.AddSumoLogic(new LoggerOptions() { Uri = uri });

#if !NETCOREAPP1_1
        /// <summary>
        /// Adds a Sumo Logic logger named 'SumoLogic' to the factory.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        /// <param name="options">Configure an instance of the <see cref="LoggerOptions" /> to set logging options</param>
        public static ILoggingBuilder AddSumoLogic(this ILoggingBuilder builder, LoggerOptions options)
        {
            builder.Services.AddSingleton<ILoggerProvider>(new LoggerProvider(options));
            return builder;
        }

        /// <summary>
        /// Adds a Sumo Logic logger named 'SumoLogic' to the factory.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        public static ILoggingBuilder AddSumoLogic(this ILoggingBuilder builder)
            => builder.AddSumoLogic(new LoggerOptions());

        /// <summary>
        /// Adds a Sumo Logic logger named 'SumoLogic' to the factory.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        /// <param name="uri">Sets the uri of Sumo Logic ingesting endpoint/param>
        public static ILoggingBuilder AddSumoLogic(this ILoggingBuilder builder, string uri)
            => builder.AddSumoLogic(new LoggerOptions() { Uri = uri });
#endif
    }
}
