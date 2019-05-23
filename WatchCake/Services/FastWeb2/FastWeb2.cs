using System;
using System.IO;
using WatchCake.Helpers;
using System.Net;
using System.Threading;
using System.Collections.Generic;
using System.Text;

namespace WatchCake.Services.FastWebNS
{
    /// <summary>
    /// Infrastructure-level service, that can be instanced, for getting the web pages content in a fastest way possible.
    /// </summary>
    class FastWeb2
    {
        /// <summary>
        /// Delegate that gets invoked on all reporting occasions.
        /// </summary>
        public Action<object> UsualReporter = (c) => { Logger.Log(c); };

        /// <summary>
        /// Delegate that gets invoked only on critical reporting occasions.
        /// </summary>
        public Action<object> CriticalReporter = (c) => { Logger.Log(c); };

        /// <summary>
        /// Domain to which this instance is setup to.
        /// </summary>
        public Uri Domain;

        /// <summary>
        /// Indication of knowing about sites enabled protection mode.
        /// </summary>
        public bool ProtectionTriggered = false;

        /// <summary>
        /// Limit of file cache validity.
        /// </summary>
        public TimeSpan CacheLimit = TimeSpan.FromMinutes(60 * 12);

        /// <summary>
        /// The number of retirs that FastWeb2 should perform before reving out the unsuccessfull result.
        /// </summary>
        public uint WebAccessRetries = 10;

        TimeSpan accessDelay;
        /// <summary>
        /// Current delay betwee web access retries.
        /// </summary>
        public TimeSpan AccessDelay
        {
            get => accessDelay;
            set => accessDelay =  value < TimeSpan.Zero ? TimeSpan.Zero : value;      
        }

        /// <summary>
        /// The step that is used to modify the web acess delay for every occasion.
        /// </summary>
        public TimeSpan RetryDelayStep = TimeSpan.FromMilliseconds(200); 

        /// <summary>
        /// Virtuacl cache of already accessed pages.
        /// [relativeUri] => {savetime,content}
        /// </summary>
        readonly Dictionary<Uri, Tuple<DateTime, string>> InMemoryCache = new Dictionary<Uri, Tuple<DateTime, string>>();

        /// <summary>
        /// Delegate providing possibility to cancel actions from outside.
        /// </summary>
        public Action DoGlobalCancel = () => { };

        
        string cacheFolder;
        /// <summary>
        /// File folder fot hte web cache.
        /// </summary>
        public string CacheFolder
        {
            get => cacheFolder;
            set
            {
                cacheFolder = value;
                if(!Directory.Exists(cacheFolder))
                    Directory.CreateDirectory(cacheFolder);
            }
        }

        /// <summary>
        /// Main constructor.
        /// </summary>
        /// <param name="domain"></param>
        public FastWeb2(Uri domain)
        {
            Domain = domain;
            CacheFolder = "webcache";
            AccessDelay = RetryDelayStep;
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
        }

        /// <summary>
        /// String constructor.
        /// </summary>
        public FastWeb2(string domainString) : this(new Uri(domainString))
        {
        }

        #region Web Client Implementation

        /// <summary>
        /// WebClinet instance.
        /// </summary>
        static readonly WebClient webClient = new FastWeb2WebClient
        {
            Encoding = Encoding.UTF8
        };

        /// <summary>
        /// Local library abstraction
        /// </summary>
        string DownloadStringLib(Uri address)
        {
            return webClient.DownloadString(address);
        }

