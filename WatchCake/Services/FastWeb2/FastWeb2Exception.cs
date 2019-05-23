using System;
using System.Net;

namespace WatchCake.Services.FastWebNS
{
    /// <summary>
    /// General FastWeb2 Exception.
    /// </summary>
    public class FastWeb2Exception : WebException
    {
        public FastWeb2Exception(string msg) : base(msg)
        {
        }

        public FastWeb2Exception(string msg, Exception innerException) : base(msg, innerException)
        {
        }
    }
}
