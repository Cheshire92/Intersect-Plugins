using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Intersect.Plugins;
using Intersect.Client.Framework.Gwen;
using Intersect.Client.Framework.GenericClasses;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Cheshire.Plugins.Client.WebButtons.Configuration
{
    /// <summary>
    /// Our base plugin configuration class.
    /// </summary>
    public class PluginSettings : PluginConfiguration
    {
        public static PluginSettings Settings { get; set; }

        public List<ButtonBase> Buttons { get; set; } = new List<ButtonBase>();

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            Buttons = new List<ButtonBase>(Buttons.Distinct());
        }
    }

    /// <summary>
    /// A class defining our basic button configuration.
    /// </summary>
    public class ButtonBase
    {
        /// <summary>
        /// The name to use for this button and its Json UI file.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The Url to open when this button is clicked.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The location of the UI to place this button in.
        /// </summary>
        public string ParentControl { get; set; }

        /// <summary>
        /// The bounds of the button upon creation.
        /// </summary>
        public Rectangle Bounds { get; set; }

        /// <summary>
        /// The alignment of the button upon creation.
        /// </summary>
        [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
        public List<Alignments> Alignments { get; set; } = new List<Alignments>();

        /// <summary>
        /// Set the image for the button.
        /// </summary>
        public string Image { get; set; }

        /// <summary>
        /// Set the image for the button when hovered over.
        /// </summary>
        public string HoverImage { get; set; }

        /// <summary>
        /// Set the image for the button when clicked.
        /// </summary>
        public string ClickedImage { get; set; }

        /// <summary>
        /// Set the sound that plays when the button is hovered over.
        /// </summary>
        public string HoverSound { get; set; }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            Alignments = new List<Alignments>(Alignments.Distinct());
        }
    }

}
