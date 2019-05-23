using System;
using System.Collections.Generic;
using System.Linq;
using WatchCake.Data.BulitInParsers;
using WatchCake.Helpers;
using WatchCake.Models;

namespace WatchCake.DAL
{
    /// <summary>
    /// Centralized entities storage. Unit of work, provides single abstract access to all entities via contained concrete repositories.
    /// </summary>
    public class Storage
    {
        #region Singleton wiring
        private static Storage instance = null;
        private static readonly object padlock = new object();

        /// <summary>
        /// Singleton instance of the centralized Storage.
        /// </summary>
        public static Storage Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                        instance = new Storage();

                    return instance;
                }
            }
        }
        #endregion Singleton wiring

        #region Concrete Repositories
        public IRepo<Option> Options;
        public IRepo<Page> Pages;
        public IRepo<Shop> Shops;
        public IRepo<Snapshot> Snapshots;
        public IRepo<Tracker> Trackers;

        /// <summary>
        /// Cross-entity joint relation. Declared explicitly for support of virtual implementation.
        /// </summary>
        private IRepo<TrackedPage> TrackedPages;
        #endregion Concrete Repositories

        #region Static Constructor
        /// <summary>
        /// Static constructor. Initializes all repositories, fills virtual repositories.
        /// </summary>
        Storage()
        {
            var context = new WcDbContext();

            Options = new EFRepoBase<Option>(context);
            Pages = new EFRepoBase<Page>(context);
            Snapshots = new EFRepoBase<Snapshot>(context);
            Trackers = new EFRepoBase<Tracker>(context);

            TrackedPages = new EFRepoBase<TrackedPage>(context);

            Shops = new DictionaryRepoBase<Shop>();

            //Add all virtual shop parsers
            foreach (var entry in BuiltInParsers.List)
                Shops.Add(new Shop() { ID = entry.ID,  Parser = entry });
        }
        #endregion Static Constructor

        #region Page-Related
        /// <summary>
        /// Try retrieve stored page, that is similar to a provided one.
        /// </summary>
        public Page FirstSimilarPageOrDefault(Page page)
        {
            var storedPage = Pages.FirstOrDefault(p => p.RelativeUri == page.RelativeUri && p.ParentShopID == page.ParentShopID);

            if(storedPage != null)
                FillPageParentShop(storedPage);

            return storedPage;
        }

        /// <summary>
        /// Fills the ParentShop member of a page with an instance of a shop, corresponding to known ParentShopID.
        /// </summary>
        void FillPageParentShop(Page page) => page.ParentShop = Shops[(int)page.ParentShopID];

        /// <summary>
        /// For a set of pages, fills the ParentShop member of a page with an instance of a shop, corresponding to known ParentShopID.
        /// </summary>
        void FillPageParentShop(IEnumerable<Page> pages)
        {
            foreach (Page page in pages)
                FillPageParentShop(page);
        }
        
        #endregion Page-Related

        #region TrackedPages
        /// <summary>
        /// Stop having provided tracker, remove all single-related entites.
        /// </summary>
        public void UnregisterTracker(Tracker tracker)
        {
            foreach (Page page in ListAssociatedTrackedPages(tracker).ToList())
                RemovePageFromTracker(page, tracker);

            Trackers.Remove((int)tracker.ID);
        }
        
        /// <summary>
        /// Check whether provided page is tracked by the provided tracker.
        /// </summary>
        public bool IsBeingTracked(Page page, Tracker tracker) => TrackedPages.Exists(tp => tp.PageID == page.ID && tp.TrackerID == tracker.ID);        

        /// <summary>
        /// Start stracking the provded page by the provided tracker.
        /// </summary>
        public void StartTrackingPage(Page page, Tracker tracker)
        {
            TrackedPages.Add(new TrackedPage() { PageID = page.ID, TrackerID = tracker.ID });
        }

        /// <summary>
        /// Remove page for tracker forever, losing all tracking history.
        /// </summary>
        public void RemovePageFromTracker(Page page, Tracker tracker)
        {
            if (page?.ID == null || tracker?.ID == null)
                throw new ArgumentNullException("Cannot remove from tracking the undefined or unregistered page and/or tracker.");

            TrackedPage subject = TrackedPages.Single(tp => tp.PageID == page.ID && tp.TrackerID == tracker.ID);
            TrackedPages.Remove((int)subject.ID);

            var thisPageTrackedTimes = TrackedPages.Count(tp => tp.PageID == page.ID);

            if (thisPageTrackedTimes < 1)
                RemovePageCascade((int)page.ID);
        }


        /// <summary>
        /// Having list of pages, write/make-sure storage tracked-pages has correct IsTracked values.
        /// </summary>
        public void SaveIsTrackedIndicators(IEnumerable<Page> pages, Tracker tracker)
        {
            if (pages == null || tracker?.ID == null)
                throw new ArgumentNullException("Cannot ensure-dont-tracks of undefined pages and/or tracker.");

            foreach (Page page in pages)
            {
                if (page.IsTracked != null)//early quit
                {
                    if (page.ID == null)
                        throw new ArgumentNullException("Cannot ensure-is-track of unregistered page.");

                    TrackedPage asssociatedTrackedPage = TrackedPages.Single(tp => tp.PageID == page.ID && tp.TrackerID == tracker.ID);
                    asssociatedTrackedPage.IsTracked = (page.IsTracked == true);
                    TrackedPages.Update(asssociatedTrackedPage);
                }
            }
        }

        /// <summary>
        /// Having list of pages, fill them with correct IsTracked values from the storage.
        /// </summary>
        public void LoadIsTrackedIndicators(IEnumerable<Page> pages, Tracker tracker)
        {
            if (pages == null || tracker?.ID == null)
                throw new ArgumentNullException("Cannot ensure-dont-tracks of undefined pages and/or tracker.");

            foreach (Page page in pages)
            {
                if (page.ID == null)
                    throw new ArgumentNullException("Cannot ensure-dont-track of unregistered page.");

                TrackedPage asssociatedTrackedPage = TrackedPages.Single(tp => tp.PageID == page.ID && tp.TrackerID == tracker.ID);
                page.IsTracked = asssociatedTrackedPage.IsTracked;
            }
        }


        /// <summary>
        /// Removes everything associated with the page by the specified ID at storage.
        /// </summary>
        public void RemovePageCascade(int pageID)
        {
            var options = Options.List(o => o.ParentID == pageID).ToList();
            foreach (Option option in options)
            {
                var snapshots = Snapshots.List(s => s.ParentID == option.ID).ToList();
                foreach (Snapshot snapshot in snapshots)
                {
                    Snapshots.Remove((int)snapshot.ID);
                }
                Options.Remove((int)option.ID);
            }

            Pages.Remove(pageID);
        }


        /// <summary>
        /// Lists pages that are conceptually tracked, regardless of their 'DontTrack' parameter.
        /// </summary>
        public IEnumerable<Page> ListAssociatedTrackedPages(Tracker tracker)
        {
            var pages =  TrackedPages.List(tp => tp.TrackerID == tracker.ID).Select(tp => Pages[(int)tp.PageID]);
            FillPageParentShop(pages);
            return pages;
        }

        /// <summary>
        /// Lists pages that are effectively tracked, considering their 'DontTrack' parameter.
        /// </summary>
        public IEnumerable<Page> ListEffectivelyTrackedPages(Tracker tracker)
        {
            var pages = TrackedPages.List(tp => tp.TrackerID == tracker.ID && tp.IsTracked).Select(tp => Pages[(int)tp.PageID]);
            FillPageParentShop(pages);
            return pages;
        }


        /// <summary>
        /// Get a set of all options trat are belong to the pages, tracked by this traceker.
        /// </summary>
        public IEnumerable<Option> ListTrackedOptions(Tracker tracker, bool includeSnapshots = false)
        {
            var options = ListAssociatedTrackedPages(tracker).SelectMany(page => Options.List(option => option.ParentID == page.ID));

            if(includeSnapshots)
                foreach (Option option in options)
                    option.Snapshots = Snapshots.List(snapshot => snapshot.ParentID == option.ID);

            return options;
        }

        /// <summary>
        /// Populate the tracker with all passive properties actual for its view withuin a list.
        /// </summary>
        public void FillTrackerListProps(Tracker tracker)
        {
            tracker.ManualPageNumberIndicator = ListAssociatedTrackedPages(tracker).Count();

            var associatedOptions = ListTrackedOptions(tracker, includeSnapshots: true);

            tracker.AveragePriceEver = Money.GetAverage(associatedOptions.Select(aopt => aopt.CalculateWeightedMeanPrice()));

            var associatedSnapshots = associatedOptions.SelectMany(option => option.Snapshots);

            tracker.TodayPrice = associatedSnapshots.Where(s => s.Timestamp >= DateTime.Now.Date).Select(snapshot => snapshot.Price).Min();
            tracker.PriceDynamics = PriceMaths.CalculatePriceShift(tracker.AveragePriceEver, tracker.TodayPrice);

            var allPrices = associatedSnapshots.Select(snapshot => snapshot.Price);

            tracker.LowestPriceEver = allPrices.Min();           
        }

        /// <summary>
        /// Populate the tracker with all passive properties actual for its detailed view.
        /// </summary>
        public void FillTrackerDetailsProps(Tracker tracker)
        {
            tracker.Options = ListTrackedOptions(tracker, includeSnapshots: true);

            var effectivelyTrackedParentsIDs = ListEffectivelyTrackedPages(tracker).Select(etp => etp.ID);

            //getting fundamental props
            foreach (Option trackedOption in tracker.Options)
            {
                trackedOption.IsTracked = effectivelyTrackedParentsIDs.Any(etpid => etpid == trackedOption.ParentID);
            }
        }
        #endregion TrackedPages
    }
}
