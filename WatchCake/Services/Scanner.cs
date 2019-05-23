using System;
using System.Collections.Generic;
using System.Linq;
using WatchCake.DAL;
using WatchCake.Models;
using WatchCake.Parsers.Models;
using WatchCake.Services.FastWebNS;

namespace WatchCake.Services
{
    /// <summary>
    /// Infrastructure-level service of scanning the pages to update their options and underlying snapshots.
    /// </summary>
    public class Scanner
    {
        /// <summary>
        /// The parameter allowing for the parsed price data to be deviated by the specified amount.
        /// Works by intruding into the snapshot passover.
        /// </summary>
        public double? PriceFakeness
        {
            get => _priceFakeness;
            set
            {
                _priceFakeness = value;
                Logger.Log($"Scanner [{this.GetHashCode()}] " + (value == null ? "has stopped faking prices." :  $"has started deviating prices by {value}."));
            }
        }

        /// <summary>
        /// Centralized app storage access point.
        /// </summary>
        protected readonly Storage Storage = Storage.Instance;
        private double? _priceFakeness;

        /// <summary>
        /// Does scan of a single tracking, by requesting single page scan for every tracked page.
        /// </summary>
        public void SingleTrackerScan(Tracker tracker, DateTime? forcedTimestamp = null)
        {
            foreach (Page pageBeingTracked in Storage.ListEffectivelyTrackedPages(tracker))
            {
                SinglePageScan(pageBeingTracked, forcedTimestamp);
            }
        }

        /// <summary>
        /// Does scan of multiple trackers, avoiding redundant page rescans.
        /// </summary>
        public void MultiTrackerScan(IEnumerable<Tracker> trackers, DateTime? forcedTimestamp = null)
        {
            var uniquePages = trackers.SelectMany(tracker => Storage.ListEffectivelyTrackedPages(tracker)).Distinct();

            foreach (Page page in uniquePages)
            {
                SinglePageScan(page, forcedTimestamp);
            }
        }

        /// <summary>
        /// Parse a provided page.
        /// </summary>
        public PageParseResult ParsePage(Page page)
        {
            //if (page.ParentShop == null)
            //    page.ParentShop = Storage.Shops[(int)page.ParentShopID];

           return page.ParentShop.Parser.Parse(page.RelativeUri);
        }

        /// <summary>
        /// Fetches and stores fresh page data.
        /// </summary>
        public void SinglePageScan(Page page, DateTime? forcedTimestamp = null)
        {
            //Parse data
            PageParseResult pageParse = null;

            try
            {
                pageParse = ParsePage(page);
            }
            catch (FastWeb2NotFoundException)
            {
                return;
            }

            // Digesting page itself
            page.Title = pageParse.Title;
            page.ThumbPath = pageParse.ThumbPath;

            Random random = new Random();

            // Digesting options
            foreach (OptionParseResult optionParse in pageParse.OptionParseResults)
            {
                Option option = Storage.Options.SingleOrDefault(storedOpt => storedOpt.ParentID == page.ID && storedOpt.Code == optionParse.Code);

                //Ensure option is stored.
                if (option == null)
                {
                    option = new Option(optionParse)
                    {
                        Parent = page
                    };
                    Storage.Options.Add(option);
                }

                var snapshot = new Snapshot(optionParse, forcedTimestamp)
                {
                    Parent = option
                };

                //Variate the price if appropriate.
                if (PriceFakeness != null)
                {
                    var randomFullShift = (random.NextDouble() - 0.5) * 2;
                    snapshot.Price.Amount = (decimal)((double)snapshot.Price.Amount * (1 + (randomFullShift * (double)PriceFakeness)));
                }

                //Store snapshot.
                Storage.Snapshots.Add(snapshot);
            }
        }
    }
}
