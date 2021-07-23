using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Cheshire.Plugins.ProfanityFilter
{
    public static class ProfanityFilter
    {
        private static List<string> mFilters;

        public static char FilterCharacter = '*';

        /// <summary>
        /// Creates the internal Regex filters used to filter naughty words.
        /// </summary>
        /// <param name="words">The list of words to create filters from.</param>
        public static void CreateFilters(List<string> words)
        {
            mFilters = words.Select(word => ToRegexPattern(word)).ToList();
        }

        /// <summary>
        /// Filter the provided string with the provided list.
        /// </summary>
        /// <param name="dirtyString">The dirty string that is to be filtered by the profanity filter.</param>
        /// <returns>Returns a clean string after filtering the dirty string with the provided list.</returns>
        public static string Apply(string dirtyString)
        {
            // Do we have something to filter?
            if (string.IsNullOrWhiteSpace(dirtyString))
            {
                return dirtyString;
            }

            // Go through our list of words to filter and take them all out!
            var filteredString = dirtyString;
            foreach (var filter in mFilters)
            {
                filteredString = Regex.Replace(filteredString, filter, StarCensoredMatch, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            }

            return filteredString;
        }

        /// <summary>
        /// Checks whether a string contains a word that would be filtered by the provided list.
        /// </summary>
        /// <param name="dirtyString">The dirty string that is to be filtered by the profanity filter.</param>
        /// <returns>Returns whether the string contains a filtered word or not.</returns>
        public static bool HasFilteredWords(string dirtyString)
        {
            // Do we have something to filter?
            if (string.IsNullOrWhiteSpace(dirtyString))
            {
                return false;
            }

            // If we filter our string, does it change?
            if (Apply(dirtyString) != dirtyString)
            {
                return true;
            }

            return false;
        }

    #region Private Methods
    // Credit goes to https://github.com/jamesmontemagno for these two methods.
    // His Censorship class was an inspiration to this, but felt like it could be handled a little more efficiently for Intersect.
    static string StarCensoredMatch(Group m) => new string(FilterCharacter, m.Captures[0].Value.Length);

         static string ToRegexPattern(string wildcardSearch)
         {
             var regexPattern = Regex.Escape(wildcardSearch);

             regexPattern = regexPattern.Replace(@"\*", ".*?");
             regexPattern = regexPattern.Replace(@"\?", ".");

             if (regexPattern.StartsWith(".*?", StringComparison.Ordinal))
             {
                 regexPattern = regexPattern.Substring(3);
                 regexPattern = @"(^\b)*?" + regexPattern;
             }

             regexPattern = @"\b" + regexPattern + @"\b";

             return regexPattern;
         }

#endregion

    }
}
