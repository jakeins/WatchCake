using System;
using System.Windows;
using System.Windows.Input;
using WatchCake.Services;
using WatchCake.ViewModels;
using Page = WatchCake.Models.Page;

namespace WatchCake
{
    /// <summary>
    /// Window of tracker modification.
    /// </summary>
    public partial class TrackerEditWindow : Window
    {
        /// <summary>
        /// Instance of "parent" window WievModel.
        /// </summary>
        TrackerEditViewModel TrackerEditViewModel;

        /// <summary>
        /// Tracker edit window constructor, taking ID of a tracker that gets edited or havinf default null if this is s new tracekr window mode.
        /// </summary>
        public TrackerEditWindow(int? trackerID = null)
        {
            DataContext = TrackerEditViewModel = new TrackerEditViewModel(trackerID);
            InitializeComponent();
            TrackerEditViewModel.FinishCommit += () => FinishCommit();
        }

        /// <summary>
        /// Perform necessary window closing actions: message to the home window about refresh, do a scan in a newly opened tracker details window.
        /// </summary>
        void FinishCommit()
        {
            if (TrackerEditViewModel.Tracker.ID == null)
                throw new NullReferenceException("Cannot succesfully finish not having tracker ID.");

            var homeWindow = Application.Current.MainWindow as HomeWindow;
            homeWindow.RefreshTrackersList();

            if (Owner is TrackerWindow trackerWindow)
                trackerWindow.ScanNow();
            else
                homeWindow.OpenTrackerDetailsWindow((int)TrackerEditViewModel.Tracker.ID);

            this.Close();
        }

        /// <summary>
        /// Clears new page text field.
        /// </summary>
        private void ClearTrackInput(object sender = null, RoutedEventArgs e = null)
        {
            TrackInput.Text = string.Empty;
            Keyboard.Focus(TrackInput);
        }

        /// <summary>
        /// Track It button handler, calls for a underlying logics. 
        /// </summary>
        private void TrackitClick(object sender, RoutedEventArgs e) => TrackitLogic(TrackInput.Text);

        /// <summary>
        /// Starts tracking page by the url provided. Returns boolean success state.
        /// </summary>
        private bool TrackitLogic(string url, bool silentMode = false)
        {
            var result = TrackerEditViewModel.TrackIt(url);
            bool clearTrackInput = false;
            bool offerAlert = true;
            Tuple<string, string> boxMessage = null;

            switch (result)
            {
                case TrackerEditViewModel.TrackStatus.Success:
                    boxMessage = new Tuple<string, string>("Page successfully tracked.", "Success");
                    clearTrackInput = true;
                    offerAlert = false;
                    break;
                case TrackerEditViewModel.TrackStatus.DuplicateFail:
                    boxMessage = new Tuple<string, string>("This page is already being tracked.", "Already known");
                    clearTrackInput = true;
                    offerAlert = false;
                    break;

                case TrackerEditViewModel.TrackStatus.UnknownShop:
                    boxMessage = new Tuple<string, string>($"System is not familiar with the shop by the provided link \n[{url}]\n" +
                        "Check if this shop is supported.", "Unknown shop");
                    break;
                case TrackerEditViewModel.TrackStatus.AccessFail:
                    boxMessage = new Tuple<string, string>("The provided page cannot be retrieved. Check if this page accessible.", "Web access failure");
                    break;
                case TrackerEditViewModel.TrackStatus.ContentFail:
                    boxMessage = new Tuple<string, string>("The content of the retrieved page cannot be resolved. Check if this is a product end page.\n If you're tracking 1 option-per-page shop, make sure this is the selected option link.", "Retrieved content failure");
                    break;

                default:
                    boxMessage = new Tuple<string, string>("Something unpredicted has happened.", "Unhandeled error");
                    break;
            }

            if (offerAlert && !silentMode)
                MessageBox.Show(boxMessage.Item1, boxMessage.Item2);
            else
                Logger.Log(boxMessage.Item2 + ": " + boxMessage.Item1);

            if (clearTrackInput)
            {
                ClearTrackInput();
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Opens provided string url in a default OS environment.
        /// </summary>
        void OpenLink(string uri) => System.Diagnostics.Process.Start(uri);

        /// <summary>
        /// Row doubleclick hanndler, opens hyperlink of the corresponding page.
        /// </summary>
        private void DataGridRow_MouseDoubleClick(object sender, EventArgs e)
        {
            var uri = ((sender as FrameworkElement)?.DataContext as Page)?.FullUri.ToString();
            OpenLink(uri);
        }

        /// <summary>
        /// Handler of input link, ensures protocol scheme presence for futher url consistency.
        /// </summary>
        private void FollowLink(object sender, EventArgs e)
        {
            var prelink = TrackInput.Text;

            if (!prelink.StartsWith("http"))
                prelink = "http" + "://" + prelink;

            OpenLink(prelink);
        }

        /// <summary>
        /// Opens page of the selected-context-menu row.
        /// </summary>
        private void ContextMenuOpenLink(object sender, RoutedEventArgs e)
        {
            var uri = TrackerEditViewModel.SelectedPage.FullUri.ToString();
            OpenLink(uri);
        }

        /// <summary>
        /// Context menu 'use as title' handler, sets tracker title as per selected row page.
        /// </summary>
        private void UseAsATitle(object sender, RoutedEventArgs e)
        {
            TrackerEditViewModel.Tracker.Title = TrackerEditViewModel.SelectedPage.Title;
        }

        /// <summary>
        /// Context menu 'remove page' handler, has prompt in the edit mode, calls for underlying logic.
        /// </summary>
        private void RemoveSelectedPage(object sender, EventArgs e)
        {
            if (TrackerEditViewModel.NewTrackerMode ||  MessageBox.Show("Are you sure about removing this page from tracker altogether? Consider stopping tracking to save its history.", "Warning!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                TrackerEditViewModel.LocalRemovePageFromTracking(TrackerEditViewModel.SelectedPage);
        }

        /// <summary>
        /// Calls for a page deleting logic following the press of a key (Del).
        /// </summary>
        private void PagesGridRow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
                RemoveSelectedPage(sender, null);
        }

        /// <summary>
        /// Track-It functionality trigger, by handling key event (Ctrl-V).
        /// </summary>
        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if(Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.V)
                TrackitLogic(TrackInput.Text, silentMode:true);
        }

        /// <summary>
        /// Track-it functionality trigger, handles (drag-and-)drop event. Call for tracking logic, prevent event propagation in case of success to avoid default windows drag-and-drop logic.
        /// </summary>
        private void AppWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.Text) is string str)
            {
                var result = TrackitLogic(str, silentMode: true);

                if(result)
                    e.Handled = true;
            }
        }

        /// <summary>
        /// Handler of modificcation finish button. Calls for the underlying finishing logic.
        /// </summary>
        private void CommitTracker(object sender, RoutedEventArgs e) => TrackerEditViewModel.CommitTracker();        
    }
}
