using System.Collections.Generic;
using System.Runtime.Serialization;

using Intersect.Plugins;

using Intersect;


namespace Cheshire.Plugins.Client.WebButtons.Configuration
{
    /// <summary>
    /// Our base plugin configuration class.
    /// </summary>
    public class PluginSettings : PluginConfiguration
    {
        public static PluginSettings Settings { get; set; }

        /// <summary>
        /// Configures the size at which each minimap tile is rendered.
        /// </summary>
        public Point MinimapTileSize { get; set; } = new Point(1, 1);

        /// <summary>
        /// Configures the colours used within the minimap.
        /// </summary>
        public Colors Colors = new Colors();

        /// <summary>
        /// Configures which map layers the minimap will render.
        /// </summary>
        public List<string> RenderLayers = new List<string>();

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            
        }
    }

    public class Colors
    {
        public Color Player { get; set; } = Color.Cyan;

        public Color MyEntity { get; set; } = Color.Red;

        public Color Npc { get; set; } = Color.Orange;

        public Color Event { get; set; } = Color.Blue;

        public Color Resource { get; set; } = Color.LightCoral;

        public Color Default { get; set; } = Color.Magenta;
    }
}
