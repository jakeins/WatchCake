using System;
using WatchCake.Models.Bases;
using WatchCake.Parsers;

namespace WatchCake.Models
{
    /// <summary>
    /// Domain-centric Shop wrapper of the PageParser.
    /// </summary>
    public class Shop : IdObject
    {
        /// <summary>
        /// Shop display name.
        /// </summary>
        public string Name => Parser.Name;

        /// <summary>
        /// Shop base uri.
        /// </summary>
        public Uri Domain => Parser.Domain;

        /// <summary>
        /// The extraction/parsing plan for the shop, single instance.
        /// </summary>
        public PageParser Parser { get; set; }
    }
}
