using System;
using Microsoft;

using Intersect.Plugins;
using Intersect.Client.General;
using Intersect.Client.Plugins;
using Intersect.Client.Plugins.Interfaces;

using Cheshire.Plugins.Utilities.Logging;
using Cheshire.Plugins.Client.WebButtons.Configuration;
using System.IO;
using Intersect.Client.Framework.Graphics;

namespace Cheshire.Plugins.Client.Minimap
{
    
    public class PluginEntry : ClientPluginEntry
    {
        private Minimap mMinimap;

        private bool mInitialized = false;

        /// <inheritdoc />
        public override void OnBootstrap([ValidatedNotNull] IPluginBootstrapContext context)
        {
            // Load our configuration into our static settings!
            PluginSettings.Settings = context.GetTypedConfiguration<PluginSettings>();

            // Set up our logging context for easy access!
            Logger.Context = context;
            Logger.WriteToConsole = false;

            // Write to our log files to notify the user we are doing something.
            Logger.Write(LogLevel.Info, "*======================================*");
            Logger.Write(LogLevel.Info, "*            Minimap Plugin            *");
            Logger.Write(LogLevel.Info, "*======================================*");
            Logger.Write(LogLevel.Info, String.Format("Name    : {0}", context.Manifest.Name));
            Logger.Write(LogLevel.Info, String.Format("Version : {0}", context.Manifest.Version));
            Logger.Write(LogLevel.Info, String.Format("Author  : {0}", context.Manifest.Authors));
            Logger.Write(LogLevel.Info, String.Format("Homepage: {0}", context.Manifest.Homepage));
            Logger.Write(LogLevel.Info, "---");
        }

        /// <inheritdoc />
        public override void OnStart([ValidatedNotNull] IClientPluginContext context)
        {
            // Load our assets, we'll need them later.
            Logger.Write(LogLevel.Info, "Loading Minimap..");
            mMinimap = new Minimap(context, PluginSettings.Settings.MinimapTileSize.X, PluginSettings.Settings.MinimapTileSize.X, Path.GetDirectoryName(context.Assembly.Location));
            Logger.Write(LogLevel.Info, "Done!");

            context.Lifecycle.LifecycleChangeState += HandleLifecycleChangeState;
            context.Lifecycle.GameUpdate += HandleGameUpdate;
            context.Lifecycle.GameDraw += HandleGameDraw;
        }

        private void HandleGameDraw(IClientPluginContext context, GameDrawArgs drawGameArgs)
        {
            if (drawGameArgs.State == DrawStates.FringeLayers)
            {
                mMinimap.Draw();
            }
        }

        /// <inheritdoc />
        public override void OnStop([ValidatedNotNull] IClientPluginContext context)
        {

        }
        
        private void HandleGameUpdate([ValidatedNotNull] IClientPluginContext context,
            [ValidatedNotNull] GameUpdateArgs gameUpdateArgs)
        {
            if (gameUpdateArgs.State == GameStates.InGame)
            {
                if (!mInitialized)
                {
                    mMinimap.Initialize();
                    mInitialized = true;
                }
                mMinimap.Update(gameUpdateArgs.Player, gameUpdateArgs.KnownEntities);
            }
            else
            {
                if (mInitialized)
                {
                    mInitialized = false;
                }
            }
        }
        
        private void HandleLifecycleChangeState([ValidatedNotNull] IClientPluginContext context,
            [ValidatedNotNull] LifecycleChangeStateArgs lifecycleChangeStateArgs)
        {
            var activeInterface = context.Lifecycle.Interface;
            if (activeInterface == null)
            {
                return;
            }

            if (lifecycleChangeStateArgs.State == GameStates.InGame)
            {
                if (mInitialized)
                {
                    mInitialized = false;
                }
            }
        }

    }
}