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
        /// <summary>
        /// A list of words and matches to filter chat messages against for profanity.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> ProfanityFilters { get; set; } = new List<string>() { "" };

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            ProfanityFilters = new List<string>(ProfanityFilters.Distinct());
        }
    }
    
}