        #endregion Web Client Implementation
        /// <summary>
        /// Try the donwload of the provided page, respecting defined access rules.
        /// </summary>
        /// <param name="fullUri"></param>
        /// <returns></returns>
        string AggressiveDownload(Uri fullUri)
        {
            string methodID = "FW::AggrDwnld";

            string inReportingID = fullUri.ToString();

            var retriesMade = 0;

            while (true)
            {
                try
                {
                    Thread.Sleep(AccessDelay);

                    Report(methodID, inReportingID, $"Donwload try #{retriesMade + 1}");

                    string content = DownloadStringLib(fullUri);

                    if (string.IsNullOrEmpty(content))
                        throw new InvalidDataException("Got empty result from the server.");

                    AccessDelay -= RetryDelayStep;

                    return content;
                }
                catch (Exception ex) when (
                                    ex is WebException ||
                                    ex is InvalidDataException ||
                                    ex is TimeoutException)
                {
                    if (((ex as WebException)?.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.NotFound)
                    {
                        var reason = $"Requested page is not found.";
                        Report(methodID, inReportingID, $"Server responded with 404 Not Found.");
                        throw new FastWeb2NotFoundException(reason, ex);
                    }                        

                    Report(methodID, inReportingID, $"Donwload try #{retriesMade + 1} gave an exception: \n{ex.GetType().Name}: {ex.Message} {ex.InnerException?.Message}");
                    if (retriesMade + 1 < WebAccessRetries)
                    {
                        retriesMade++;
                        AccessDelay += RetryDelayStep;
                        Report(methodID, inReportingID, $"Going for donwload retry #{retriesMade} with {AccessDelay.TotalMilliseconds / 1000D}s delay.");
                        continue;
                    }
                    else
                    {
                        var reason = $"{retriesMade + 1} download tries were unsuccessfull. Last exception: \n  {ex.GetType().Name}: {ex.Message}";
                        //CriticalReport(methodID, inReportingID, reason);
                        throw new FastWeb2Exception(reason, ex);
                    }
                }
            }
        }

        /// <summary>
        /// Gets html source for a specified relative uri in a fastest way possible. Returns meaningful content or throws 
        /// </summary>
        public string Get(string inputUri) => Get(new Uri(inputUri, UriKind.Relative));

        /// <summary>
        /// Gets html source for a specified relative uri in a fastest way possible. Returns meaningful content or throws 
        /// </summary>
        public string Get(Uri inputUri = null)
        {
            string methodID = "FW2.Get";

            Uri fullUri;

            switch (inputUri?.IsAbsoluteUri)
            {
                case true:
                    fullUri = inputUri;
                    break;
                case false:
                    fullUri = new Uri(Domain.SlashSafeUriConcat(inputUri));
                    break;
                default:
                    fullUri = Domain;
                    break;
            }

            string reportingID = fullUri.ToString();
            string content = null;

            content = TryGetFromMemory(fullUri);

            if (content != null)
            {
                Report(methodID, reportingID, "Returning content from memory.");
                return content;
            }

            content = TryGetFromFile(fullUri);

            if (content != null)
            {
                Report(methodID, reportingID, "Returning content from local file.");
                SaveToMemory(fullUri, content, new FileInfo(CachePath(fullUri)).LastWriteTime);
                return content;
            }

            Report(methodID, reportingID, "Download is necessary.");

            if (ProtectionTriggered)
            {
                var reason = "Download failed because site protection is triggered.";
                Report(methodID, reportingID, reason);

                throw new FastWeb2Exception(reason);
            }

            Report(methodID, reportingID, $"Downloading {fullUri} …");

            content = AggressiveDownload(fullUri);

            if (content == null)
            {
                var reason = "Download failed.";
                Report(methodID, reportingID, reason);
                throw new FastWeb2Exception(reason);
            }
            else
            {
                Report(methodID, reportingID, "Downloaded successfully.");
                SaveToMemory(fullUri, content);
                SaveToFile(fullUri, content);

                return content;
            }
        }

        /// <summary>
        /// Get page content from memory cache.
        /// </summary>
        string TryGetFromMemory(Uri fullUri)
        {
            string content = null;
            if (InMemoryCache.ContainsKey(fullUri))
            {
                if (InMemoryCache[fullUri].Item1.Add(CacheLimit) > DateTime.Now)
                    content = InMemoryCache[fullUri].Item2;
                else
                    InMemoryCache.Remove(fullUri);
            }
            return content;
        }

        /// <summary>
        /// Get page content from file cache.
        /// </summary>
        string TryGetFromFile(Uri fullUri)
        {
            string content = null;
            string cachePath = CachePath(fullUri);

            var x = File.Exists(cachePath);

            if (File.Exists(cachePath))
            {
                var fileInfo = new FileInfo(cachePath);

                if (fileInfo.Length > 0 && fileInfo.LastWriteTime.Add(CacheLimit) > DateTime.Now)
                    content = File.ReadAllText(cachePath);
                else
                    File.Delete(cachePath);
            }
            return content;
        }

        /// <summary>
        /// Save page to memory cache.
        /// </summary>
        void SaveToMemory(Uri fullUri, string content, DateTime? inputTime = null)
        {
            DateTime writeTime = inputTime ?? DateTime.Now;
            InMemoryCache[fullUri] = new Tuple<DateTime, string>(writeTime, content);
        }

        /// <summary>
        /// Save page to file cache.
        /// </summary>
        void SaveToFile(Uri fullUri, string content)
        {
            File.WriteAllText(CachePath(fullUri), content, Encoding.UTF8);
        }

        /// <summary>
        /// Form file cache filesystem path using provided url.
        /// </summary>
        string CachePath(Uri fullUri)
        {
            return CacheFolder + @"\" + StringStuff.Uri2filename(fullUri) + ".html";
        }

        /// <summary>
        /// Do a usual reporting.
        /// </summary>
        void Report(string inCallerName, string inReportId, string inMessage)
        {
            UsualReporter($"{inMessage}  @ {inReportId} @ {inCallerName}");
        }

        /// <summary>
        /// Do a critical reporting.
        /// </summary>
        void CriticalReport(string inCallerName, string inReportId, string inMessage)
        {
            Report(inCallerName, inReportId, inMessage);
            CriticalReporter($"{inMessage}  @ {inReportId} @ {inCallerName}");
        }
    }
}
