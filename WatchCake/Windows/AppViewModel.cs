using WatchCake.Models.Bases;
using System.Windows;
using WatchCake.Services;
using WatchCake.DAL;
using System;

namespace WatchCake.ViewModels
{
    /// <summary>
    /// The base Window ViewModel class with centralized accessors to Storage and Scanner.
    /// </summary>
    public abstract class AppViewModel : IdObject
    {
        /// <summary>
        /// Centralized app UoW-storage access point.
        /// </summary>
        protected readonly Storage Storage = Storage.Instance;

        /// <summary>
        /// WPF app dispatcher invocation shorthand.
        /// </summary>
        protected void Dispatch(Action callback) => Application.Current.Dispatcher.Invoke(callback);

        /// <summary>
        /// Per-window scanner service instance.
        /// </summary>
        protected Scanner Scanner = new Scanner() /*{ PriceFakeness = 0.5 }*/;
    }
}
