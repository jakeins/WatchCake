using WatchCake.Models;
using WatchCake.Services;
using WatchCake.DAL;
using System.Collections.Generic;
using System;
using WatchCake.Parsers.Models;
using System.Collections.ObjectModel;
using WatchCake.Helpers;
using System.Net;
using System.Linq;
using System.Text.RegularExpressions;
using System.Globalization;

namespace WatchCake.ViewModels
{
    /// <summary>
    /// Tracker Modification (New/Edit) Window ViewModel.
    /// </summary>
    public class TrackerEditViewModel : AppViewModel
    {
        /// <summary>
        /// Bulit-in Track-It logic result state enumeration.
        /// </summary>
        public enum TrackStatus
        {
            /// <summary>
            /// The page by the provided url is successfully tracked.
            /// </summary>
            Success,

            /// <summary>
            /// The system is not familiar with the shop by the provided url.
            /// </summary>
            UnknownShop,

            /// <summary>
            /// There are problems in accessing site by the provided url.
            /// </summary>
            AccessFail,

            /// <summary>
            /// The content of the provided url received, but there are problems with interpeting it.
            /// </summary>
            ContentFail,

            /// <summary>
            /// The provided link is the effective copy of another already tracked here page.
            /// </summary>
            DuplicateFail
        }
        
        /// <summary>
        /// The instance of tracke being modified.
        /// </summary>
        public Tracker Tracker { get; set; }

        /// <summary>
        /// Flag of the modification mode. True for new tracker, false for edit tracker.
        /// </summary>
        public bool NewTrackerMode { get; set; }

        /// <summary>
        /// Indicative name of the current edit mode. Can be used as a window title.
        /// </summary>
        public string ModeDisplay => (NewTrackerMode ? "New" : "Edit") + " Tracker";

        /// <summary>
        /// The reference for the currently selected page. Useful for WPF DataGrid.
        /// </summary>
        public Page SelectedPage { get; set; }

        private bool _trackerTitlePinned;
        /// <summary>
        /// Flag of title source. If pinned, its the user input. Otherwise, is automatically generated.
        /// </summary>
        public bool TrackerTitlePinned
        {
            get => _trackerTitlePinned;
            set
            {
                _trackerTitlePinned = value;
                RaisePropertyChanged();

                if (!value)
                    RegenerateTrackerTitle();
            }
        }

        /// <summary>
        /// Event marking a commit of the edited tracker. Allows subscribing by the caller window and reacting appropriately.
        /// </summary>
        public event Action FinishCommit;

        /// <summary>
        /// Constructor of the tracker modification window. Uses input ID for starting editing existing tracker. Having input nothing/null creates new tracker.
        /// </summary>
        public TrackerEditViewModel(int? trackerID)
        {
            if (trackerID == null)
            {
                NewTrackerMode = true;

                //initialize fresh tracker
                Tracker = new Tracker()
                {
                    Pages = new ObservableCollection<Page>()
                };                
            }
            else
            {
                NewTrackerMode = false;
                TrackerTitlePinned = true;

                //load tracker from storage, populate accordingly
                Tracker = Storage.Trackers[(int)trackerID];
                Tracker.Pages = new ObservableCollection<Page>(Storage.ListAssociatedTrackedPages(Tracker));
                Storage.LoadIsTrackedIndicators(Tracker.Pages, Tracker);                
            }
        }   

        /// <summary>
        /// Main page tracking start logic. Works locally(virtually), nothing gets stored from this method.
        /// </summary>
        public TrackStatus TrackIt(string url)
        {
            //retrieving matching shop
            Shop matchingShop = Storage.Shops.SingleOrDefault(shop => url.Contains(shop.Domain.StripScheme().StripTrailingSlashes()));

            //failure report if nothing retrieved
            if (matchingShop == null)
                return TrackStatus.UnknownShop;

            //getting relative segment
            string stripSchemed = matchingShop.Domain.StripScheme().StripTrailingSlashes();
            var relativeUrl = url.Substring(url.IndexOf(stripSchemed) + stripSchemed.Length).StripLeadingSlashes();

            Logger.Log($"[{relativeUrl}] at [{matchingShop.Domain}]");

            //Preparing new Page instance.
            Page page = new Page() { ParentShop = matchingShop, RelativeUri = relativeUrl };

            //Early quit if this page is already locally(virtually) present.
            if (this.Tracker.HasLocalDuplicates(page))
                return TrackStatus.DuplicateFail;

            //Try to perform a parse. Return appropriate local status in case of failure.
            try
            {
                PageParseResult pageParse = Scanner.ParsePage(page);
                page.Title = pageParse.Title;
                page.IsTracked = true;
            }
            catch (WebException)
            {
                return TrackStatus.AccessFail;
            }
            catch (NullReferenceException)
            {
                return TrackStatus.ContentFail;
            }           

            //Having succsessfully parsed page by now, add it to the local tracker.
            this.Tracker.Pages.Add(page);

            //call for tracker title regeneration, if allowed
            if (!TrackerTitlePinned)
                RegenerateTrackerTitle();

            Logger.Log("Added " + page.Title + ", total: " + Tracker.Pages.Count);

            return TrackStatus.Success;
        }

