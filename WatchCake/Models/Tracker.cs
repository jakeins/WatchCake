using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using WatchCake.Helpers;
using WatchCake.Models.Bases;

namespace WatchCake.Models
{
    /// <summary>
    /// Tracker, the entity that holds a number of pages that should be tracker comparatively on the another.
    /// </summary>
    public class Tracker : IdObject
    {
        /// <summary>
        /// Display name that is actually set explicity.
        /// </summary>
        public string trueTitle;

        /// <summary>
        /// Dynamic display name. Can be automatic or manual.
        /// </summary>
        public string Title
        {
            get => trueTitle ?? "Untitled Tracker" + (ID != null ? " #" + ID : null);
            set { trueTitle = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// Fixed time of tracker creation
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Default constructor, sets timestamp as now. In case of automatic reconstitution, it gets overridden right after.
        /// </summary>
        public Tracker()
        {
            Timestamp = DateTime.Now;
        }

        #region HomeWindowProps
        private int? _manualPageNumber;
        /// <summary>
        /// Manually set page number indicator for optimized statistics (at home win, for example).
        /// </summary>
        [NotMapped]
        public int? ManualPageNumberIndicator { get => _manualPageNumber; set { _manualPageNumber = value; RaisePropertyChanged(); } }

        private Money _lowestPriceEver;
        /// <summary>
        /// Record lowest price for option in this tracker.
        /// </summary>
        [NotMapped]
        public Money LowestPriceEver { get => _lowestPriceEver; set { _lowestPriceEver = value; RaisePropertyChanged(); } }

        private Money _averagePriceEver;
        /// <summary>
        /// Average price for option in this tracker.
        /// </summary>
        [NotMapped]
        public Money AveragePriceEver { get => _averagePriceEver; set { _averagePriceEver = value; RaisePropertyChanged(); } }

        private Money _todayPrice;
        /// <summary>
        /// Today price for option in this tracker.
        /// </summary>
        [NotMapped]
        public Money TodayPrice { get => _todayPrice; set { _todayPrice = value; RaisePropertyChanged(); } }

        private decimal? _priceDynamics;
        /// <summary>
        /// Price dynamics, indicated the extent to which today price differs from average.
        /// </summary>
        [NotMapped]
        public decimal? PriceDynamics { get => _priceDynamics; set { _priceDynamics = value; RaisePropertyChanged(); } }

        #endregion HomeWindowProps

        #region Tracker Window Props
        ObservableCollection<Page> _pages;
        [NotMapped]
        public ObservableCollection<Page> Pages
        {
            get => _pages;
            set
            {
                if(_pages != null)
                    _pages.CollectionChanged -= RaisePageCountChanged;

                _pages = value;
                RaisePropertyChanged();

                if (_pages != null)
                    _pages.CollectionChanged += RaisePageCountChanged;
            }
        }

        /// <summary>
        /// Signalize about possible page-count change.
        /// </summary>
        void RaisePageCountChanged(object sender, NotifyCollectionChangedEventArgs e) => RaisePropertyChanged(nameof(NotifyingPageCount));

        /// <summary>
        /// Pages count, that is notifiable.
        /// </summary>
        public int NotifyingPageCount => Pages == null ? 0 : Pages.Count;

        /// <summary>
        /// Check whether tracker already keeps locally (not in storage) the page, similar to the provided one.
        /// </summary>
        public bool HasLocalDuplicates(Page page) => Pages.Any(p => p.FullUri.StripTrailingSlashes() == page.FullUri.StripTrailingSlashes());
               
        private IEnumerable<Option> _options;

        /// <summary>
        /// The set of options associated with the current tracker.
        /// </summary>
        [NotMapped]
        public IEnumerable<Option> Options { get => _options; set { _options = value; RaisePropertyChanged(); } }
        #endregion Tracker Window Props
    }
}
