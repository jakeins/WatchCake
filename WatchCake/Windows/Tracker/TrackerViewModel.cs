using WatchCake.Models;
using WatchCake.Services;
using WatchCake.DAL;
using System.Threading.Tasks;
using System.Windows.Controls.DataVisualization.Charting;
using System.Collections.Generic;
using System;

namespace WatchCake.ViewModels
{
    /// <summary>
    /// Window ViewModel of concrete tracker details.
    /// </summary>
    public class TrackerViewModel : AppViewModel
    {
        /// <summary>
        /// Subject tracker reference.
        /// </summary>
        public Tracker Tracker { get; set; }

        /// <summary>
        /// Chart cotnrol for option-snapshots access point.
        /// </summary>
        public Chart TrackChart { get; set; }

        /// <summary>
        /// Dynamic window title property.
        /// </summary>
        public string Title => $"#{Tracker.ID} {Tracker.Title} - WatchCake";

        /// <summary>
        /// Main constructor of the window ViewModel. Receives subject tracker ID, and early trackChart setter to have it populated from the beginning.
        /// </summary>
        public TrackerViewModel(int trackerID, Chart trackChart)
        {
            TrackChart = trackChart;
            InitializeViewModel(trackerID);
        }

        /// <summary>
        /// Effective initializer of hte viewmodel. Gets tracker from the storage, populates it, sets 1st chart state.
        /// </summary>
        public void InitializeViewModel(int trackerID)
        {
            Tracker = Storage.Trackers[trackerID];
            Storage.FillTrackerDetailsProps(Tracker);
            ResetChartData();
        }

        /// <summary>
        /// Does a parallel scan of a subject tracker, repopulates properties (dispatched). 
        /// </summary>
        public void ScanNow()
        {
            Task.Run(() => {
                Scanner.SingleTrackerScan(Tracker);
                ReloadIndicatorsDispatched();
            });
        }

        /// <summary>
        /// Repopulate trackers data parallel & dispatched wrapper.
        /// </summary>
        public void ReloadIndicatorsDispatched()
        {
            Task.Run(() => {
                Dispatch(() =>
                {
                    ReloadIndicatorsPlain();
                });
            });
        }

        /// <summary>
        /// Synchronously repopulate tracker indicative properties.
        /// </summary>
        private void ReloadIndicatorsPlain()
        {
            Storage.FillTrackerDetailsProps(Tracker);
            ResetChartData();
            Storage.FillTrackerListProps(Tracker);
        }

        /// <summary>
        /// Recalculate and write snapshots chart data.
        /// </summary>
        void ResetChartData()
        {
            TrackChart.Series.Clear();

            foreach (Option option in Tracker.Options)
            {
                List<KeyValuePair<DateTime, decimal>> pointCollection = new List<KeyValuePair<DateTime, decimal>>();

                foreach (Snapshot snapshot in option.Snapshots)
                    pointCollection.Add(new KeyValuePair<DateTime, decimal>(snapshot.Timestamp, snapshot.Price.Amount));

                LineSeries serie = new LineSeries
                {
                    Title = $"{option.Parent.ParentShop.ID}-{option.Parent.ID}-{option.ID}",
                    DependentValuePath = "Value",
                    IndependentValuePath = "Key",
                    ItemsSource = pointCollection
                };

                TrackChart.Series.Add(serie);
            }
                      
        }
    }
}
