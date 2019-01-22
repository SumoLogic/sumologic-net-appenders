using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace SumoLogic.Logging.AspNetCore
{
    public static class LoggerFactoryExtensions
    {
        /// <summary>
        /// Adds a Sumo Logic logger named 'SumoLogic' to the factory.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        public static ILoggingBuilder AddSumoLogic(this ILoggingBuilder builder)
        {
            builder.Services.AddSingleton<ILoggerProvider, LoggerProvider>();
            return builder;
        }

        /// <summary>
        /// Adds a Sumo Logic logger named 'SumoLogic' to the factory.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        /// <param name="configure">Configure an instance of the <see cref="LoggerOptions" /> to set logging options</param>
        public static ILoggingBuilder AddSumoLogic(this ILoggingBuilder builder, Action<LoggerOptions> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }
            builder.AddSumoLogic();
            builder.Services.Configure(configure);

            return builder;
        }

        /// <summary>
        /// Adds a Sumo Logic logger named 'SumoLogic' to the factory.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        /// <param name="uri">Sets the uri of Sumo Logic ingesting endpoint/param>
        public static ILoggingBuilder AddSumoLogic(this ILoggingBuilder builder, string uri)
        {
            builder.AddSumoLogic(options => options.Uri = uri);
            return builder;
        }

    }
}
