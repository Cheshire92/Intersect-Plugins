using System;
using System.IO;
using Microsoft;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

using Intersect.Plugins;
using Intersect.Client.General;
using Intersect.Client.Plugins;
using Intersect.Client.Interface;
using Intersect.Client.Framework.Graphics;
using Intersect.Client.Framework.Gwen.Control;
using Intersect.Client.Plugins.Interfaces;

using Cheshire.Plugins.Utilities.Logging;
using Cheshire.Plugins.Client.WebButtons.Extensions;
using Cheshire.Plugins.Client.WebButtons.Configuration;

namespace Cheshire.Plugins.Client.WebButtons
{
    
    public class PluginEntry : ClientPluginEntry
    {
        public string AssetPath { get; set; }

        public Dictionary<string, GameTexture> Images = new Dictionary<string, GameTexture>();

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
            Logger.Write(LogLevel.Info, "*          Web Buttons Plugin          *");
            Logger.Write(LogLevel.Info, "*======================================*");
            Logger.Write(LogLevel.Info, String.Format("Name    : {0}", context.Manifest.Name));
            Logger.Write(LogLevel.Info, String.Format("Version : {0}", context.Manifest.Version));
            Logger.Write(LogLevel.Info, String.Format("Author  : {0}", context.Manifest.Authors));
            Logger.Write(LogLevel.Info, String.Format("Homepage: {0}", context.Manifest.Homepage));
            Logger.Write(LogLevel.Info, "---");

            // Check whether our asset path exists!
            Logger.Write(LogLevel.Info, "Checking for asset path location..");
            AssetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources", "plugins", "Cheshire.Plugins.Client.WebButtons", "Assets");
            if (!Directory.Exists(AssetPath))
            {
                Logger.Write(LogLevel.Info, "Creating asset path..");
                Directory.CreateDirectory(AssetPath);
            }
            Logger.Write(LogLevel.Info, "Done!");
        }

        /// <inheritdoc />
        public override void OnStart([ValidatedNotNull] IClientPluginContext context)
        {
            // Load our assets, we'll need them later.
            Logger.Write(LogLevel.Info, "Loading assets..");
            LoadAssets(context);
            Logger.Write(LogLevel.Info, "Done!");

            context.Lifecycle.LifecycleChangeState += HandleLifecycleChangeState;
            context.Lifecycle.GameUpdate += HandleGameUpdate;
        }

        private void LoadAssets(IClientPluginContext context)
        {
            // Go through each button in our configuration and load the assets!
            foreach (var button in PluginSettings.Settings.Buttons)
            {
                if (button.Image != null && button.Image.Length > 0 && !Images.ContainsKey(button.Image))
                {
                    Logger.Write(LogLevel.Info, $"Loading asset {button.Image}");
                    var asset = context.ContentManager.Load<GameTexture>(Intersect.Client.Framework.Content.ContentTypes.Interface, Path.Combine(AssetPath, button.Image));
                    if (asset != null)
                    {
                        Images.Add(button.Image, asset);
                    }
                    else
                    {
                        Logger.Write(LogLevel.Error, $"Could not load asset {button.Image}! Check if the file exists?");
                    }
                }
            
                if (button.HoverImage != null && button.HoverImage.Length > 0 && !Images.ContainsKey(button.HoverImage))
                {
                    Logger.Write(LogLevel.Info, $"Loading asset {button.HoverImage}");
                    var asset = context.ContentManager.Load<GameTexture>(Intersect.Client.Framework.Content.ContentTypes.Interface, Path.Combine(AssetPath, button.HoverImage));
                    if (asset != null)
                    {
                        Images.Add(button.HoverImage, asset);
                    }
                    else
                    {
                        Logger.Write(LogLevel.Error, $"Could not load asset {button.HoverImage}! Check if the file exists?");
                    }
                }
            
                if (button.ClickedImage != null && button.ClickedImage.Length > 0 && !Images.ContainsKey(button.ClickedImage))
                {
                    Logger.Write(LogLevel.Info, $"Loading asset {button.ClickedImage}");
                    var asset = context.ContentManager.Load<GameTexture>(Intersect.Client.Framework.Content.ContentTypes.Interface, Path.Combine(AssetPath, button.ClickedImage));
                    if (asset != null)
                    {
                        Images.Add(button.ClickedImage, asset);
                    }
                    else
                    {
                        Logger.Write(LogLevel.Error, $"Could not load asset {button.ClickedImage}! Check if the file exists?");
                    }
                }
            }
        }

        /// <inheritdoc />
        public override void OnStop([ValidatedNotNull] IClientPluginContext context)
        {

        }
        
        private void HandleGameUpdate([ValidatedNotNull] IClientPluginContext context,
            [ValidatedNotNull] GameUpdateArgs gameUpdateArgs)
        {

        }
        
        private void HandleLifecycleChangeState([ValidatedNotNull] IClientPluginContext context,
            [ValidatedNotNull] LifecycleChangeStateArgs lifecycleChangeStateArgs)
        {
            var activeInterface = context.Lifecycle.Interface;
            if (activeInterface == null)
            {
                return;
            }

            Logger.Write(LogLevel.Error, $"Generating new interface elements after changing lifecycle!");
            GenerateControls(activeInterface);
        }

        private void GenerateControls(IMutableInterface activeInterface)
        {
            // Go through each of our buttons for the current state of the game and create them!
            foreach (var control in PluginSettings.Settings.Buttons)
            {
                Logger.Write(LogLevel.Error, $"Attempting to generate {control.Name} on {control.ParentControl}..");
                // Get the parent control that we want to create our button onn.
                var parentControl = activeInterface.Children.FindByName(control.ParentControl);
                if (parentControl != null)
                {
                    // Create our new button, set its values!
                    var button = new Button(parentControl, control.Name);
                    button.SetBounds(control.Bounds);

                    foreach (var alignment in control.Alignments)
                    {
                        button.AddAlignment(alignment);
                    }
                    button.ProcessAlignments();

                    // Set the images if they exists.
                    if (control.Image != null && control.Image.Length > 0 && Images.ContainsKey(control.Image))
                    {
                        button.SetImage(Images[control.Image], control.Image, Button.ControlState.Normal);
                    }
                    if (control.HoverImage != null && control.HoverImage.Length > 0 && Images.ContainsKey(control.HoverImage))
                    {
                        button.SetImage(Images[control.HoverImage], control.HoverImage, Button.ControlState.Hovered);
                    }
                    if (control.ClickedImage != null && control.ClickedImage.Length > 0 && Images.ContainsKey(control.ClickedImage))
                    {
                        button.SetImage(Images[control.ClickedImage], control.ClickedImage, Button.ControlState.Clicked);
                    }

                    // Set the sounds if configured.
                    if (control.HoverSound != null && control.HoverSound.Length > 0)
                    {
                        button.SetHoverSound(control.HoverSound);
                    }

                    // Set our click action to be opening a webbrowser.
                    button.Clicked += (sender, args) => {
                        if (string.IsNullOrWhiteSpace(control.Url))
                        {
                            Logger.Write(LogLevel.Error, $@" {control.Name} configuration property {control.Url} is null/empty/whitespace.");
                            return;
                        }
                        
                        Process.Start(control.Url);
                    };

                    Logger.Write(LogLevel.Error, $"Done!");
                }
                else
                {
                    Logger.Write(LogLevel.Error, $"Parent control {control.ParentControl} not found! Wrong state or spelling error?");
                }
            }
        }
    }
}