using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Intersect.Plugins;

using Newtonsoft.Json;

namespace Cheshire.Plugins.ProfanityFilter.Configuration
{
    /// <summary>
    /// Our base plugin configuration class.
    /// </summary>
    public class PluginSettings : PluginConfiguration
    {
        public static PluginSettings Settings { get; set; }

        /// <summary>
        /// A list of words and matches to filter chat messages against for profanity.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> ProfanityFilters { get; set; } = new List<string>() { "" };

        /// <summary>
        /// Configuration for all the strings this Plugin uses.
        /// </summary>
        public Strings Strings { get; set; } = new Strings();

        /// <summary>
        /// All configuration switches for this plugin.
        /// </summary>
        public Config Config { get; set; } = new Config();

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            ProfanityFilters = new List<string>(ProfanityFilters.Distinct());
        }
    }
    
    /// <summary>
    /// A class defining all of our plugin strings used for translation.
    /// </summary>
    public class Strings
    {
        /// <summary>
        /// The error message we send a client upon rejecting a chosen username.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CharacterCreationError = @"The chosen name does not meet requirements set by the server.";
    }

    /// <summary>
    /// A class defining all of our plugin configuration switches.
    /// </summary>
    public class Config
    {
        /// <summary>
        /// Determines whether the logging output of this plugin is displayed in the server console.
        /// </summary>
        public bool WriteOutputToConsole { get; set; } = true;

        /// <summary>
        /// Determines whether chat messages are filtered by this plugin.
        /// </summary>
        public bool FilterChatMessages { get; set; } = true;

        /// <summary>
        /// Determines whether Character Names are filtered by this plugin.
        /// </summary>
        public bool FilterCharacterNames { get; set; } = true;

        /// <summary>
        /// Determines the character used to censor bad words with.
        /// </summary>
        public char CensorCharacter { get; set; } = '*';
    }

}
