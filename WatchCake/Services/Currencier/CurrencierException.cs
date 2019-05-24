using System;
using System.Net;

namespace WatchCake.Services.FastWebNS
{
    /// <summary>
    /// General Currencier Exception.
    /// </summary>
    public class CurrencierException : WebException
    {
        public CurrencierException(string msg) : base(msg)
        {
        }

        public CurrencierException(string msg, Exception innerException) : base(msg, innerException)
        {
        }
    }
}
