using System;
using System.Text.RegularExpressions;

namespace WatchCake.Helpers
{
    /// <summary>
    /// Helpers for the uri manipulations.
    /// </summary>
    public static class UriExtensions
    {
        /// <summary>
        /// Removes uri scheme (http(s)://) from the provided uri.
        /// </summary>
        public static string StripScheme(this string uri)
        {
            string result = Regex.Replace(uri.ToString(), @"^(https?:\/\/)?", "");
            return result;
        }
        /// <summary>
        /// Removes uri scheme (http(s)://) from the provided uri.
        /// </summary>
        public static string StripScheme(this Uri uri) => uri?.ToString().StripScheme();


        /// <summary>
        /// Strips any trailing slashes from the provided uri string.
        /// </summary>
        public static string StripTrailingSlashes(this string uri)
        {
            string result = Regex.Replace(uri, @"(\/+)$", "");
            return result;
        }
        /// <summary>
        /// Strips any trailing slashes from the provided uri string.
        /// </summary>
        public static string StripTrailingSlashes(this Uri uri) => uri.ToString().StripTrailingSlashes();


        /// <summary>
        /// Strips any leading slashes from the provided uri string.
        /// </summary>
        public static string StripLeadingSlashes(this string uri)
        {
            string result = Regex.Replace(uri, @"^(\/+)", "");
            return result;
        }
        /// <summary>
        /// Strips any leading slashes from the provided uri string.
        /// </summary>
        public static string StripLeadingSlashes(this Uri uri) => uri.ToString().StripTrailingSlashes();


        /// <summary>
        /// Concatenates Uri segments placing strictly single slash between them.
        /// </summary>
        public static string SlashSafeUriConcat(this string a, string b)
        {
            return a.StripTrailingSlashes() + "/" + b.StripLeadingSlashes();
        }
        /// <summary>
        /// Concatenates Uri segments placing strictly single slash between them.
        /// </summary>
        public static string SlashSafeUriConcat(this Uri a, Uri b) => a.ToString().SlashSafeUriConcat(b.ToString());
    }
}
