using System;
using WatchCake.Helpers;
using WatchCake.Models.Bases;
using WatchCake.Parsers.Models;

namespace WatchCake.Models
{
    /// <summary>
    /// The state of the product option in a geven moment in time.
    /// </summary>
    public class Snapshot : ChildIdObject<Option>
    {
        /// <summary>
        /// When snapshot was taken.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Site's description of the stock/availability status.
        /// May be non applicable since sites often do not list inavailable options.
        /// </summary>
        public string StockStatus { get; set; }

        /// <summary>
        /// Original site price.
        /// </summary>
        public Money Price { get; set; }


        /// <summary>
        /// Parameterless constuctor for automatic reconstitution.
        /// </summary>
        public Snapshot()
        {
        }

        /// <summary>
        /// Initializes option Snapshot with the data from OptionParse object.
        /// </summary>
        public Snapshot(OptionParseResult optionParse, DateTime? forcedTimestamp = null)
        {
            Timestamp = forcedTimestamp ?? DateTime.Now;
            StockStatus = optionParse.StockStatus;
            Price = optionParse.Price;
        }
    }
}