using System.Collections.Generic;
using System.Runtime.Serialization;

using Intersect;
using Intersect.Plugins;

namespace Cheshire.Plugins.Client.Minimap.Configuration
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
        public Point MinimapTileSize { get; set; } = new Point(8, 8);

        /// <summary>
        /// Configures the minimum zoom level (0 - 100)
        /// </summary>
        public byte MinimumZoom { get; set; } = 0;

        /// <summary>
        /// Configures the maximum zoom level (0 - 100)
        /// </summary>
        public byte MaximumZoom { get; set; } = 100;

        /// <summary>
        /// Configures the default zoom level (0 - 100)
        /// </summary>
        public byte DefaultZoom { get; set; } = 65;

        /// <summary>
        /// Configures the amount to zoom by each step.
        /// </summary>
        public byte ZoomStep { get; set; } = 5;

        /// <summary>
        /// Configures the images used within the minimap. If any are left blank the system will default to its color.
        /// </summary>
        public Images Images { get; set; } = new Images();

        /// <summary>
        /// Configures the colours used within the minimap.
        /// </summary>
        public Colors Colors { get; set; } = new Colors();

        /// <summary>
        /// Configures which map layers the minimap will render.
        /// </summary>
        public List<string> RenderLayers { get; set; } = new List<string>();

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            Validate();
        }

        private void Validate()
        {
            if (MinimumZoom < 0 || MinimumZoom > 100)
            {
                MinimumZoom = 0;
            }

            if (MaximumZoom < 0 || MaximumZoom > 100)
            {
                MaximumZoom = 100;
            }

            if (DefaultZoom < 0 || DefaultZoom > 100)
            {
                DefaultZoom = 0;
            }

        }
    }

    public class Colors
    {
        public Color Player { get; set; } = Color.Cyan;

        public Color PartyMember { get; set; } = Color.Blue;

        public Color MyEntity { get; set; } = Color.Red;

        public Color Npc { get; set; } = Color.Orange;

        public Color Event { get; set; } = Color.Blue;

        public Dictionary<string, Color> Resource { get; set; } = new Dictionary<string, Color>() {
            { "None", Color.White }
        };

        public Color Default { get; set; } = Color.Magenta;
    }

    public class Images
    {
        public string Player { get; set; } = "minimap_player.png";

        public string PartyMember { get; set; } = "minimap_partymember.png";

        public string MyEntity { get; set; } = "minimap_me.png";

        public string Npc { get; set; } = "minimap_npc.png";

        public string Event { get; set; } = "minimap_event.png";

        public Dictionary<string, string> Resource { get; set; } = new Dictionary<string, string>() {
            { "None", "minimap_resource_none.png" }
        };

        public string Default { get; set; } = "minimap_npc.png";
    }
}
