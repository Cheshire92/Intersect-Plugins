using System;

using Intersect.Plugins;

namespace Cheshire.Plugins.Utilities.Logging
{
    public static class Logger
    {
        public static IPluginBaseContext Context { get; set; }

        public static bool WriteToConsole { get; set; }

        public static void Write(LogLevel level, string message)
        {
            if (WriteToConsole)
            {
                Console.WriteLine(message);
            }

            switch (level)
            {
                case LogLevel.Error:
                    Context.Logging.Plugin.Error(message);
                    break;

                case LogLevel.Info:
                    Context.Logging.Plugin.Info(message);
                    break;

                case LogLevel.Warning:
                    Context.Logging.Plugin.Warn(message);
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
