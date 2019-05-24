using WatchCake.Models;
using System.Collections.ObjectModel;
using WatchCake.Services;
using WatchCake.DAL;
using System.Linq;
using System.Threading.Tasks;
using System;
using WatchCake.Services.Currencier;
using System.Windows;

namespace WatchCake.ViewModels
{
    /// <summary>
    /// ViewModel of the home window
    /// </summary>
    public class HomeViewModel : AppViewModel
    {
        /// <summary>
        /// Reference to the currently selected tracker. Useful for some implementations, like WPF DataGrid.
        /// </summary>
        public Tracker SelectedTracker { get; set; }

        /// <summary>
        /// Observable list of trackers to be shown.
        /// </summary>
        public ObservableCollection<Tracker> TrackersObservable { get; } = new ObservableCollection<Tracker>();

        /// <summary>
        /// Default constructor. VM entry point.
        /// </summary>
        public HomeViewModel()
        {
            //Set necessary dependency to the Currency service
            Currencier.IsFileReadAllowed = (currencyID) =>
            {
                MessageBoxResult dialogResult = MessageBox.Show(
                    $"Try to get all rates from the file?",
                    $"Actual {currencyID} rate is not available",
                    MessageBoxButton.YesNo);

                return dialogResult == MessageBoxResult.Yes;
            };

            InitializeViewModel();  
            
        }

        /// <summary>
        /// Effective start of ViewModel logic: fill window with all trackers, fill their properties.
        /// </summary>
        public void InitializeViewModel()
        {
            FetchTrackerSet();
            Task.Run(() => RefreshTrackersIndicatorsDispatched());
        }

        /// <summary>
        /// Event signalizing about the rescan of all tracekrs from home.
        /// </summary>
        public event Action<int> TrackerRescanned;

        /// <summary>
        /// Does next effective scan of all trackers.
        /// </summary>
        public void DoFullRescan()
        {
            Task.Run(() =>
            {
                Scanner.MultiTrackerScan(TrackersObservable);

                Dispatch(() =>
                {
                    foreach (Tracker tracker in TrackersObservable)
                    TrackerRescanned?.Invoke((int)tracker.ID);
                });
                RefreshTrackersIndicatorsDispatched();
            });
        }

        /// <summary>
        /// Order rescanning of a single provided tracker.
        /// </summary>
        public void RescanSingleTracker(Tracker tracker)
        {
            Task.Run(() =>
            {
                Scanner.SingleTrackerScan(tracker);

                Dispatch(() =>
                {                    
                    TrackerRescanned?.Invoke((int)tracker.ID);
                });
            });            
        }

        /// <summary>
        /// Event signalizing about the removal of a given tracker.
        /// </summary>
        public event Action<int> TrackerRemoving;

        /// <summary>
        /// Removes tracker from storage, refreshes shown list.
        /// </summary>
        public void RemoveTracker(Tracker tracker)
        {
            TrackerRemoving?.Invoke((int)tracker.ID);
            Storage.UnregisterTracker(tracker);
            RefreshTrackersListDispatched();
        }

        /// <summary>
        /// Call every time the trackers list should be updated from the storage (probably updated). 
        /// </summary>
        public void FetchTrackerSet()
        {
            var inactualTrackers = TrackersObservable.ToList();

            foreach (var storedTracker in Storage.Trackers.List())
            {
                bool isNew = TrackersObservable.Count(t => t.ID == storedTracker.ID) < 1;

                if(isNew)
                    TrackersObservable.Add(storedTracker);
                else
                    inactualTrackers.RemoveAll(t => t.ID == storedTracker.ID);
            }

            foreach (Tracker inactualTracker in inactualTrackers)
            {
                TrackersObservable.Remove(inactualTracker);
            }
        }

        /// <summary>
        /// Dispatched trackers list refresh.
        /// </summary>
        public void RefreshTrackersListDispatched()
        {
            Dispatch(() =>
            {
                FetchTrackerSet();
            });

            RefreshTrackersIndicatorsDispatched();
        }

        /// <summary>
        /// Call to refresh indicative values in displaying trackers
        /// </summary>
        void RefreshTrackersIndicatorsDispatched()
        {
            foreach (var obsTracker in TrackersObservable)
            {
                Dispatch(() =>
                {
                    Storage.FillTrackerListProps(obsTracker);
                });                
            }
        }
    }
}
