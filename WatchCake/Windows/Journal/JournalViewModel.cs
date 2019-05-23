using WatchCake.Services;
using System.Text;

namespace WatchCake.ViewModels
{
    /// <summary>
    /// Window ViewModel for the 
    /// </summary>
    public class JournalViewModel : AppViewModel
    {
        /// <summary>
        /// Actual jounral text content instance.
        /// </summary>
        public StringBuilder Text { get; set; } = new StringBuilder();

        /// <summary>
        /// Default constructor, subscribes to a logger broadcaster.
        /// </summary>
        public JournalViewModel()
        {
            Logger.Broadcast += Append;
            Logger.Log("Journal Window started listening Logger.");
        }

        /// <summary>
        /// Add message to the jounral content.
        /// </summary>
        public void Append(object entry)
        {
            string stringed = entry.ToString();

            if (stringed[stringed.Length - 1] != '\n')
                stringed += '\n';

            Text.Insert(0, stringed);

            RaisePropertyChanged(nameof(Text));
        }
    }
}