        /// <summary>
        /// Remove page from current tracker locally.
        /// </summary>
        public void LocalRemovePageFromTracking(Page page)
        {
            Tracker.Pages.Remove(page);

            if (!TrackerTitlePinned)
                RegenerateTrackerTitle();
        }

        /// <summary>
        /// Geenrate title basing on existing page titles. Gets statistically popular words of the shortest title.
        /// </summary>
        public void RegenerateTrackerTitle()
        {
            if (Tracker.Pages.Count == 0)
                Tracker.Title = null;
            else
            {
                //initialize satticstics sets
                var wordedNames = new List<List<string>>();
                var allwords = new List<string>();

                //Textinfo instance, used for convering string inro title (proper) case.
                var titleCaser = new CultureInfo("en-US", false).TextInfo;

                //go through all local pages, split each title into capitalized words and add to statistic sets.
                for (int i = 0; i < Tracker.Pages.Count; i++)
                {
                    var cleanTitle = Regex.Replace(Tracker.Pages[i].Title, @"[^a-zA-Z\d]+", " ");
                    cleanTitle = Regex.Replace(cleanTitle, @"\s+", " ");
                    var wordedName = new List<string>(cleanTitle.Split(' ').Select(w => titleCaser.ToTitleCase(w)));
                    wordedNames.Add(wordedName);
                    allwords.AddRange(wordedName);
                }

                var shortestWordedName = wordedNames.OrderBy(ws => ws.Count).First();

                List<string> subjectWordedName = new List<string>();

                //filtering the words of the shortest title
                foreach (string word in shortestWordedName)
                    if (allwords.Count(w => w == word) > 1)
                        subjectWordedName.Add(word);

                //glue lefteover words together
                string generatedName = string.Join(" ", subjectWordedName);

                //in case of unsatisfactory result, just return the shortest title
                if (generatedName.Length < 3)
                    generatedName = string.Join(" ", shortestWordedName);

                //apply to the actual title var
                Tracker.Title = generatedName;
            }
        }

        /// <summary>
        /// Digest the modifications and store the results.
        /// </summary>
        internal void CommitTracker()
        {
            if (this.Tracker.Pages.Count < 1)
                throw new InvalidOperationException("Cannot commit the new tracker that has no pages.");

            //add tracker to the storage if its new mode
            if(NewTrackerMode)
                Storage.Trackers.Add(this.Tracker);

            List<Page> untrackVictimList = Storage.ListAssociatedTrackedPages(Tracker).ToList();
            List<Page> ultimatePages = new List<Page>();

            //going through candidate pages
            foreach (Page candidatePage in this.Tracker.Pages)
            {
                Page ultimatePage = candidatePage;

                if (ultimatePage.ID == null) // meaning this is a newly added page
                {
                    //retrieving effectively equal from storage
                    if (Storage.FirstSimilarPageOrDefault(candidatePage) is Page similarStoredpage)
                        ultimatePage = similarStoredpage;
                    else
                        Storage.Pages.Add(candidatePage);//or pushing new page into storage
                }

                ultimatePages.Add(ultimatePage);

                // Ensure the page is getting tracked by a tracker.
                if (!Storage.IsBeingTracked(ultimatePage, this.Tracker))
                    Storage.StartTrackingPage(ultimatePage, this.Tracker);

                //remove succesfully existing page from the untrack victim list
                untrackVictimList.RemoveAll(p=>p.ID == ultimatePage.ID);
            }

            //Make sure Is-Tracked indicators get reflected in the storage
            Storage.SaveIsTrackedIndicators(ultimatePages, this.Tracker);

            //Remove inactual pages from storage.
            foreach (Page inactualPage in untrackVictimList)
                Storage.RemovePageFromTracker(inactualPage, Tracker);

            //Do an extraordinary scan for a refreshed tracker list.
            Scanner.SingleTrackerScan(this.Tracker);
            
            //Fire an event of a commit finish.
            this.FinishCommit();
        }
    }
}
