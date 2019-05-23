using System.Collections.Generic;
using WatchCake.Parsers;

namespace WatchCake.Data.BulitInParsers
{
    /// <summary>
    /// Static storage of the shop-page parsers. Is partial, sand contains PageParsers initializers, one per file.
    /// </summary>
    public static partial class BuiltInParsers
    {
        /// <summary>
        /// Complete list of currently known shop-page parsers.
        /// </summary>
        public static List<PageParser> List { get; set; }

        /// <summary>
        /// Default constructor, fill the List of shop-pages.
        /// </summary>
        static BuiltInParsers()
        {
            List = new List<PageParser>();

            //Reflect all defined page parsers and add to general list
            var pageParserFields = typeof(BuiltInParsers).GetFields();
            foreach (var property in pageParserFields)
                List.Add((PageParser)property.GetValue(null));

            //Add shop twins with alternative addresses.
            List.Add(PromUa.ProduceTwin(1001, "Zhyva", "https://zhyva.com.ua"));
            List.Add(PromUa.ProduceTwin(1002, "Novosad", "https://novosad-market.prom.ua/"));
            List.Add(PromUa.ProduceTwin(1003, "Smaragd", "https://smaragd.prom.ua/"));
            List.Add(PromUa.ProduceTwin(1004, "Gorshok", "https://gorshok.kiev.ua/"));
        }
    }
}
