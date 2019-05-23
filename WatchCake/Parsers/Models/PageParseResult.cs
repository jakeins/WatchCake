using System;
using System.Collections.Generic;

namespace WatchCake.Parsers.Models
{
    /// <summary>
    /// Representation of the parsed product page data.
    /// </summary>
    public class PageParseResult 
    {
        /// <summary>
        /// Exttracted title of the page.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Product thumbnail path.
        /// </summary>
        public string ThumbPath { get; set; }

        /// <summary>
        /// All option of this page.
        /// </summary>
        public List<OptionParseResult> OptionParseResults { get; } = new List<OptionParseResult>();
    }
}