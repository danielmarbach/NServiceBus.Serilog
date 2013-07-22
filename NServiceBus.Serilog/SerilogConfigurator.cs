﻿using NServiceBus.Logging;
using Serilog;

namespace NServiceBus.Serilog
{
    /// <summary>
    /// Used to forward NServiceBus logging message through Serilog
    /// </summary>
    public static class SerilogConfigurator
    {
        /// <summary>
        /// Configur NServiceBus logging messages to use Serilog
        /// </summary>
        /// <remarks>The default <see cref="Log.Logger"/> is used as the target for messages.</remarks>
        public static void Configure()
        {
            Configure(Log.Logger);
        }

        /// <summary>
        /// Configur NServiceBus logging messages to use Serilog
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to use as the target for messages.</param>
        public static void Configure(ILogger logger)
        {
            LogManager.LoggerFactory = new LoggerFactory(logger);
        }
    }
}