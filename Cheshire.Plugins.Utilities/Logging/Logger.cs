using System;

using Intersect.Plugins;
using Intersect.Server.Plugins;

namespace Cheshire.Plugins.Utilities.Logging
{
    public static class Logger
    {
        public static void Write(IPluginBootstrapContext context, LogLevel level, string message, bool writeToConsole = true)
        {
            if (writeToConsole)
            {
                Console.WriteLine(message);
            }

            switch (level)
            {
                case LogLevel.Error:
                    context.Logging.Plugin.Error(message);
                    break;

                case LogLevel.Info:
                    context.Logging.Plugin.Info(message);
                    break;

                case LogLevel.Warning:
                    context.Logging.Plugin.Warn(message);
                    break;
            }         
        }

        public static void Write(IServerPluginContext context, LogLevel level, string message, bool writeToConsole = true)
        {
            if (writeToConsole)
            {
                Console.WriteLine(message);
            }

            switch (level)
            {
                case LogLevel.Error:
                    context.Logging.Plugin.Error(message);
                    break;

                case LogLevel.Info:
                    context.Logging.Plugin.Info(message);
                    break;

                case LogLevel.Warning:
                    context.Logging.Plugin.Warn(message);
                    break;
            }
        }
    }



    public enum LogLevel
    {
        Info,
        Warning,
        Error
    }
}
