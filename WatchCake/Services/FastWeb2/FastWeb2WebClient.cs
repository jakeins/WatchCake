using System;
using System.Net;

namespace WatchCake.Services.FastWebNS
{
    /// <summary>
    /// The custom webclient forcing the protocol verion of 1.0.
    /// </summary>
    [System.ComponentModel.DesignerCategory("Code")]
    public class FastWeb2WebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri uri)
        {
            HttpWebRequest w = (HttpWebRequest)base.GetWebRequest(uri);
            w.ProtocolVersion = Version.Parse("1.0");
            return w;
        }
    }
}