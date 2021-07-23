using System;

using Cheshire.Plugins.Utilities.Logging;
using Cheshire.Plugins.ProfanityFilter.Configuration;
using Cheshire.Plugins.ProfanityFilter.Networking.Hooks;

using Intersect.Plugins;
using Intersect.Server.Plugins;
using Intersect.Network.Packets.Client;

namespace Cheshire.Plugins.ProfanityFilter
{
    public class PluginEntry : ServerPluginEntry
    {

        /// <inheritdoc/>
        public override void OnBootstrap(IPluginBootstrapContext context)
        {
            base.OnBootstrap(context);

            // Load our configuration into our static settings!
            PluginSettings.Settings = context.GetTypedConfiguration<PluginSettings>();

            // Set up our logging context for easy access!
            Logger.Context = context;
            Logger.WriteToConsole = PluginSettings.Settings.Config.WriteOutputToConsole;

            // Write to our console and log files to notify the user we are doing something.
            Logger.Write(LogLevel.Info, "*=====================================*");
            Logger.Write(LogLevel.Info, "*       Profanity Filter Plugin       *");
            Logger.Write(LogLevel.Info, "*=====================================*");
            Logger.Write(LogLevel.Info, String.Format("Name    : {0}", context.Manifest.Name));
            Logger.Write(LogLevel.Info, String.Format("Version : {0}", context.Manifest.Version));
            Logger.Write(LogLevel.Info, String.Format("Author  : {0}", context.Manifest.Authors));
            Logger.Write(LogLevel.Info, String.Format("Homepage: {0}", context.Manifest.Homepage));
            Logger.Write(LogLevel.Info, "---");

            // Register our packet hooks so that we can act before a chat message is processed!
            Logger.Write(LogLevel.Info, "Registering Packet Hooks..");
            if (PluginSettings.Settings.Config.FilterChatMessages)
            {
                if (!context.Packet.TryRegisterPacketPreHook<ChatMsgPacketPreHook, ChatMsgPacket>(out _))
                {
                    Logger.Write(LogLevel.Error, $"Failed to register {nameof(ChatMsgPacketPreHook)} packet pre-hook handler.");
                    Environment.Exit(-4);
                }
                else
                {
                    Logger.Write(LogLevel.Info, $"Registered {nameof(ChatMsgPacketPreHook)} packet pre-hook handler..");
                }
            }
            if (PluginSettings.Settings.Config.FilterCharacterNames)
            {
                if (!context.Packet.TryRegisterPacketPreHook<CreateCharacterPacketPreHook, CreateCharacterPacket>(out _))
                {
                    Logger.Write(LogLevel.Error, $"Failed to register {nameof(CreateCharacterPacketPreHook)} packet pre-hook handler.");
                    Environment.Exit(-4);
                }
                else
                {
                    Logger.Write(LogLevel.Info, $"Registered {nameof(CreateCharacterPacketPreHook)} packet pre-hook handler..");
                }
            }

            // Generate our filter regular expressions.
            Logger.Write(LogLevel.Info, $@"Generating ProfanityFilter Expressions..");
            ProfanityFilter.FilterCharacter = PluginSettings.Settings.Config.CensorCharacter;
            ProfanityFilter.CreateFilters(PluginSettings.Settings.ProfanityFilters);

            Logger.Write(LogLevel.Info, "Done!");
        }
        
        /// <inheritdoc/>
        public override void OnStart(IServerPluginContext context)
        {

        }
        
        /// <inheritdoc/>
        public override void OnStop(IServerPluginContext context)
        {

        }
    }
}
