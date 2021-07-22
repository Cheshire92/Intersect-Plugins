using System;

using Cheshire.Plugins.ProfanityFilter.Configuration;
using Cheshire.Plugins.ProfanityFilter.Networking.Hooks;
using Cheshire.Plugins.ProfanityFilter.Utilities;

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

            // Write to our console and log files to notify the user we are doing something.
            Logger.Write(context, LogLevel.Info, "*=====================================*");
            Logger.Write(context, LogLevel.Info, "*       Profanity Filter Plugin       *");
            Logger.Write(context, LogLevel.Info, "*=====================================*");
            Logger.Write(context, LogLevel.Info, String.Format("Name    : {0}", context.Manifest.Name));
            Logger.Write(context, LogLevel.Info, String.Format("Version : {0}", context.Manifest.Version));
            Logger.Write(context, LogLevel.Info, String.Format("Author  : {0}", context.Manifest.Authors));
            Logger.Write(context, LogLevel.Info, String.Format("Homepage: {0}", context.Manifest.Homepage));
            Logger.Write(context, LogLevel.Info, "---");

            // Register our packet hooks so that we can act before a chat message is processed!
            Logger.Write(context, LogLevel.Info, "Registering Packet Hooks..");
            if (!context.Packet.TryRegisterPacketPreHook<ChatMsgPacketPreHook, ChatMsgPacket>(out _))
            {
                Logger.Write(context, LogLevel.Error, $"Failed to register {nameof(ChatMsgPacketPreHook)} packet pre-hook handler.");
                Environment.Exit(-4);
            }
            Logger.Write(context, LogLevel.Info, "Done!");

            // Generate our filter regular expressions.
            Logger.Write(context, LogLevel.Info, $@"Generating ProfanityFilter Expressions..");
            ProfanityFilter.CreateFilters(context.GetTypedConfiguration<PluginSettings>()?.ProfanityFilters);
            Logger.Write(context, LogLevel.Info, "Done!");

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
