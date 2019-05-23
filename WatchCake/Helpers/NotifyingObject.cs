using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WatchCake.Helpers
{
    /// <summary>
    /// Base object having implementation of INotifyPropertyChanged interface for its members.
    /// </summary>
    public abstract class NotifyingObject : INotifyPropertyChanged
    {
        /// <summary>
        /// Event used by WPF.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Trigger event on effective change.
        /// </summary>
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
