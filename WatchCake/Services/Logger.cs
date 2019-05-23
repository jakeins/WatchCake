using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace WatchCake.Services
{
    /// <summary>
    /// Centralized journaling service.
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Event signalizing about new entry, providing it.
        /// </summary>
        public static event Action<object> Broadcast;

        /// <summary>
        /// Buffer of a journal used for conditions where it is not yet listened.
        /// </summary>
        static Queue<string> Buffer = new Queue<string>();

        /// <summary>
        /// Static constructor.
        /// </summary>
        static Logger()
        {
            Log("Logger initialized.");
        }

        /// <summary>
        /// New logging entry taking method.
        /// </summary>
        public static void Log(object entry)
        {
            Buffer.Enqueue("• " + DateTime.Now + " ▷ " + entry);

            if (Broadcast?.GetInvocationList().Length > 0)
                while(Buffer.Count > 0)
                    Broadcast?.Invoke(Buffer.Dequeue());

            Console.WriteLine(entry);
        }
    }
}
