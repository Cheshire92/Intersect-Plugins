using System;
using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Intersect.Client.Framework.Gwen.Control;

using Cheshire.Plugins.Utilities.Logging;

namespace Cheshire.Plugins.Utilities.Client.Interface
{
    public static class BaseExtensions
    {
        /// <summary>
        /// Load a Json UI file from the given location and apply it to the control.
        /// </summary>
        /// <param name="control">The <see cref="Base"/> control to apply this configuration to.</param>
        /// <param name="filePath">The file to load the configuration from.</param>
        /// <param name="saveOutput">Should the output configuration once loaded be saved?</param>
        public static void LoadJsonUI(this Base control, string filePath, bool saveOutput = true)
        {
            try
            {
                var obj = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(filePath));

                if (obj != null)
                {
                    control.LoadJson(obj);
                    control.ProcessAlignments();
                }

                if (obj == null)
                {
                    saveOutput = false;
                }

            }
            catch (Exception exception)
            {
                //Log JSON UI Loading Error
                if (Logger.Context != null)
                {
                    Logger.Write(LogLevel.Error, exception.Message);
                }
            }

            if (saveOutput)
            {
                control.SaveControlLayout(filePath);
            }
        }

        /// <summary>
        /// Save the Json UI file to the given location.
        /// </summary>
        /// <param name="control">The <see cref="Base"/> control to save the configuration for.</param>
        /// <param name="filePath">The file to save the configuration to.</param>
        public static void SaveControlLayout(this Base control, string filePath)
        {
            File.WriteAllText(filePath, control.GetJsonUI());
        }

    }
}
