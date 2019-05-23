using System;
using WatchCake.Helpers;

namespace WatchCake.Parsers.Models
{
    /// <summary>
    /// Representation of the parsed product option data.
    /// </summary>
    public class OptionParseResult 
    {
        /// <summary>
        /// Internal retailer site identifier of the specified product.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// One of two basic product options, usually the color.
        /// </summary>
        public string PropertyA { get; set; }

        /// <summary>
        /// One of two basic product options, usually the size.
        /// </summary>
        public string PropertyB { get; set; }

        /// <summary>
        /// Additional option.
        /// </summary>
        public string PropertyC { get; set; }

        /// <summary>
        /// Site's description of the stock/availability status.
        /// May be non applicable since sites often do not list inavailable options.
        /// </summary>
        public string StockStatus { get; set; }

        /// <summary>
        /// Price.
        /// </summary>
        public Money Price { get; set; }
    }
}