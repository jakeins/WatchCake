using System.Windows;
using WatchCake.ViewModels;

namespace WatchCake
{
    /// <summary>
    /// Window for showing program log.
    /// </summary>
    public partial class JournalWindow : Window
    {
        /// <summary>
        /// Reference to the "parent" window view model.
        /// </summary>
        public JournalViewModel JournalViewModel;

        /// <summary>
        /// Default constructor, initializes, sets viewmodel, subsribes closing substitute.
        /// </summary>
        public JournalWindow()
        {
            InitializeComponent();
            DataContext = JournalViewModel = new JournalViewModel();
            this.Closing += JournalWindow_Closing;
        }

        /// <summary>
        /// Handler that substitutes closing with hiding.
        /// </summary>
        private void JournalWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }
}
