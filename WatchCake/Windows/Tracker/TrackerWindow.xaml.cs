using System.Windows;
using System.Windows.Input;
using WatchCake.Models;
using WatchCake.ViewModels;

namespace WatchCake
{
    /// <summary>
    /// Window for a concrete tracker details.
    /// </summary>
    public partial class TrackerWindow : Window
    {
        /// <summary>
        /// ID of the subject tracker.
        /// </summary>
        public int TrackerID { get; set;}

        /// <summary>
        /// Reference to the "parent" window ViewModel.
        /// </summary>
        public TrackerViewModel TrackerViewModel;

        /// <summary>
        /// Main constructor, receives ID of a tracker to be the subject.
        /// </summary>
        public TrackerWindow(int trackerID)
        {
            TrackerID = trackerID;
            InitializeComponent();
            DataContext = TrackerViewModel = new TrackerViewModel(trackerID, TrackChart);
        }

        /// <summary>
        /// Handler for the table row doubleclick, opens underlying url in OS.
        /// </summary>
        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var uri = ((sender as FrameworkElement)?.DataContext as Option)?.Parent.FullUri;
            System.Diagnostics.Process.Start(uri.ToString());
        }

        /// <summary>
        /// Scan Now button click handler, call for the ViewModel logic.
        /// </summary>
        public void ScanNow(object sender = null, RoutedEventArgs e = null)
        {
            TrackerViewModel.ScanNow();
        }

        /// <summary>
        /// Edit tracker button handler. Opens edit tracker dialog.
        /// </summary>
        private void EditTracker(object sender, RoutedEventArgs e)
        {
            var trackerID = ((sender as FrameworkElement)?.DataContext as Tracker).ID;
            var editTrackerWindow = new TrackerEditWindow(trackerID)
            {
                Owner = this
            };
            editTrackerWindow.ShowDialog();
        }
    }
}
