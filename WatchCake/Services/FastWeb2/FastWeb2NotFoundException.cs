using System;

namespace WatchCake.Services.FastWebNS
{
    /// <summary>
    /// Specific FastWeb2 Exception indicating the 404 Not Found web response.
    /// </summary>
    public class FastWeb2NotFoundException : FastWeb2Exception
    {
        public FastWeb2NotFoundException(string msg) : base(msg)
        {
        }

        public FastWeb2NotFoundException(string msg, Exception innerException) : base(msg, innerException)
        {
        }
    }
}
