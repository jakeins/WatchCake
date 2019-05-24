using System;
using System.Windows;
using WatchCake.Models;
using WatchCake.ViewModels;

namespace WatchCake
{
    /// <summary>
    /// Main / Home window of the app.
    /// </summary>
    public partial class HomeWindow : Window
    {
        /// <summary>
        /// Reference to the "parent" windows ViewModel.
        /// </summary>
        HomeViewModel HomeViewModel;

        /// <summary>
        /// Reference to the Jounral window, needed to keep it always alive.
        /// </summary>
        JournalWindow JournalWindow;

        /// <summary>
        /// Default home window constructor. Instantiates ViewModel and does event subsriptions.
        /// </summary>
        public HomeWindow()
        {
            DataContext = HomeViewModel = new HomeViewModel();
            InitializeComponent();

            //Own method is being subscribed to own event to perform post-init actions after window initialization is ended for sure.
            Activated += HomeWindow_Activated;
        }

        /// <summary>
        /// Handler passing control to the actual post-init actions method.
        /// </summary>
        private void HomeWindow_Activated(object sender, EventArgs e) => PostStartActions();

        /// <summary>
        /// The action necessary after full window initialization. E.g. setting child Journal window owner is possible only having owner fully initialized.
        /// </summary>
        void PostStartActions()
        {
            JournalWindow = new JournalWindow() { Owner = this };
            Activated -= HomeWindow_Activated;
        }

        /// <summary>
        /// Refresh trackers in a window list. Passes control over ViewModel fucntionality.
        /// </summary>
        public void RefreshTrackersList() => HomeViewModel.RefreshTrackersListDispatched();

        /// <summary>
        /// Table row doubleclick handler. Calls for tracker details window opening.
        /// </summary>
        private void DataGridRow_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            int? trackerID = ((sender as FrameworkElement)?.DataContext as Tracker)?.ID;

            if (trackerID == null)
                throw new NullReferenceException("Tracker id cannot be null.");

            OpenTrackerDetailsWindow((int)trackerID);
        }

        /// <summary>
        /// Logic for opening/activation of the requested tracker details window.
        /// </summary>
        public void OpenTrackerDetailsWindow(int trackerID)
        {
            TrackerWindow openedTrackerWindow = null;

            //try to find this tracker details window among opened ones.
            foreach (Window window in OwnedWindows)
            {
                if (window is TrackerWindow trackerWindow && trackerWindow.TrackerID == trackerID)
                {
                    openedTrackerWindow = trackerWindow;
                    break;
                }
            }

            if (openedTrackerWindow != null)//having found opened, just activate
                openedTrackerWindow.Activate();
            else//without already opened, instantiate new details window
            {
                //create fresh window
                var freshWindow = new TrackerWindow(trackerID)
                {
                    Owner = this
                };

                //subsribe the reload method of a new window to the main window rescan event, so it reloads as well
                HomeViewModel.TrackerRescanned += (int tID) => 
                {
                    if (tID == trackerID)
                        freshWindow.TrackerViewModel.ReloadIndicatorsDispatched();
                };

                //subsribe the close method of a new window to the main window tracker-removing event, so the removed tracker gets closed
                HomeViewModel.TrackerRemoving += (int tID) =>
                {
                    if (tID == trackerID)
                        freshWindow.Close();
                };

                freshWindow.Show();
            }
        }

        /// <summary>
        /// Instantiate and show new tracker widnow (as dialog).
        /// </summary>
        private void NewTracker(object sender, RoutedEventArgs e)
        {
            var newTrackerWindow = new TrackerEditWindow()
            {
                Owner = this
            };
            newTrackerWindow.ShowDialog();
        }

        /// <summary>
        /// Handle Context-Menu-Details click and pass control to open window logic.
        /// </summary>
        private void ContextMenuTrackerDetails(object sender, RoutedEventArgs e)
        {
            int? trackerID = HomeViewModel.SelectedTracker.ID;

            if (trackerID == null)
                throw new NullReferenceException("Tracker id cannot be null.");

            OpenTrackerDetailsWindow((int)trackerID);
        }

        /// <summary>
        /// Handle Contexet-Menu-Rescan-This-Tracker click by calling ViewModel functionality.
        /// </summary>
        private void ContextMenuTrackerRescan(object sender, RoutedEventArgs e)
        {
            HomeViewModel.RescanSingleTracker(HomeViewModel.SelectedTracker);
        }

        /// <summary>
        /// Handle Context-Menu-Remove-Tracker click by calling ViewModel functionality through are-u-sure prompt.
        /// </summary>
        private void ContextMenuTrackerRemove(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure about removing this tracker?", "Warning!",MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                HomeViewModel.RemoveTracker(HomeViewModel.SelectedTracker);
        }

        /// <summary>
        /// Handle Contexet-Menu-Rescan-All click by calling ViewModel functionality.
        /// </summary>
        private void FullRescanClick(object sender, RoutedEventArgs e)
        {
            HomeViewModel.DoFullRescan();                
        }

        /// <summary>
        /// Shows instantiated Journal window.
        /// </summary>
        private void OpenJournal(object sender = null, RoutedEventArgs e = null)
        {
            JournalWindow.Show();
        }
    }
}